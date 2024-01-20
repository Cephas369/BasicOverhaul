using System;
using System.Diagnostics;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(ItemRoster), "AddToCounts", new Type[] {typeof(ItemObject), typeof(int)})]
public static class GetCheatItemCountPatch
{
    public static void Prefix(ItemObject item, ref int number)
    {
        string callerClass = new StackFrame(2).GetMethod().Name;
        if (callerClass == "OpenInventoryPresentation" && BasicOverhaulGlobalConfig.Instance.CheatItemCount > 0 && Game.Current?.CheatMode == true)
        {
            number = BasicOverhaulGlobalConfig.Instance.CheatItemCount;
        }
    }
}