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
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.GUI
{
  public class BOPartyVM : PartyVM
  {
    private static Dictionary<Side, Dictionary<FilterType, PartyFilterControllerVM>> _dataSources;
      
    private static Dictionary<FilterType, object> _filterMethods = new()
    {
      { FilterType.Tier, null },
      { FilterType.Class, null },
      { FilterType.Culture, null },
    };
  
    private static readonly MethodInfo _refreshTopInformation = AccessTools.Method(typeof(PartyVM), "RefreshTopInformation");
    private static readonly MethodInfo _refreshPartyInformation = AccessTools.Method(typeof(PartyVM), "RefreshPartyInformation");
    
    public static BOPartyVM? Instance { get; private set; }
    public BOPartyVM(PartyScreenLogic partyScreenLogic) : base(partyScreenLogic)
    {
      SetHotKeys();
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
        { FilterType.Class, null },
        { FilterType.Culture, null },
      };
      _dataSources = null;
    }

    private void OnFilterChange(Side partyRosterSide, (FilterType filterType, object selected) selected)
    {
        MBBindingList<PartyCharacterVM> partyTroops =
            partyRosterSide == Side.Left ? OtherPartyTroops : MainPartyTroops;

        _filterMethods[selected.filterType] = selected.selected;

        if (selected.selected is string text && text == "all")
            _filterMethods[selected.filterType] = null;
        int index = partyTroops.Count - 1;
        for (int i = partyTroops.Count - 1; i >= 0; i--)
        {
            Dictionary<FilterType, bool> results = new();
            foreach (var keyValuePair in _filterMethods.Where(x=>x.Value != null))
            {
                switch (keyValuePair.Key)
                {
                    case FilterType.Culture:
                        string cultureId = (string)keyValuePair.Value;
                        results.Add(keyValuePair.Key, false);
                        if (partyTroops[index].Character.Culture.StringId == cultureId)
                            results[keyValuePair.Key] = true;
                        break;

                    case FilterType.Class:
                        FormationClass formationClass = (FormationClass)keyValuePair.Value;
                        results.Add(keyValuePair.Key, false);
                        if (partyTroops[index].Character.GetFormationClass() == formationClass)
                            results[keyValuePair.Key] = true;
                        break;

                    case FilterType.Tier:
                        int tier = (int)keyValuePair.Value;
                        results.Add(keyValuePair.Key, false);
                        if (partyTroops[index].Character.Tier == tier)
                            results[keyValuePair.Key] = true;
                        break;
                }
            }

            if (results.Values.All(x=>x))
            {
                partyTroops.Insert(0, partyTroops[index]);
                partyTroops.RemoveAt(index + 1);
                index++;
            }

            index--;
        }

        _refreshTopInformation.Invoke(this, new object[] { });
        _refreshPartyInformation.Invoke(this, new object[] { });
    }
    
    private static Dictionary<FilterType, List<TroopFilterSelectorItemVM>> InitializeFilters()
    {
      int biggestTier = CharacterObject.All.OrderByDescending(x => x.Tier).FirstOrDefault().Tier;
      Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = new()
      {
        { FilterType.Tier, new() },
        { FilterType.Class, new() },
        { FilterType.Culture, new() },
      };
            
      filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_cultures}All Cultures"), FilterType.Culture,"all"));
      filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_classes}All Classes"), FilterType.Class,"all"));
      filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("{=all_tiers}All Tiers"), FilterType.Tier,"all"));
        
      FormationClass[] formationClasses = 
      {
        FormationClass.Infantry,
        FormationClass.Ranged,
        FormationClass.Cavalry,
        FormationClass.HorseArcher
      };
        
      filters[FilterType.Culture].AddRange(MBObjectManager.Instance.GetObjectTypeList<CultureObject>()
        .Select(x => new TroopFilterSelectorItemVM(x.Name, FilterType.Culture, x.StringId)).ToList());

      foreach (var formationClass in formationClasses)
        filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(GameTexts.FindAllTextVariations("str_troop_group_name").ElementAt((int)formationClass), FilterType.Class, formationClass));

      for (int i = 1; i <= biggestTier; i++)
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

      return filters;
    }

    private void SetHotKeys()
    {
      SetGetKeyTextFromKeyIDFunc(Game.Current.GameTextManager.GetHotKeyGameTextFromKeyID);
      SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
      SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
      SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
      SetTakeAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("TakeAllTroops"));
      SetDismissAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("GiveAllTroops"));
      SetTakeAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("TakeAllPrisoners"));
      SetDismissAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("GiveAllPrisoners"));
      SetOpenUpgradePanelInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("OpenUpgradePopup"));
      SetOpenRecruitPanelInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("OpenRecruitPopup"));
      UpgradePopUp.SetPrimaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemPrimaryAction"));
      UpgradePopUp.SetSecondaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemSecondaryAction"));
      RecruitPopUp.SetPrimaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemPrimaryAction"));
      RecruitPopUp.SetSecondaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemSecondaryAction"));
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
    public PartyFilterControllerVM FilterClassLeft
    {
      get => _dataSources[Side.Left][FilterType.Class];
      set
      {
        if (value == _dataSources[Side.Left][FilterType.Class])
          return;
        _dataSources[Side.Left][FilterType.Class] = value;
        OnPropertyChangedWithValue(value);
      }
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
    
    [DataSourceProperty]
    public PartyFilterControllerVM FilterClassRight
    {
      get => _dataSources[Side.Right][FilterType.Class];
      set
      {
        if (value == _dataSources[Side.Right][FilterType.Class])
          return;
        _dataSources[Side.Right][FilterType.Class] = value;
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
  }
}
