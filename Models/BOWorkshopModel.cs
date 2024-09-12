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
    private WorkshopModel _previousModel;

    public BOWorkshopModel(WorkshopModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override ExplainedNumber GetEffectiveConversionSpeedOfProduction(Workshop workshop, float speed, bool includeDescription)
    {
        ExplainedNumber baseValue = _previousModel.GetEffectiveConversionSpeedOfProduction(workshop, speed, includeDescription);
        if (BasicOverhaulCampaignConfig.Instance?.WorkshopProductionSpeed > 0)
            baseValue.AddFactor(BasicOverhaulCampaignConfig.Instance.WorkshopProductionSpeed, new TextObject("Basic Overhaul"));
        return baseValue;
    }
}