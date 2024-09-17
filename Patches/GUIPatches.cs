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
        if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true && movieName == "PartyScreen" && dataSource is PartyVM partyVm)
        {
            partyVm = new BOPartyVM(partyVm.PartyScreenLogic);
            dataSource = partyVm;
        }

        if (BasicOverhaulGlobalConfig.Instance?.EnableInventoryScreenFilters == true && movieName == "Inventory" && dataSource is SPInventoryVM spInventoryVm)
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
    [HarmonyPatch(typeof(GauntletInventoryScreen), "OnInitialize")]
    public static class GauntletInventoryScreenPatch
    {
        public static void Postfix(ref SPInventoryVM ____dataSource)
        {
            if(BasicOverhaulGlobalConfig.Instance?.EnableInventoryScreenFilters == true)
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
        if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true && __instance is GauntletPartyScreen gauntletPartyScreen && BOPartyVM.Instance != null)
        {
            dataSource.SetValue(__instance, BOPartyVM.Instance);
        }
    }
}
public static class XmlGUILoadPatch
{
    public static void Postfix(XmlDocument doc, XmlReader reader, bool preserveWhitespace)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true && reader?.BaseURI?.Contains("Party") == true)
        {
            XmlNode? node = doc.SelectSingleNode("Prefab")?.SelectSingleNode("Window")?.SelectSingleNode("PartyScreenWidget")?
                .SelectSingleNode("Children");

            if (node != null)
            {
                XmlNode? beforeNode = node.SelectSingleNode("PartyTroopManagerPopUp");
                if (beforeNode != null)
                {
                    string[] partyVMNodes = 
                    {
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"300\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"20\" DataSource=\"{FilterCultureLeft}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"264\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"20\"  DataSource=\"{FilterClassLeft}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"228\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"20\"  DataSource=\"{FilterTierLeft}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"300\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-20\"  DataSource=\"{FilterCultureRight}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"264\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-20\"  DataSource=\"{FilterClassRight}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"228\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-20\"  DataSource=\"{FilterTierRight}\"/>"
                    };
                
                    foreach (var element in partyVMNodes)
                    {
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(element);
                        node.InsertBefore(doc.ImportNode(document.DocumentElement, true), beforeNode);
                    }
                }
            }
        }
        
        if (BasicOverhaulGlobalConfig.Instance?.EnableInventoryScreenFilters == true && reader?.BaseURI?.Contains("Inventory") == true)
        {
            XmlNode? node = doc.SelectSingleNode("Prefab")?.SelectSingleNode("Window")?.SelectSingleNode("InventoryScreenWidget")?
                .SelectSingleNode("Children");
            if (node != null)
            {
                XmlNode? beforeNode = node.SelectSingleNode("InventoryTooltip");
                if (beforeNode != null)
                {
                    string[] inventoryVMNodes = 
                    {
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"90\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"22\" Parameter.Height=\"25\" DataSource=\"{FilterTypeLeft}\" />",
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"120\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"22\" Parameter.Height=\"25\" DataSource=\"{FilterTierLeft}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" MarginTop=\"150\" MarginLeft=\"!SidePanel.Width\" PositionXOffset=\"22\" Parameter.Height=\"25\" DataSource=\"{FilterCultureLeft}\" />",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"90\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-22\" Parameter.Height=\"25\" DataSource=\"{FilterTypeRight}\" />",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"120\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-22\" Parameter.Height=\"25\" DataSource=\"{FilterTierRight}\"/>",
                        "<PartyFilterController HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginTop=\"150\" MarginRight=\"!SidePanel.Width\" PositionXOffset=\"-22\" Parameter.Height=\"25\" DataSource=\"{FilterCultureRight}\" />",
                        "<ButtonWidget Id=\"DoneButtonWidgetLeft\" Command.Click=\"ExecuteSearchActionLeft\" HorizontalAlignment=\"Left\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"120\" SuggestedHeight=\"35\" MarginLeft=\"585\" MarginTop=\"53\" Brush=\"Party.TalkSlot.Background\" GamepadNavigationIndex=\"0\"> <Children> <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"40\" MarginLeft=\"30\" MarginRight=\"50\" MaxWidth=\"380\" VerticalAlignment=\"Center\" Brush=\"Popup.Button.Text\" Text=\"@ButtonOkLabel\" /> </Children> </ButtonWidget>",
                        "<ButtonWidget Id=\"DoneButtonWidgetRight\" Command.Click=\"ExecuteSearchActionRight\" HorizontalAlignment=\"Right\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"120\" SuggestedHeight=\"35\" MarginRight=\"585\" MarginTop=\"53\" Brush=\"Party.TalkSlot.Background\" GamepadNavigationIndex=\"0\"> <Children> <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"40\" MarginLeft=\"30\" MarginRight=\"50\" MaxWidth=\"380\" VerticalAlignment=\"Center\" Brush=\"Popup.Button.Text\" Text=\"@ButtonOkLabel\" /> </Children> </ButtonWidget>"
                    };
                    foreach (var element in inventoryVMNodes)
                    {
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(element);
                    
                        node.InsertBefore(doc.ImportNode(document.DocumentElement, true), beforeNode);
                    }
                }
            }
        }
    }
}