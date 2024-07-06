using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SettlementComponent), "ChangeGold")]
public static class SettlementGoldPatch
{
    private static MethodInfo DoneLogic = AccessTools.Method(typeof(InventoryLogic), "DoneLogic");
    public static void Prefix(ref int changeAmount, SettlementComponent __instance)
    {
        if (changeAmount > 0 && BasicOverhaulCampaignConfig.Instance?.TownsGoldMultiplier > 0 && 
            __instance.IsTown && new StackFrame(3).GetMethod().Name != DoneLogic.Name)
            changeAmount *= BasicOverhaulCampaignConfig.Instance.TownsGoldMultiplier;
    }
}