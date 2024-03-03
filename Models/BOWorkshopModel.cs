using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models;

public class BOWorkshopModel : DefaultWorkshopModel
{
    public override ExplainedNumber GetEffectiveConversionSpeedOfProduction(Workshop workshop, float speed, bool includeDescription)
    {
        ExplainedNumber baseValue = base.GetEffectiveConversionSpeedOfProduction(workshop, speed, includeDescription);
        if (BasicOverhaulCampaignConfig.Instance?.WorkshopProductionSpeed > 0)
            baseValue.AddFactor(BasicOverhaulCampaignConfig.Instance.WorkshopProductionSpeed, new TextObject("Basic Overhaul"));
        return baseValue;
    }
}