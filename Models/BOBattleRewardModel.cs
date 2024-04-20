using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace BasicOverhaul.Models;

internal class BOBattleRewardModel : DefaultBattleRewardModel
{
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