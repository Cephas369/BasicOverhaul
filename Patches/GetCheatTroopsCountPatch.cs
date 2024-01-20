using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(TroopRoster), "AddToCounts")]
public static class GetCheatTroopsCountPatch
{
    private static FieldInfo _countToAddForEachTroopCheatMode =
        AccessTools.Field(typeof(PartyScreenManager), "_countToAddForEachTroopCheatMode");
    
    private static void Prefix(CharacterObject character, ref int count, bool insertAtFront = false, int woundedCount = 0, int xpChange = 0, bool removeDepleted = true, int index = -1)
    {
        string callerClass = new StackFrame(2).GetMethod().Name;
        if (callerClass == "GetRosterWithAllGameTroops" && BasicOverhaulGlobalConfig.Instance.CheatTroopCount > 0 && Game.Current?.CheatMode == true)
        {
            count = BasicOverhaulGlobalConfig.Instance.CheatTroopCount;
        }
    }
}