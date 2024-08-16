using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using BasicOverhaul.Behaviors;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SandBoxManager), "OnCampaignStart")]
public static class LoadXMLPatch
{
    private static PropertyInfo IsBanditFaction = AccessTools.Property(typeof(Clan), "IsBanditFaction");
    public static void Prefix(CampaignGameStarter gameInitializer, GameManagerBase gameManager, bool isSavedCampaign)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableDeserterParties == false && XmlResource.XmlInformationList.Any(x => x.Name == "deserter_clan"))
            XmlResource.XmlInformationList.RemoveAll(x => x.Name == "deserter_clan");
    }
    
    public static void Postfix(CampaignGameStarter gameInitializer, GameManagerBase gameManager, bool isSavedCampaign)
    {
        if (DesertionBehavior.DesertersClan?.IsBanditFaction == false)
        {
            IsBanditFaction.SetValue(DesertionBehavior.DesertersClan, true);
        }
    }
}

[HarmonyPatch(typeof(BanditsCampaignBehavior), "SpawnAPartyInFaction")]
public static class BanditsCampaignBehaviorPatch
{
    public static bool Prefix(Clan selectedFaction)
    {
        if (selectedFaction == DesertionBehavior.DesertersClan)
        {
            return false;
        }
        
        return true;
    }
}

[HarmonyPatch(typeof(BanditPartyComponent))]
public static class BanditPartyComponentPatch
{
    [HarmonyPatch("CreateBanditParty")]
    public static void Postfix(
        string stringId,
        Clan clan,
        Hideout hideout,
        bool isBossParty,
        ref MobileParty __result)
    {
        if (clan == DesertionBehavior.DesertersClan)
        {
            __result = MobileParty.AllBanditParties.GetRandomElement();
            InformationManager.DisplayMessage(new InformationMessage("Attention: deserters from Basic Overhaul is conflicting with another mod", Colors.Red));
        }
    }
    
    [HarmonyPatch("CreateLooterParty")]
    public static void Postfix(
        string stringId,
        Clan clan,
        Settlement relatedSettlement,
        bool isBossParty,
        ref MobileParty __result)
    {
        if (clan == DesertionBehavior.DesertersClan)
        {
            __result = MobileParty.AllBanditParties.GetRandomElement();
            InformationManager.DisplayMessage(new InformationMessage("Attention: deserters from Basic Overhaul is conflicting with another mod", Colors.Red));
        }
    }
}