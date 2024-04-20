using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

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
        return CampaignCheats.HealMainParty(new List<string>());
    }
}