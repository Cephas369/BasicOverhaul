using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace BasicOverhaul.Models;

[HarmonyPatch(typeof(ChangeClanInfluenceAction), "ApplyInternal")]
public static class ChangeClanInfluenceActionPatch
{
    public static void Prefix(Clan clan, ref float amount)
    {
        if (BasicOverhaulCampaignConfig.Instance?.InfluenceGainMultiplier > 0 && amount > 0)
            amount *= BasicOverhaulCampaignConfig.Instance.InfluenceGainMultiplier;
    }
}

[HarmonyPatch(typeof(GainRenownAction), "ApplyInternal")]
public static class GainRenownActionPatch
{
    public static void Prefix(Hero hero, ref float gainedRenown, bool doNotNotify)
    {
        if (BasicOverhaulCampaignConfig.Instance?.RenownGainMultiplier > 0 && gainedRenown > 0)
            gainedRenown *= BasicOverhaulCampaignConfig.Instance.RenownGainMultiplier;
    }
}
internal class BOBattleRewardModel : DefaultBattleRewardModel
{
    public override ExplainedNumber CalculateMoraleGainVictory(PartyBase party, float renownValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateMoraleGainVictory(party, renownValueOfBattle, contributionShare);
        if(BasicOverhaulCampaignConfig.Instance?.BattleMoraleGainMultiplier > 0)
            baseNumber.AddFactor((float)(BasicOverhaulCampaignConfig.Instance.BattleMoraleGainMultiplier));
        return baseNumber;
    }
}