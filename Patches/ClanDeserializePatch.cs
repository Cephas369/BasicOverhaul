using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SandBoxManager), "OnCampaignStart")]
public static class LoadXMLPatch
{
    public static void Prefix(CampaignGameStarter gameInitializer, GameManagerBase gameManager, bool isSavedCampaign)
    {
        if (BasicOverhaulConfig.Instance?.EnableDeserterParties == false && XmlResource.XmlInformationList.Any(x => x.Name == "deserter_clan"))
            XmlResource.XmlInformationList.RemoveAll(x => x.Name == "deserter_clan");
    }
}