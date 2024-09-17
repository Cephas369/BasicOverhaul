// Decompiled with JetBrains decompiler
// Type: TaleWorlds.CampaignSystem.ViewModelCollection.Party.PartyVM
// Assembly: TaleWorlds.CampaignSystem.ViewModelCollection, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C2B4842E-B3B8-46FE-A96B-C6CECFB63981
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.CampaignSystem.ViewModelCollection.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using BasicOverhaul.Patches;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.GUI
{
  public class BOInventoryVM : SPInventoryVM
  {
    private static Dictionary<Side, Dictionary<FilterType, PartyFilterControllerVM>> _dataSources;
      
    private static Dictionary<FilterType, Delegate> _filterMethods = new()
    {
      { FilterType.Tier, null },
      { FilterType.Type, null },
      { FilterType.Culture, null },
      { FilterType.Query, null }
    };
    public static BOInventoryVM? Instance { get; private set; }
    
    public BOInventoryVM(InventoryLogic inventoryLogic, bool isInCivilianModeByDefault, Func<WeaponComponentData,
        ItemObject.ItemUsageSetFlags> getItemUsageSetFlags, string fiveStackShortcutkeyText, string entireStackShortcutkeyText) : base(inventoryLogic, isInCivilianModeByDefault, getItemUsageSetFlags, fiveStackShortcutkeyText, entireStackShortcutkeyText)
    {
      SetGetKeyTextFromKeyIDFunc(Game.Current.GameTextManager.GetHotKeyGameTextFromKeyID);
      SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
      SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
      SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
      SetPreviousCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
      SetNextCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
      SetBuyAllInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("TakeAll"));
      SetSellAllInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("GiveAll"));
      
      Instance = this;
      Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = InitializeFilters();
      _dataSources = new();
      for (int i = 0; i < 2; i++)
      {
        Side side = i == 0 ? Side.Left : Side.Right;
        _dataSources.Add(side, new());

        foreach (var filter in filters)
        {
          var partyFilter = new PartyFilterControllerVM(side, OnFilterChange, filter.Value, filter.Key);
          
          _dataSources[side].Add(filter.Key, partyFilter);
        }
      }
    }
    
    public override void OnFinalize()
    {
      base.OnFinalize();
      Instance = null;
      _filterMethods = new()
      {
        { FilterType.Tier, null },
        { FilterType.Type, null },
        { FilterType.Culture, null },
        { FilterType.Query, null },
      };
      _dataSources = null;
    }
    private void Reset(Side side)
    {
      MBBindingList<SPItemVM> items = GetItemListBySide(side);
      foreach (var itemVm in items)
        itemVm.IsFiltered = false;
    }
    private void OnFilterChange(Side side, (FilterType filterType, object selected) selected)
    {
      Reset(side);
      if (selected.selected is string text && text == "all")
      {
        _filterMethods[selected.filterType] = null;
      }
      else
      {
        MBBindingList<SPItemVM> items = GetItemListBySide(side);
        switch (selected.filterType)
        {
          case FilterType.Type:
            ItemObject.ItemTypeEnum itemType = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), (string)selected.selected);
            _filterMethods[selected.filterType] = () =>
            {
              for (int i = items.Count - 1; i >= 0; i--)
              {
                if (items[i].TypeId != (int)itemType)
                  items[i].IsFiltered = true;
              }
            };
            break;

          case FilterType.Tier:
            int tier = (int)selected.selected;
            _filterMethods[selected.filterType] = () =>
            {
              for (int i = items.Count - 1; i >= 0; i--)
              {
                if ((int)items[i].ItemRosterElement.EquipmentElement.Item?.Tier != tier)
                  items[i].IsFiltered = true;
              }
            };
            break;
          
          case FilterType.Culture:
            string culture = (string)selected.selected;
            _filterMethods[selected.filterType] = () =>
            {
              for (int i = items.Count - 1; i >= 0; i--)
              {
                if ((items[i].ItemRosterElement.EquipmentElement.Item?.Culture?.StringId ?? "none") != culture)
                  items[i].IsFiltered = true;
              }
            };
            break;
        }
      }

      foreach (var method in _filterMethods)
        if(method.Value != null)
          method.Value.DynamicInvoke();

      RefreshValues();
    }

    public override void RefreshValues()
    {
      base.RefreshValues();
      ButtonOkLabel = new TextObject("{=bo_search}Search").ToString();
    }

    private void ExecuteSearchActionLeft() => ShowSearchInquiry(Side.Left);
    private void ExecuteSearchActionRight() => ShowSearchInquiry(Side.Right);

    private void ShowSearchInquiry(Side side)
    {
      InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=bo_search_title}Search item").ToString(), 
      new TextObject("{=bo_search_desc}Leave empty to reset the query").ToString(), true, false, 
      new TextObject("{=bo_search}Search").ToString(), null, query => FilterByQuery(query, side), null), true);
    }

    private void FilterByQuery(string query, Side side)
    {
      Reset(side);
      if (query == null || query.Length < 1)
      {
        _filterMethods[FilterType.Query] = null;
      }
      else
      {
        MBBindingList<SPItemVM> items = GetItemListBySide(side);
        _filterMethods[FilterType.Query] = () =>
        {
          for (int i = items.Count - 1; i >= 0; i--)
          {
            if (items[i].ItemRosterElement.EquipmentElement.Item?.Name.ToString().ToLower().Contains(query.ToLower()) == false)
              items[i].IsFiltered = true;
          }
        };
      }
      
      foreach (var method in _filterMethods)
        if(method.Value != null)
          method.Value.DynamicInvoke();

      RefreshValues();
    }
    private MBBindingList<SPItemVM> GetItemListBySide(Side side) => side == Side.Left ? LeftItemListVM : RightItemListVM;
    private Dictionary<FilterType, List<TroopFilterSelectorItemVM>> InitializeFilters()
    {
      Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = new()
      {
        { FilterType.Type, new() },
        { FilterType.Tier, new() },
        { FilterType.Culture, new() }
      };
        
      filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_types}All Types"), FilterType.Type, "all"));
      filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_tiers}All Tiers"), FilterType.Tier, "all"));
      
      filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_cultures}All Cultures"), FilterType.Culture, "all"));
      filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(new TextObject("{=no_culture}No Culture"), FilterType.Culture, "none"));

      for (int i = 1; i <= 24; i++)
      {
        filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(GameTexts.FindText("str_inventory_type_" + i),
          FilterType.Type, ((ItemObject.ItemTypeEnum)i).ToString()));
      }

      for (int i = 0; i <= 5; i++)
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

      var allCultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
      foreach (var culture in allCultures)
      { 
        filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(culture.GetName(), FilterType.Culture, culture.StringId));
      }

      return filters;
    }

    [DataSourceProperty]
    public PartyFilterControllerVM FilterTierLeft
    {
      get => _dataSources[Side.Left][FilterType.Tier];
      set
      {
        if (value == _dataSources[Side.Left][FilterType.Tier])
          return;
        _dataSources[Side.Left][FilterType.Tier] = value;
        OnPropertyChangedWithValue(value);
      }
    }
    
    [DataSourceProperty]
    public PartyFilterControllerVM FilterTypeLeft
    {
      get => _dataSources[Side.Left][FilterType.Type];
      set
      {
        if (value == _dataSources[Side.Left][FilterType.Type])
          return;
        _dataSources[Side.Left][FilterType.Type] = value;
        OnPropertyChangedWithValue(value);
      }
    }
    
    [DataSourceProperty]
    public PartyFilterControllerVM FilterCultureLeft
    {
      get => _dataSources[Side.Left][FilterType.Culture];
      set
      {
        if (value == _dataSources[Side.Left][FilterType.Culture])
          return;
        _dataSources[Side.Left][FilterType.Culture] = value;
        OnPropertyChangedWithValue(value);
      }
    }

    [DataSourceProperty]
    public PartyFilterControllerVM FilterTierRight
    {
      get => _dataSources[Side.Right][FilterType.Tier];
      set
      {
        if (value == _dataSources[Side.Right][FilterType.Tier])
          return;
        _dataSources[Side.Right][FilterType.Tier] = value;
        OnPropertyChangedWithValue(value);
      }
    }
    
    [DataSourceProperty]
    public PartyFilterControllerVM FilterTypeRight
    {
      get => _dataSources[Side.Right][FilterType.Type];
      set
      {
        if (value == _dataSources[Side.Right][FilterType.Type])
          return;
        _dataSources[Side.Right][FilterType.Type] = value;
        OnPropertyChangedWithValue(value);
      }
    }
    
    [DataSourceProperty]
    public PartyFilterControllerVM FilterCultureRight
    {
      get => _dataSources[Side.Right][FilterType.Culture];
      set
      {
        if (value == _dataSources[Side.Right][FilterType.Culture])
          return;
        _dataSources[Side.Right][FilterType.Culture] = value;
        OnPropertyChangedWithValue(value);
      }
    }
    
    private string _buttonOkLabel;
    
    [DataSourceProperty]
    public string ButtonOkLabel
    {
      get => _buttonOkLabel;
      set
      {
        if (value == _buttonOkLabel) 
          return;
        
        _buttonOkLabel = value;
        OnPropertyChangedWithValue(value);
      }
    }
  }
}
