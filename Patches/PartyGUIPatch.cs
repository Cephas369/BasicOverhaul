using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace BasicOverhaul.Patches;

public enum FilterType
{
    Culture,
    Class,
    Tier
}

public class PartyGUIPatch
{
    private static GauntletLayer _gauntletLayer;
    private static Dictionary<FilterType, PartyFilterControllerVM> dataSources;
    private static Dictionary<FilterType, Delegate> filterMethods = new()
    {
        { FilterType.Culture, null },
        { FilterType.Class, null },
        { FilterType.Tier, null }
    };
    private static PartyVM _partyVm;
    private static MBReadOnlyList<PartyCharacterVM> original = new();
    
    private static MethodInfo RefreshTopInformation = AccessTools.Method(typeof(PartyVM), "RefreshTopInformation");
    private static MethodInfo RefreshPartyInformation = AccessTools.Method(typeof(PartyVM), "RefreshPartyInformation");
    
    public static void Postfix(ScreenLayer layer, ScreenBase __instance)
    {
        if (__instance is GauntletPartyScreen gauntletPartyScreen && layer is GauntletLayer gauntletLayer)
        {
            _partyVm = (PartyVM)AccessTools.Field(typeof(GauntletPartyScreen), "_dataSource").GetValue(gauntletPartyScreen);
            
            foreach (var character in _partyVm.OtherPartyTroops)
                original.Add(character);

            Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = InitializeFilters();
                
            dataSources = new();
            int lastMargin = 140;
            foreach (var filter in filters)
            {
                dataSources.Add(filter.Key, new PartyFilterControllerVM(PartyScreenLogic.PartyRosterSide.Left, 
                    OnFilterChange, filter.Value, lastMargin, filter.Key));

                gauntletLayer.LoadMovie("PartyFilterController", dataSources[filter.Key]);

                lastMargin += 160;
            }
        }
    }

    private static void OnFilterChange(PartyScreenLogic.PartyRosterSide partyRosterSide, (FilterType filterType, object selected) selected)
    {
        Reset();
        if (selected.selected is string text && text == "all")
        {
            filterMethods[selected.filterType] = null;
        }
        else
            switch (selected.filterType)
            {
                case FilterType.Culture:
                    string cultureId = (string)selected.selected;
                    filterMethods[selected.filterType] = () =>
                    {
                        for (int i = _partyVm.OtherPartyTroops.Count - 1; i >= 0; i--)
                        {
                            if (_partyVm.OtherPartyTroops[i].Character.Culture.StringId != cultureId)
                                _partyVm.OtherPartyTroops.Remove(_partyVm.OtherPartyTroops[i]);
                        }
                    };
                    break;
                
                case FilterType.Class:
                    FormationClass formationClass = (FormationClass)Enum.Parse(typeof(FormationClass), (string)selected.selected);
                    filterMethods[selected.filterType] = () =>
                    {
                        for (int i = _partyVm.OtherPartyTroops.Count - 1; i >= 0; i--)
                        {
                            if (_partyVm.OtherPartyTroops[i].Character.GetFormationClass() != formationClass)
                                _partyVm.OtherPartyTroops.Remove(_partyVm.OtherPartyTroops[i]);
                        }
                    };
                    break;
                
                case FilterType.Tier:
                    int tier = (int)selected.selected;
                    filterMethods[selected.filterType] = () =>
                    {
                        for (int i = _partyVm.OtherPartyTroops.Count - 1; i >= 0; i--)
                        {
                            if (_partyVm.OtherPartyTroops[i].Character.Tier != tier)
                                _partyVm.OtherPartyTroops.Remove(_partyVm.OtherPartyTroops[i]);
                        }
                    };
                    break;
            }

        foreach (var method in filterMethods)
            if(method.Value != null)
                method.Value.DynamicInvoke();
        
        RefreshTopInformation.Invoke(_partyVm, new object[] { });
        RefreshPartyInformation.Invoke(_partyVm, new object[] { });
    }
    
    private static void Reset()
    {
        _partyVm.OtherPartyTroops.Clear();
        foreach (var character in original)
            _partyVm.OtherPartyTroops.Add(character);
    }

    private static Dictionary<FilterType, List<TroopFilterSelectorItemVM>> InitializeFilters()
    {
        int biggestTier = CharacterObject.All.OrderByDescending(x => x.Tier).FirstOrDefault().Tier;
        Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = new()
        {
            { FilterType.Culture, new() },
            { FilterType.Class, new() },
            { FilterType.Tier, new() }
        };

        string[] formationClasses = 
        {
            nameof(FormationClass.Infantry),
            nameof(FormationClass.Ranged),
            nameof(FormationClass.Cavalry),
            nameof(FormationClass.HorseArcher),
        };
        
        filters[FilterType.Culture] = MBObjectManager.Instance.GetObjectTypeList<CultureObject>()
            .Select(x => new TroopFilterSelectorItemVM(x.Name, FilterType.Culture, x.StringId)).ToList();
        
        foreach (var formationClass in formationClasses)
            filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(new TextObject(InsertWhitespace(formationClass)), FilterType.Class, formationClass));

        for (int i = 1; i <= biggestTier; i++)
            filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

        filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(new TextObject("All Cultures"), FilterType.Culture,"all"));
        filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(new TextObject("All Classes"), FilterType.Class,"all"));
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("All Tiers"), FilterType.Tier,"all"));
        
        return filters;
    }
    
    public static void OnPartyVMFinalize()
    {
        original = new();
        filterMethods = new()
        {
            { FilterType.Culture, null },
            { FilterType.Class, null },
            { FilterType.Tier, null }
        };
        dataSources = null;
        _gauntletLayer = null;
        _partyVm = null;
    }
    
    static string InsertWhitespace(string input)
    {
        string pattern = @"(?<=[a-z])(?=[A-Z])";
        string replacement = " ";
        string result = Regex.Replace(input, pattern, replacement);

        return result;
    }
}