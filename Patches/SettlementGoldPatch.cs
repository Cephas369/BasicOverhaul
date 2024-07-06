using HarmonyLib;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SettlementComponent), "ChangeGold")]
public static class SettlementGoldPatch
{
    public static void Prefix(ref int changeAmount, SettlementComponent __instance)
    {
        if (changeAmount > 0 && BasicOverhaulCampaignConfig.Instance?.TownsGoldMultiplier > 0 && __instance.IsTown)
            changeAmount *= BasicOverhaulCampaignConfig.Instance.TownsGoldMultiplier;
    }
}