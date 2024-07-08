using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(KillCharacterAction), "ApplyInternal")]
public static class KillCharacterActionPatch
{
    public static bool ApplyInternal = false;
    public static void Prefix(
        Hero victim,
        Hero killer,
        KillCharacterAction.KillCharacterActionDetail actionDetail,
        bool showNotification,
        bool isForced = false)
    {
        ApplyInternal = true;
    }
    public static void Postfix(
        Hero victim,
        Hero killer,
        KillCharacterAction.KillCharacterActionDetail actionDetail,
        bool showNotification,
        bool isForced = false)
    {
        ApplyInternal = false;
    }
}

[HarmonyPatch(typeof(ChangeRelationAction), "ApplyPlayerRelation")]
public static class ExecutionPatch
{
    public static bool Prefix(
        Hero gainedRelationWith,
        int relation,
        bool affectRelatives = true,
        bool showQuickNotification = true)
    {
        if (BasicOverhaulGlobalConfig.Instance?.DisableExecutionRelationPenalty == true && KillCharacterActionPatch.ApplyInternal)
        {
            return false;
        }

        return true;
    }
}