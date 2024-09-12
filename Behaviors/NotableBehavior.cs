using Helpers;
using TaleWorlds.CampaignSystem;
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
}
