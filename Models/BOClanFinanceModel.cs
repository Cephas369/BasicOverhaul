using System.Linq;
using BasicOverhaul.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models;

internal class BOClanFinanceModel : DefaultClanFinanceModel
{
    public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false,
        bool applyWithdrawals = false, bool includeDetails = false)
    {
        ExplainedNumber baseNumber = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);
        if (clan == Clan.PlayerClan)
            foreach (Settlement settlement in clan.Settlements.Where(x =>
                         x.IsTown && SlaveBehavior.Instance?.SlaveData.ContainsKey(x.StringId) == true &&
                         SlaveBehavior.Instance.SlaveData[x.StringId].DestinationType == DestinationTypes.Clan))
            {
                float profit = SlaveBehavior.Instance.SlaveData[settlement.StringId].SlaveAmount * 4;
                baseNumber.Add(profit,
                    new TextObject("{=bo_town_slavery}{TOWN} Slavery").SetTextVariable("TOWN", settlement.Name));
            }

        return baseNumber;
    }
}