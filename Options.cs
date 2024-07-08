using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Mission = TaleWorlds.MountAndBlade.Mission;

namespace BasicOverhaul
{
    public static class Options
    {
        [BasicOption("{=cheat_desc.1}Set campaign speed", new []{ "{=speed}Speed" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("set_campaign_speed", "bo")]
        [UsedImplicitly]
        public static string SetCampaignSpeed(List<string> strings)
        {
            string result = "Format is bo.set_campaign_speed [positive speedUp multiplier].";
            if (Campaign.Current == null || Mission.Current != null)
                return "You must be in the campaign map.";
            
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
            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.2}Know all heroes", isCheat: true)]
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

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.8}Add food to party", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("add_food", "bo")]
        [UsedImplicitly]
        private static string SpawnWeapon(List<string> strings)
        {
            if (Campaign.Current == null)
                return "You must be in a campaign.";

            List<ItemObject> foods = new();

            int max = (int)(PartyBase.MainParty.NumberOfAllMembers * 1.5);
            for (int i = 0; i < max; i++)
            {
                ItemObject random = MBObjectManager.Instance.GetObjectTypeList<ItemObject>().Where(x => x.IsFood)
                    .GetRandomElementInefficiently();
                int randomNumber = MBRandom.RandomInt(0, max - i-1);
                PartyBase.MainParty.ItemRoster.AddToCounts(random, randomNumber);
                i += randomNumber;
            }

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.3}Remove desertion system")]
        [CommandLineFunctionality.CommandLineArgumentFunction("destroy_desertion_system", "bo")]
        [UsedImplicitly]
        public static string DestroyDeserterParties(List<string> strings)
        {
            if (Campaign.Current == null)
                return "Campaign was not started.";
            
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=are_you_sure}Are you sure ? This is irreversible.").ToString(), null, true, true,
                new TextObject("{=bo_yes}Yes").ToString(), new TextObject("{=bo_no}No").ToString(), () =>
                {
                    List<MobileParty> deserterParties = MobileParty.All.Where(x => x.StringId.Contains("deserter")).ToList();
            
                    for(int i = deserterParties.Count() - 1; i >= 0; i--)
                        DestroyPartyAction.Apply(PartyBase.MainParty, deserterParties[i]);

                    Clan.All.Remove(Clan.FindFirst(x => x.StringId == "deserters"));
                }, null));

            return GameTexts.FindText("str_done").ToString();
        }

        [BasicOption("{=cheat_desc.4}Maximize settlement walls", new []{ "{=settlement_name}Settlement name", "{=level}Level" }, isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_settlement_levels", "bo")]
        [UsedImplicitly]
        public static string MaximizeSettlementLevels(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 1) && !CampaignCheats.CheckParameters(strings, 2))
                return "Format uses 1 settlement name parameters without spaces: bo.maximize_settlement_levels [Settlement] [Level?]";
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

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.5}Maximize player stats", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_player", "bo")]
        [UsedImplicitly]
        public static string MaximizePlayer(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            MaximizeHero(Hero.MainHero);
            
            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.6}Maximize clan hero stats", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_clan_hero", "bo")]
        [UsedImplicitly]
        public static string MaximizeHero(List<string> strings)
        {
            if (Campaign.Current == null || Clan.PlayerClan == null)
                return "You must be in a campaign and you must have a clan.";

            List<InquiryElement> heroes = Clan.PlayerClan.Heroes.Where(x=>x.IsAlive).Select(x =>
                new InquiryElement(x, x.Name.ToString(),
                    new ImageIdentifier(CharacterCode.CreateFrom(x.CharacterObject)))).ToList();
            
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("Heroes", null, heroes, true, 0,
                1, "Done", null, list =>
                {
                    if (list.Any() && list[0].Identifier is Hero hero)
                        MaximizeHero(hero);
                    
                }, null));
            

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.17}Enable/disable cheat mode ({VALUE})")]
        [CommandLineFunctionality.CommandLineArgumentFunction("switch_cheat_mode", "bo")]
        [UsedImplicitly]
        public static string SwitchCheatMode(List<string> strings)
        {
            if (Campaign.Current == null || BasicOverhaulCampaignConfig.Instance == null)
                return "You must be in a campaign.";

            if (BasicOverhaulGlobalConfig.Instance?.EnableSwitchCheatMode == false)
                return "You must activate the enable switch cheat mode in the mod config to use this.";

            bool current = (bool)Helpers.CheatModeField.GetValue(null);
            BasicOverhaulCampaignConfig.Instance.CheatModeEnabled = !current;
            Helpers.CheatModeField.SetValue(null, !current);

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.7}Maximize every kingdom stats", new []{ "Kingdom Name" }, isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("maximize_kingdom", "bo")]
        [UsedImplicitly]
        private static string AddKingdomMoney(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;
            
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 kingdom ID parameter: bo.maximize_kingdom [Kingdom]";

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

            foreach (Hero hero in kingdom.Heroes)
                MaximizeHero(hero);
            
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

            return GameTexts.FindText("str_done").ToString();
        }

        private static void MaximizeHero(Hero hero)
        {
            hero.ChangeHeroGold(999999999-hero.Gold);

            if (hero.Clan != null)
            {
                hero.Clan.Influence = 9999999;
                hero.Clan.Renown = 5000000;
            }

            foreach (SkillObject skill in Game.Current.ObjectManager.GetObjectTypeList<SkillObject>())
                hero.SetSkillValue(skill, 300);
            
            foreach(CharacterAttribute characterAttribute in MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>())
                AccessTools.Method(typeof(Hero), "SetAttributeValueInternal").Invoke(hero, new object[] { characterAttribute, 10 });
            
            foreach (PerkObject perkObject in PerkObject.All)
                AccessTools.Method(typeof(Hero), "SetPerkValueInternal").Invoke(hero, new object[] { perkObject, true });
            
        }
    }
}