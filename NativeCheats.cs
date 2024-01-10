using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BasicOverhaul;

public static class NativeCheats
{
    [BasicCheat("Unlock crafting pieces")]
    public static string UnlockCraftingPieces(List<string> strings)
    {
        return CampaignCheats.UnlockCraftingPieces(new List<string>());
    }
    
    [BasicCheat("Heal main party")]
    public static string HealMainParty(List<string> strings)
    {
        return CampaignCheats.HealMainParty(new List<string>());
    }
}