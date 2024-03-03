using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Hero), "SetSkillValue")]
public static class SetSkillValuePrefix
{
    public static void Prefix(SkillObject skill, ref int value)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableLimitSkill == true && value > 300)
            value = 300;
    }
}