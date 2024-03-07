using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using BasicOverhaul.GUI;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
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
public static class LoadMoviePatch
{
    private static SPInventoryVM currentInventoryDataSource;
    public static void Prefix(ref string movieName, ref ViewModel dataSource)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true)
        {
            if (movieName == "PartyScreen" && dataSource is PartyVM partyVm)
            {
                partyVm = new BOPartyVM(partyVm.PartyScreenLogic);
                dataSource = partyVm;
            }

            if (movieName == "Inventory" && dataSource is SPInventoryVM spInventoryVm)
            {
                InventoryLogic inventoryLogic =
                    (InventoryLogic)AccessTools.Field(typeof(SPInventoryVM), "_inventoryLogic").GetValue(spInventoryVm);
                Func<WeaponComponentData, ItemObject.ItemUsageSetFlags> _getItemUsageSetFlags = 
                    (Func<WeaponComponentData, ItemObject.ItemUsageSetFlags>)AccessTools.Field(typeof(SPInventoryVM), "_getItemUsageSetFlags").GetValue(spInventoryVm);
                string _fiveStackShortcutkeyText =
                    (string)AccessTools.Field(typeof(SPInventoryVM), "_fiveStackShortcutkeyText").GetValue(spInventoryVm);
                string _entireStackShortcutkeyText =
                    (string)AccessTools.Field(typeof(SPInventoryVM), "_entireStackShortcutkeyText").GetValue(spInventoryVm);
                
                spInventoryVm = new BOInventoryVM(inventoryLogic, !spInventoryVm.IsInWarSet, _getItemUsageSetFlags, _fiveStackShortcutkeyText, _entireStackShortcutkeyText);
                dataSource = spInventoryVm;
                currentInventoryDataSource = dataSource as SPInventoryVM;
            }
        }
    }
    [HarmonyPatch(typeof(GauntletInventoryScreen), "OnInitialize")]
    public static class GauntletInventoryScreenPatch
    {
        public static void Postfix(ref SPInventoryVM ____dataSource)
        {
            ____dataSource = currentInventoryDataSource;
        }
    }
}




[HarmonyPatch(typeof(ScreenBase), "AddLayer")]
public static class FixStackModifiersPatch
{
    private static FieldInfo dataSource = AccessTools.Field(typeof(GauntletPartyScreen), "_dataSource");
    public static void Prefix(ScreenLayer layer, ScreenBase __instance)
    {
        if (__instance is GauntletPartyScreen gauntletPartyScreen && BOPartyVM.Instance != null)
        {
            dataSource.SetValue(__instance, BOPartyVM.Instance);
        }
    }
}
public static class XmlGUILoadPatch
{
    public static void Postfix(XmlDocument doc, XmlReader reader, bool preserveWhitespace)
    {
        if (reader?.BaseURI?.Contains("Party") == true)
        {
            XmlNode? node = doc.SelectSingleNode("Prefab")?.SelectSingleNode("Window")?.SelectSingleNode("PartyScreenWidget")?
                .SelectSingleNode("Children");

            if (node != null)
            {
                string[] partyVMNodes = new[]
                {
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterCultureLeft}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterClassLeft}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTierLeft}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterCultureRight}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterClassRight}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTierRight}\"/>"
                };
                
                foreach (var element in partyVMNodes)
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(element);
                    node.AppendChild(doc.ImportNode(document.DocumentElement, true));
                }
            }
        }
        
        if (reader?.BaseURI?.Contains("Inventory") == true)
        {
            XmlNode? node = doc.SelectSingleNode("Prefab")?.SelectSingleNode("Window")?.SelectSingleNode("InventoryScreenWidget")?
                .SelectSingleNode("Children");

            if (node != null)
            {
                string[] inventoryVMNodes = new[]
                {
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTierLeft}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTypeLeft}\" />",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTierRight}\"/>",
                    "<PartyFilterController HorizontalAlignment=\"Center\" DataSource=\"{FilterTypeRight}\" />",
                };
                
                foreach (var element in inventoryVMNodes)
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(element);
                    node.AppendChild(doc.ImportNode(document.DocumentElement, true));
                }
            }
        }
    }
}