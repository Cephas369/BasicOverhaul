using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul
{
    public class BasicCheat : Attribute
    {
        public string Description;
        public string[]? Parameters;

        public BasicCheat(string description, string[]? parameters = null)
        {
            Description = description;
            Parameters = parameters;
        }
    }
    internal sealed class Cheats
    {
        [BasicCheat("Set campaign speed", new []{ "speed" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("set_campaign_speed", "bo")]
        [UsedImplicitly]
        public static string SetCampaignSpeed(List<string> strings)
        {
            string result = "Format is bo.set_campaign_speed [positive speedUp multiplier].";
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
            {
                return result;
            }
            float num;
            if (!float.TryParse(strings[0], out num) || num <= 0f)
            {
                return result;
            }
            Campaign.Current.SpeedUpMultiplier = num;
            return "Done!";
        }
        
        [BasicCheat("Know all heroes")]
        [CommandLineFunctionality.CommandLineArgumentFunction("know_all_heroes", "bo")]
        [UsedImplicitly]
        public static string KnowAllHeroes(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;


            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                hero.SetHasMet();
            }

            return "Done!";
        }
        
        [BasicCheat("Destroy deserter parties")]
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_deserter_parties", "bo")]
        [UsedImplicitly]
        public static string DestroyDeserterParties(List<string> strings)
        {
            if (Campaign.Current == null)
                return "Campaign was not started.";

            foreach (MobileParty mobileParty in MobileParty.All.Where(x=>x.StringId.Contains("deserter")))
            {
                DestroyPartyAction.Apply(PartyBase.MainParty, mobileParty);
            }

            return "Done!";
        }

        [BasicCheat("Maximize settlement walls", new []{ "Settlement name", "Level" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_settlement_levels", "bo")]
        [UsedImplicitly]
        public static string MaximizeSettlementLevels(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 1) && !CampaignCheats.CheckParameters(strings, 2))
                return "Format uses 1 settlement name parameters without spaces: overhaul.maximize_settlement_levels [Settlement] [Level?]";
            int buildingLevel = 3;
            if(CampaignCheats.CheckParameters(strings, 2))
            {
                bool isnumber = int.TryParse(strings[1], out buildingLevel);
                if (!isnumber)
                    return "Level must be a number";
                if (buildingLevel < 0 || buildingLevel > 3)
                    return "Level must be a number between 0-3";
            }

            var b1 = strings[0].ToLower();

            Settlement? settlement = null;

            foreach (var k in Settlement.All)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1.ToLower().Replace(" ", ""))
                    settlement = k;
            }

            if (settlement is null)
                return "Could not find required settlement!";

            if (settlement.Town is null)
                return "Settlement is not town!";

            
            foreach(Building building in settlement.Town.Buildings)
            {
                building.CurrentLevel = buildingLevel;
            }

            return "Done!";
        }
        
        [BasicCheat("Maximize player stats")]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_player", "bo")]
        [UsedImplicitly]
        public static string MaximizePlayer(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            Hero.MainHero.ChangeHeroGold(999999999);
            Hero.MainHero.ChangeHeroGold(999999999);

            Clan.PlayerClan.Influence = 99999999;
            Clan.PlayerClan.Renown = 5000000;
            

            foreach (SkillObject skill in Game.Current.ObjectManager.GetObjectTypeList<SkillObject>())
            {
                
                Hero.MainHero.SetSkillValue(skill, 300);
            }
            foreach(CharacterAttribute characterAttribute in MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>())
            {
                AccessTools.Method(typeof(Hero), "SetAttributeValueInternal").Invoke(Hero.MainHero, new object[] { characterAttribute, 10 });
            }

            foreach (PerkObject perkObject in PerkObject.All)
            {
                AccessTools.Method(typeof(Hero), "SetPerkValueInternal").Invoke(Hero.MainHero, new object[] { perkObject, true });
            }
            return "Done!";
        }
        
        [BasicCheat("Maximize all kingdom stats", new []{ "Kingdom Name" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_kingdom", "bo")]
        [UsedImplicitly]
        private static string AddKingdomMoney(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;
            
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 kingdom ID parameter: overhaul.maximize_kingdom [Kingdom]";

            var b1 = strings[0].ToLower();

            Kingdom? kingdom = null;

            foreach (var k in Kingdom.All)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom = k;
            }

            if (kingdom is null)
                return "Could not find either required kingdom!";

            foreach(Hero hero in kingdom.Heroes)
            {
                hero.ChangeHeroGold(99999999);
                hero.Clan.Influence = 99999;
                foreach (SkillObject skill in Game.Current.ObjectManager.GetObjectTypeList<SkillObject>())
                {
                    hero.SetSkillValue(skill, 300);
                }
                foreach (CharacterAttribute characterAttribute in MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>())
                {
                    AccessTools.Method(typeof(Hero), "SetAttributeValueInternal").Invoke(hero, new object[] { characterAttribute, 10 });
                }

            }
            foreach(Settlement settlement in kingdom.Settlements)
            {
                if (settlement.IsTown || settlement.IsCastle)
                {
                    settlement.Town.Prosperity = 99999f;
                    settlement.Town.Security = 9999f;
                }
                    
            }
            foreach (Village village in kingdom.Villages)
            {
                village.Hearth = 99999f;
            }

            return "Done!";
        }
        
        [BasicCheat("Clear player's party")]
        [CommandLineFunctionality.CommandLineArgumentFunction("clear_main_party", "bo")]
        [UsedImplicitly]
        public static string CleanMainParty(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;


            foreach (var troopElement in PartyBase.MainParty.MemberRoster.GetTroopRoster())
            {
                if(troopElement.Character != CharacterObject.PlayerCharacter)
                    PartyBase.MainParty.MemberRoster.RemoveTroop(troopElement.Character);
            }

            return "Done!";
        }
    }
}