using System.Collections.Generic;
using System.Linq;
using BasicOverhaul.Behaviors;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BasicOverhaul;

public static class FixerCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("destroy_desertion_system", "bo")]
    [UsedImplicitly]
    public static string DestroyDeserterParties(List<string> strings)
    {
        if (Campaign.Current == null)
            return "Campaign was not started.";
            
        InformationManager.ShowInquiry(new InquiryData(new TextObject("{=are_you_sure}Are you sure ? This is irreversible.").ToString(), null, true, true,
            new TextObject("{=bo_yes}Yes").ToString(), new TextObject("{=bo_no}No").ToString(), () =>
            {
                List<MobileParty> deserterParties = MobileParty.All.Where(x => x.StringId.Contains("deserter")).ToList();
            
                for(int i = deserterParties.Count() - 1; i >= 0; i--)
                    DestroyPartyAction.Apply(PartyBase.MainParty, deserterParties[i]);

                Clan.All.Remove(Clan.FindFirst(x => x.StringId == "deserters"));
            }, null));

        return GameTexts.FindText("str_done").ToString();
    }
    
    [CommandLineFunctionality.CommandLineArgumentFunction("return_messengers", "bo")]
    [UsedImplicitly]
    public static string ReturnMessengerParties(List<string> strings)
    {
        if (Campaign.Current == null)
            return "Campaign was not started.";

        MessengerBehavior messengerBehavior = Campaign.Current.GetCampaignBehavior<MessengerBehavior>();
        if (messengerBehavior != null)
        {
            messengerBehavior.ReturnAllMessengers();
        }

        return GameTexts.FindText("str_done").ToString();
    }
}