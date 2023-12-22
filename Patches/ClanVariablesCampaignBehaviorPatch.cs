using HarmonyLib;
using System.Text;
using TaleWorlds.CampaignSystem;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(CampaignEventDispatcher), "DailyTickClan")]
public class ClanVariablesCampaignBehaviorPatch
{
    public static bool Prefix(Clan clan)
    {
        if (BasicOverhaulConfig.Instance?.EnableDeserterParties == true && clan.StringId=="deserters")
            return false;
        return true;
    }
}

