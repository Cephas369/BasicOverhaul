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
      { FilterType.Type, null }
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
        int marginTop = 140;
        _dataSources.Add(side, new());

        foreach (var filter in filters)
        {
          var partyFilter = new PartyFilterControllerVM(side,
            OnFilterChange, filter.Value, side == Side.Right ? 595 : 0,side == Side.Left ? 595 : 0, marginTop, filter.Key);
                    
          _dataSources[side].Add(filter.Key, partyFilter);
          marginTop -= 40;
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
        { FilterType.Type, null }
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
        }
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
        { FilterType.Tier, new() }
      };
        
      filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_types}All Types"), FilterType.Type,"all"));
      filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_tiers}All Tiers"), FilterType.Tier,"all"));

      for (int i = 1; i <= 24; i++)
      {
        filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(GameTexts.FindText("str_inventory_type_" + i),
          FilterType.Type, ((ItemObject.ItemTypeEnum)i).ToString()));
      }

      for (int i = 0; i <= 5; i++)
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

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
  }
}
