using HarmonyLib;
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