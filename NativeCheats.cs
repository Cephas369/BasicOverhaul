using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace BasicOverhaul;

public static class NativeCheats
{
    [BasicOption("Unlock crafting pieces", isCheat: true)]
    public static string UnlockCraftingPieces(List<string> strings)
    {
        return CampaignCheats.UnlockCraftingPieces(new List<string>());
    }
    
    [BasicOption("Heal main party", isCheat: true)]
    public static string HealMainParty(List<string> strings)
    {
        if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            return CampaignCheats.ErrorType;
        for (int index = 0; index < PartyBase.MainParty.MemberRoster.Count; ++index)
        {
            TroopRosterElement elementCopyAtIndex = PartyBase.MainParty.MemberRoster.GetElementCopyAtIndex(index);
            if (elementCopyAtIndex.Character.IsHero)
                elementCopyAtIndex.Character.HeroObject.Heal(elementCopyAtIndex.Character.HeroObject.MaxHitPoints);
            else
                MobileParty.MainParty.Party.AddToMemberRosterElementAtIndex(index, 0, -PartyBase.MainParty.MemberRoster.GetElementWoundedNumber(index));
        }
        return "Success";
    }
}