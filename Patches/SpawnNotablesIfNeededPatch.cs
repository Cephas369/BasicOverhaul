using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(SettlementHelper), "SpawnNotablesIfNeeded")]
public class SpawnNotablesIfNeededPatch
{
    public static bool Prefix(Settlement settlement)
    {
        var list = new List<Occupation>();
        if (settlement.IsCastle)
        {
            list = new List<Occupation>
            {
                Occupation.Headman
            };

            var randomFloat = MBRandom.RandomFloat;
            var num = 0;
            foreach (var occupation in list)
            {
                num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                    occupation);
            }

            var count = settlement.Notables.Count;
            var num2 = settlement.Notables.Any() ? (num - settlement.Notables.Count) / (float)num : 1f;
            num2 *= MathF.Pow(num2, 0.36f);
            if (randomFloat <= num2 && count < num)
            {
                var list2 = new List<Occupation>();
                foreach (var occupation2 in list)
                {
                    var num3 = 0;
                    using (var enumerator2 = settlement.Notables.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                            {
                                num3++;
                            }
                        }
                    }

                    var targetNotableCountForSettlement =
                        Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                            occupation2);
                    if (num3 < targetNotableCountForSettlement)
                    {
                        list2.Add(occupation2);
                    }
                }

                if (list2.Count > 0)
                {
                    Hero hero = HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement(), settlement);
                    hero.SupporterOf = settlement.OwnerClan;
                    EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                }
            }
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Campaign), "DailyTickSettlement")]
public static class FixNotableGovernors
{
    private static FieldInfo _heroPerks = AccessTools.Field(typeof(Hero), "_heroPerks");
    public static void Prefix(Settlement settlement)
    {
        if ((settlement.IsTown || settlement.IsCastle) && settlement.Town.Governor != null && settlement.Town.Governor.Clan == null)
        {
            if (settlement.Town.Governor.GetPerkValue(DefaultPerks.Charm.Virile))
            {
                CharacterPerks heroPerks = (CharacterPerks)_heroPerks.GetValue(settlement.Town.Governor);
                heroPerks.SetPropertyValue(DefaultPerks.Charm.Virile, 0);
                _heroPerks.SetValue(settlement.Town.Governor, heroPerks);
            }
        }
    }
}