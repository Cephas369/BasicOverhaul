using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SettlementGovernorSelectionVM))]
public static class AvailableGovernorsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(Settlement), typeof(Action<Hero>))]
    public static void ConstructorPostfix(SettlementGovernorSelectionVM __instance, Settlement settlement,
        Action<Hero> onDone)
    {
        if (BasicOverhaulConfig.Instance?.EnableGovernorNotables == true && settlement != null)
        {
            foreach (var notable in settlement.Notables)
            {
                if (!notable.IsDisabled && !notable.IsDead)
                {
                    __instance.AvailableGovernors.Add(new SettlementGovernorSelectionItemVM(notable,
                        delegate (SettlementGovernorSelectionItemVM x) { onDone.Invoke(x.Governor); }));
                }
            }
        }
    }
}