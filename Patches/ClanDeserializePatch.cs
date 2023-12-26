using System;
using System.Collections.Generic;
using System.Xml;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(ModuleHelper), "GetXmlPath")]
public static class LoadXMLPatch
{
    public static void Prefix(string moduleId, string xmlName, ref string __result)
    {
        if (BasicOverhaulConfig.Instance?.EnableDeserterParties == false && moduleId == "BasicOverhaul" && xmlName == "deserter_clan")
            __result = "";
    }
}