using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Models;

public class BOSettlementEconomyModel : DefaultSettlementEconomyModel
{
    public override int GetTownGoldChange(Town town)
    {
        int baseValue = base.GetTownGoldChange(town);
        if (baseValue > 0 && BasicOverhaulCampaignConfig.Instance?.TownsGoldMultiplier > 0)
            baseValue *= BasicOverhaulCampaignConfig.Instance.TownsGoldMultiplier;
        
        return baseValue;
    }
}