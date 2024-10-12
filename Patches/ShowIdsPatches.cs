using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(ItemObject), "Name", MethodType.Getter)]
public static class ShowItemId
{
    public static void Postfix(ref TextObject __result, ItemObject __instance)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableSeeIds == true)
        {
            __result = new TextObject(__result.ToString().Replace("=", string.Empty) + $"({__instance.StringId})");
        }
    }
}

[HarmonyPatch(typeof(BasicCharacterObject), "Name", MethodType.Getter)]
public static class ShowCharacterId
{
    public static void Postfix(ref TextObject __result, BasicCharacterObject __instance)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableSeeIds == true && !__instance.IsHero)
        {
            __result.Value += $" ({__instance.StringId})";
        }
    }
}

[HarmonyPatch(typeof(CharacterObject), "Name", MethodType.Getter)]
public static class ShowHeroId
{
    public static void Postfix(ref TextObject __result, CharacterObject __instance)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableSeeIds == true && __instance.IsHero)
        {
            __result.Value += $" ({__instance.StringId})";
        }
    }
}

[HarmonyPatch(typeof(Settlement), "Name", MethodType.Getter)]
public static class ShowSettlementId
{
    public static void Postfix(ref TextObject __result, Settlement __instance)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableSeeIds == true)
        {
            __result.Value += $" ({__instance.StringId})";
        }
    }
}