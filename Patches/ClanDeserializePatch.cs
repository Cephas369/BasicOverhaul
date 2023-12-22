using System.Xml;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Clan), "Deserialize")]
public class ClanDeserializePatch
{
    public static bool Prefix(MBObjectManager objectManager, XmlNode node)
    {
        if (BasicOverhaulConfig.Instance?.EnableDeserterParties == false && node.Attributes?["id"].Value == "deserters")
            return false;
        return true;
    }
}