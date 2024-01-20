using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BasicOverhaul.Models;

internal class BOBattleRewardModel : DefaultBattleRewardModel
{
    private float GlobalChance => BasicOverhaulCampaignConfig.Instance?.GlobalLootChance ?? 0;
    public override float DestroyHideoutBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DestroyHideoutBannerLootChance;

    public override float CaptureSettlementBannerLootChance => GlobalChance > 0 ? GlobalChance : base.CaptureSettlementBannerLootChance;

    public override float DefeatRegularHeroBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatRegularHeroBannerLootChance;

    public override float DefeatClanLeaderBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatClanLeaderBannerLootChance;

    public override float DefeatKingdomRulerBannerLootChance => GlobalChance > 0 ? GlobalChance : base.DefeatKingdomRulerBannerLootChance;
    public override ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateRenownGain(party, renownValueOfBattle, contributionShare);
        if (BasicOverhaulCampaignConfig.Instance?.BattleRenownGainMultiplier > 0)
            baseNumber.AddFactor(BasicOverhaulCampaignConfig.Instance.BattleRenownGainMultiplier);
        return baseNumber;
    }
    public override ExplainedNumber CalculateInfluenceGain(PartyBase party, float influenceValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateInfluenceGain(party, influenceValueOfBattle, contributionShare);
        if (BasicOverhaulCampaignConfig.Instance?.BattleInfluenceGainMultiplier > 0)
            baseNumber.AddFactor(BasicOverhaulCampaignConfig.Instance.BattleInfluenceGainMultiplier);
        return baseNumber;
    }
    public override ExplainedNumber CalculateMoraleGainVictory(PartyBase party, float renownValueOfBattle,
        float contributionShare)
    {
        ExplainedNumber baseNumber = base.CalculateMoraleGainVictory(party, renownValueOfBattle, contributionShare);
        if(BasicOverhaulCampaignConfig.Instance?.BattleMoraleGainMultiplier > 0)
            baseNumber.AddFactor((float)(BasicOverhaulCampaignConfig.Instance.BattleMoraleGainMultiplier));
        return baseNumber;
    }
}