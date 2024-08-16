using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BasicOverhaul.Models;

public class BOExecutionRelationModel : DefaultExecutionRelationModel
{
    public override int GetRelationChangeForExecutingHero(Hero victim, Hero hero, out bool showQuickNotification)
    {
        int baseValue = base.GetRelationChangeForExecutingHero(victim, hero, out showQuickNotification);
        if (BasicOverhaulGlobalConfig.Instance?.DisableExecutionRelationPenalty == true)
            return 0;
        
        return baseValue;
    }
}