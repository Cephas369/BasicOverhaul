using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BasicOverhaul.GUI;
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


[HarmonyPatch(typeof(GauntletLayer), "LoadMovie")]
public static class LoadMoviePatch
{
    public static void Prefix(ref string movieName, ref ViewModel dataSource)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true && movieName == "PartyScreen" && dataSource is PartyVM partyVm)
        {
            movieName = "BOPartyScreen";
            partyVm = new BOPartyVM(partyVm.PartyScreenLogic);
            dataSource = partyVm;
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