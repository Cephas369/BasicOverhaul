using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Behaviors
{
    internal class NotableBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement.Town == null || settlement.OwnerClan == null)
                return;
            
            if (settlement.IsCastle)
                SettlementHelper.SpawnNotablesIfNeeded(settlement);
        }
        public override void SyncData(IDataStore dataStore)
        {
        }
    }

    internal class BONotableSpawnModel : DefaultNotableSpawnModel
    {
        public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation) =>
            settlement.IsCastle && occupation == Occupation.Headman ? 1 : base.GetTargetNotableCountForSettlement(settlement, occupation);
    }
}
