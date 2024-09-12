using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Behaviors;

internal class BONotableSpawnModel : DefaultNotableSpawnModel
{
    private NotableSpawnModel _previousModel;

    public BONotableSpawnModel(NotableSpawnModel previousModel)
    {
        _previousModel = previousModel;
    }
    
    public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation) =>
        settlement.IsCastle && occupation == Occupation.Headman ? 1 : base.GetTargetNotableCountForSettlement(settlement, occupation);
}