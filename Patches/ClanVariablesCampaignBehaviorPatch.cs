using System;
using System.Linq;
using HarmonyLib;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(CampaignEventDispatcher), "DailyTickClan")]
public class ClanVariablesCampaignBehaviorPatch
{
    public static bool Prefix(Clan clan)
    {
        /*if (BasicOverhaulGlobalConfig.Instance?.EnableDeserterParties == true && clan.StringId=="deserters")
            return false;*/
        return true;
    }
}



[HarmonyPatch(typeof(EncyclopediaListVM), MethodType.Constructor, new Type[] {typeof(EncyclopediaPageArgs)})]
public static class EncyclopediaListClanWealthComparerPatch
{
    [HarmonyPostfix]
    public static void Postfix(EncyclopediaPageArgs args, EncyclopediaListVM __instance)
    {
        int index = __instance.Items.FindIndex(x => x.Id == "deserters");
        if (index > 0)
        {
            __instance.Items.RemoveAt(index);
        }
    }
}