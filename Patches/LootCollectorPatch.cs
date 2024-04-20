using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Roster;

namespace BasicOverhaul.Patches;
public static class LootCollectorPatch
{
    public static void Prefix(
        ICollection<TroopRosterElement> shareFromCasualties,
        ref float lootFactor)
    {
        if (BasicOverhaulCampaignConfig.Instance?.GlobalLootChance >= 0)
            lootFactor = BasicOverhaulCampaignConfig.Instance.GlobalLootChance;
    }
}