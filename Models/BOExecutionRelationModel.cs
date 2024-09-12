using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BasicOverhaul.Models;

public class BOExecutionRelationModel : DefaultExecutionRelationModel
{
    private ExecutionRelationModel _previousModel;

    public BOExecutionRelationModel(ExecutionRelationModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override int GetRelationChangeForExecutingHero(Hero victim, Hero hero, out bool showQuickNotification)
    {
        int baseValue = _previousModel.GetRelationChangeForExecutingHero(victim, hero, out showQuickNotification);
        if (BasicOverhaulGlobalConfig.Instance?.DisableExecutionRelationPenalty == true)
            return 0;
        
        return baseValue;
    }
}