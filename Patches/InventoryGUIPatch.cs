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
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
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

[HarmonyPatch(typeof(GauntletLayer), "LoadMovie")]
public static class InventoryPatch
{
    private static Dictionary<Side, List<(FilterType filterType, PartyFilterControllerVM partyFilterControllers)>> dataSources;
    private static Dictionary<FilterType, Delegate> filterMethods = new()
    {
        { FilterType.Tier, null },
        { FilterType.Type, null }
    };
    private static SPInventoryVM _inventoryVm;
    private static MethodInfo RefreshValues = AccessTools.Method(typeof(SPInventoryVM), "RefreshValues");

    public static void Postfix(string movieName, ViewModel dataSource, GauntletLayer __instance)
    {
        if (BasicOverhaulConfig.Instance?.EnableInventoryScreenFilters == true && movieName == "Inventory" && ScreenManager.TopScreen is GauntletInventoryScreen gauntletInventoryScreen)
        {
            ClearValues();
            _inventoryVm = (SPInventoryVM)AccessTools.Field(typeof(GauntletInventoryScreen), "_dataSource").GetValue(gauntletInventoryScreen);

            Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = InitializeFilters();
                
            dataSources = new();
            
            for (int i = 0; i < 2; i++)
            {
                Side side = i == 0 ? Side.Left : Side.Right;
                int lastMargin = 140;

                dataSources.Add(side, new());

                foreach (var filter in filters)
                {
                    var partyFilter = new PartyFilterControllerVM(side, OnFilterChange, filter.Value, side == Side.Left ? 600 : 1200, lastMargin,
                        filter.Key);

                    dataSources[side].Add((filter.Key, partyFilter));

                    __instance.LoadMovie("PartyFilterController", partyFilter);

                    lastMargin -= 40;
                }
            }
            ScreenManager.TrySetFocus(__instance);
        }
    }
    private static MBBindingList<SPItemVM> GetItemListBySide(Side side) => side == Side.Left ? _inventoryVm.LeftItemListVM : _inventoryVm.RightItemListVM;
    private static Dictionary<FilterType, List<TroopFilterSelectorItemVM>> InitializeFilters()
    {
        Dictionary<FilterType, List<TroopFilterSelectorItemVM>> filters = new()
        {
            { FilterType.Type, new() },
            { FilterType.Tier, new() }
        };
        
        filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(new TextObject("All Types"), FilterType.Type,"all"));
        filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject("All Tiers"), FilterType.Tier,"all"));

        List<string> itemTypes = Enum.GetNames(typeof(ItemObject.ItemTypeEnum)).ToList();
        itemTypes.RemoveAll(x=>x=="Invalid");
        foreach (var itemType in itemTypes)
            filters[FilterType.Type].Add(new TroopFilterSelectorItemVM(new TextObject(Regex.Replace(itemType, @"(?<=[a-z])(?=[A-Z])", " ")),
                FilterType.Type, itemType));

        for (int i = 0; i <= 5; i++)
            filters[FilterType.Tier].Add(new TroopFilterSelectorItemVM(new TextObject(i), FilterType.Tier, i));

        return filters;
    }
    private static void Reset(Side side)
    {
        MBBindingList<SPItemVM> items = GetItemListBySide(side);
        foreach (var itemVm in items)
            itemVm.IsFiltered = false;
    }
    private static void OnFilterChange(Side side, (FilterType filterType, object selected) selected)
    {
        Reset(side);
        if (selected.selected is string text && text == "all")
        {
            filterMethods[selected.filterType] = null;
        }
        else
        {
            MBBindingList<SPItemVM> items = GetItemListBySide(side);
            switch (selected.filterType)
            {
                case FilterType.Type:
                    ItemObject.ItemTypeEnum itemType = (ItemObject.ItemTypeEnum)Enum.Parse(typeof(ItemObject.ItemTypeEnum), (string)selected.selected);
                    filterMethods[selected.filterType] = () =>
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
                    filterMethods[selected.filterType] = () =>
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

        foreach (var method in filterMethods)
            if(method.Value != null)
                method.Value.DynamicInvoke();

        RefreshValues.Invoke(_inventoryVm, new object[] { });
    }
    
    public static void ClearValues()
    {
        filterMethods = new()
        {
            { FilterType.Tier, null },
            { FilterType.Type, null }
        };
        dataSources = null;
        _inventoryVm = null;
    }
}