using System;
using System.Collections.Generic;
using System.IO;
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
    Tier,
    Type
}

public enum Side
{
    Left,
    Right
}

public class PartyGUIPatch
{
    private static GauntletLayer _gauntletLayer;
    private static Dictionary<Side, List<(FilterType filterType, PartyFilterControllerVM partyFilterControllers)>> dataSources;
    private static Dictionary<FilterType, object> filterMethods = new()
    {
        { FilterType.Tier, null },
        { FilterType.Class, null },
        { FilterType.Culture, null },
    };
    private static PartyVM _partyVm;

    private static MethodInfo RefreshTopInformation = AccessTools.Method(typeof(PartyVM), "RefreshTopInformation");
    private static MethodInfo RefreshPartyInformation = AccessTools.Method(typeof(PartyVM), "RefreshPartyInformation");
    
    public static void Postfix(ScreenLayer layer, ScreenBase __instance)
    {
        if (__instance is GauntletPartyScreen gauntletPartyScreen && layer is GauntletLayer gauntletLayer)
        {
            _partyVm = (PartyVM)AccessTools.Field(typeof(GauntletPartyScreen), "_dataSource").GetValue(gauntletPartyScreen);
            
            Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = InitializeFilters();
            dataSources = new();
            
            for (int i = 0; i < 2; i++)
            {
                Side side = i == 0 ? Side.Left : Side.Right;
                int marginTop = 400;
                dataSources.Add(side, new());

                foreach (var filter in filters)
                {
                    var partyFilter = new PartyFilterControllerVM(side,
                        OnFilterChange, filter.Value, side == Side.Left ? 675 : 1128, marginTop, filter.Key);
                    
                    dataSources[side].Add((filter.Key, partyFilter));
                    
                    gauntletLayer.LoadMovie("PartyFilterController", partyFilter);

                    marginTop -= 36;
                }
            }
        }
    }

    private static void OnFilterChange(Side partyRosterSide, (FilterType filterType, object selected) selected)
    {
        MBBindingList<PartyCharacterVM> partyTroops =
            partyRosterSide == Side.Left ? _partyVm.OtherPartyTroops : _partyVm.MainPartyTroops;

        filterMethods[selected.filterType] = selected.selected;

        if (selected.selected is string text && text == "all")
            filterMethods[selected.filterType] = null;
        int index = partyTroops.Count - 1;
        for (int i = partyTroops.Count - 1; i >= 0; i--)
        {
            Dictionary<FilterType, bool> results = new();
            foreach (var keyValuePair in filterMethods.Where(x=>x.Value != null))
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
                        FormationClass formationClass =
                            (FormationClass)Enum.Parse(typeof(FormationClass), (string)keyValuePair.Value);
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

        RefreshTopInformation.Invoke(_partyVm, new object[] { });
        RefreshPartyInformation.Invoke(_partyVm, new object[] { });
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
            
        filters[FilterType.Culture].Add(new TroopFilterSelectorItemVM(new TextObject("All Cultures"), FilterType.Culture,"all"));
        filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(new TextObject("All Classes"), FilterType.Class,"all"));
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("All Tiers"), FilterType.Tier,"all"));
        
        string[] formationClasses = 
        {
            nameof(FormationClass.Infantry),
            nameof(FormationClass.Ranged),
            nameof(FormationClass.Cavalry),
            nameof(FormationClass.HorseArcher),
        };
        
        filters[FilterType.Culture].AddRange(MBObjectManager.Instance.GetObjectTypeList<CultureObject>()
            .Select(x => new TroopFilterSelectorItemVM(x.Name, FilterType.Culture, x.StringId)).ToList());
        
        foreach (var formationClass in formationClasses)
            filters[FilterType.Class].Add(new TroopFilterSelectorItemVM(new TextObject(InsertWhitespace(formationClass)), FilterType.Class, formationClass));

        for (int i = 1; i <= biggestTier; i++)
            filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

        return filters;
    }
    
    public static void OnPartyVMFinalize()
    {
        filterMethods = new()
        {
            { FilterType.Tier, null },
            { FilterType.Class, null },
            { FilterType.Culture, null },
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