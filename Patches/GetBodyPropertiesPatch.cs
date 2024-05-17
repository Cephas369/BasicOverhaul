using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;


[HarmonyPatch(typeof(BasicCharacterObject), "GetBodyProperties")]
public static class BasicCharacterObjectGetBodyPropertiesPatch
{
    public static void Postfix(Equipment equipment, ref BodyProperties __result, BasicCharacterObject __instance, int seed = -1)
    {
        RandomBodiesHelper.GetRandomBody(ref __result, __instance);
    }
}

[HarmonyPatch(typeof(CharacterObject), "GetBodyProperties")]
public static class CharacterObjectGetBodyPropertiesPatch
{
    public static void Postfix(Equipment equipment, ref BodyProperties __result, BasicCharacterObject __instance, int seed = -1)
    {
        RandomBodiesHelper.GetRandomBody(ref __result, __instance);
    }
}

public static class RandomBodiesHelper
{
    private static float GetRandomBuild(BasicCharacterObject characterObject)
    {
        List<float> values = new();
        float maxBuild = characterObject.IsFemale ? 0.8f : 1f;
        for (float f = characterObject.IsFemale ? 0.1f : 0.25f; f < maxBuild; f += 0.01f)
            values.Add(f);
        for (int i = 0; i < characterObject.GetBattleTier(); i++)
            values.Add(MBRandom.RandomFloatRanged(0.6f, 1f));
        
        return values[MBRandom.RandomInt(0, values.Count - 1)];
    }
    
    private static float GetRandomWeight()
    {
        List<float> values = new();
        for (float f = 0.05f; f < 1f; f += 0.01f)
            values.Add(f);
        if (Campaign.Current != null && MobileParty.MainParty != null)
        {
            for (float f = 0f; f < MobileParty.MainParty.Food * 0.01f; f += 0.1f)
            {
                values.Add(MBRandom.RandomFloatRanged(MobileParty.MainParty.Food * 0.01f, 1f));
            }
        }
        
        return values[MBRandom.RandomInt(0, values.Count - 1)];
    }
    
    public static void GetRandomBody(ref BodyProperties bodyProperties, BasicCharacterObject characterObject)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableRandomHumanSizes == true && !characterObject.IsHero)
        {
            DynamicBodyProperties dynamicBodyProperties = bodyProperties.DynamicProperties;
            dynamicBodyProperties.Weight = GetRandomWeight();
            dynamicBodyProperties.Build = GetRandomBuild(characterObject);
            bodyProperties = new BodyProperties(dynamicBodyProperties, bodyProperties.StaticProperties);
        }
    }
}