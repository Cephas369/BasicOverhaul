using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(MissionBoundaryCrossingHandler), "TickForMainAgent")]
public static class AddMissionBoundaries
{
    public static bool Prefix()
    {
        if (BasicOverhaulGlobalConfig.Instance?.DisableMissionBoundaries == true)
            return false;
        return true;
    }
}


[HarmonyPatch(typeof(ItemRoster), "AddToCounts", new Type[] { typeof(EquipmentElement), typeof(int) })]
public static class AddToCounts
{
    public static void Prefix(EquipmentElement rosterElement, int number)
    {
        if (rosterElement.Item.Type == ItemObject.ItemTypeEnum.BodyArmor && number > 0)
        {
            int i;
            i = 0;
        }
    }
}