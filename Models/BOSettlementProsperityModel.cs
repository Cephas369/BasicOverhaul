using BasicOverhaul.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models;

internal class BOSettlementProsperityModel : DefaultSettlementProsperityModel
{
    
    public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
    {
        ExplainedNumber baseNumber = base.CalculateProsperityChange(fortification, includeDescriptions);

        if (BasicOverhaulGlobalConfig.Instance?.EnableSlaveSystem == true)
        {
            Settlement settlement = fortification.Settlement;
        
            if (settlement == null) return baseNumber;
            string settlementId = settlement.StringId;
            if (SlaveBehavior.Instance.SlaveData.ContainsKey(settlementId) && SlaveBehavior.Instance.SlaveData[settlementId].DestinationType == DestinationTypes.TownProsperity)
            {
                baseNumber.Add(SlaveBehavior.Instance.SlaveData[settlementId].SlaveAmount / 2f,
                    new TextObject("{=bo_slavery}Slavery"));
            }
        }
        
        return baseNumber;
    }
}