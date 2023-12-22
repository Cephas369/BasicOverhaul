using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BasicOverhaul.Models;

internal class BOBattleRewardModel : DefaultBattleRewardModel
{
    private float GlobalChance => BasicOverhaulConfig.Instance.GlobalLootChance;
    public override float DestroyHideoutBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DestroyHideoutBannerLootChance;

    public override float CaptureSettlementBannerLootChance => GlobalChance > 0 ? GlobalChance : base.CaptureSettlementBannerLootChance;

    public override float DefeatRegularHeroBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatRegularHeroBannerLootChance;

    public override float DefeatClanLeaderBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatClanLeaderBannerLootChance;

    public override float DefeatKingdomRulerBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatKingdomRulerBannerLootChance;
    public override ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateRenownGain(party, renownValueOfBattle, contributionShare);
        if (BasicOverhaulConfig.Instance?.BattleRenownGainMultiplier > 0)
            baseNumber.AddFactor(BasicOverhaulConfig.Instance.BattleRenownGainMultiplier);
        return baseNumber;
    }
    public override ExplainedNumber CalculateInfluenceGain(PartyBase party, float influenceValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateInfluenceGain(party, influenceValueOfBattle, contributionShare);
        if (BasicOverhaulConfig.Instance?.BattleInfluenceGainMultiplier > 0)
            baseNumber.AddFactor(BasicOverhaulConfig.Instance.BattleInfluenceGainMultiplier);
        return baseNumber;
    }
    public override ExplainedNumber CalculateMoraleGainVictory(PartyBase party, float renownValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateMoraleGainVictory(party, renownValueOfBattle, contributionShare);
        if(BasicOverhaulConfig.Instance?.BattleMoraleGainMultiplier > 0)
            baseNumber.AddFactor((float)(BasicOverhaulConfig.Instance.BattleMoraleGainMultiplier));
        return baseNumber;
    }
}