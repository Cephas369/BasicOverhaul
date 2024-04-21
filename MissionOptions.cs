using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using FaceGen = TaleWorlds.Core.FaceGen;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
// ReSharper disable All

namespace BasicOverhaul
{
    public static class MissionOptions
    {
        public static (float combatmax, float max) SpeedOnCheat;
        public static bool IsPlayerDamageOp;
        public static bool PlayerInvincible;
        public static bool MountInvincible;
        
        [BasicOption("{=cheat_desc.9}Set player speed", new []{ "{=speed_amount}Speed amount" }, true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("increase_player_speed", "bo_misson")]
        [UsedImplicitly]
        private static string SetPlayerSpeed(List<string> strings)
        {
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 parameter: bo_misson.increase_player_speed [Amount]";

            if (Mission.Current == null || Agent.Main == null)
                return "You must be in a mission and your character must be alive!";

            bool isNumber = int.TryParse(strings[0], out int amount);

            if (!isNumber)
                return "Amount parameter must be a number!";

            if (Agent.Main != null)
            {
                Agent main = Agent.Main;
                main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, amount);
                main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, amount);
                main.UpdateCustomDrivenProperties();
                SpeedOnCheat = (main.AgentDrivenProperties.CombatMaxSpeedMultiplier, main.AgentDrivenProperties.MaxSpeedMultiplier);
            }
            
            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.10}Increase mount speed", new []{ "{=speed_amount}Speed amount" }, true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("increase_mount_speed", "bo_misson")]
        [UsedImplicitly]
        private static string IncreaseMountSpeed(List<string> strings)
        {
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 parameter: bo_misson.increase_mount_speed [Amount]";

            if (Mission.Current == null || Agent.Main == null)
                return "You must be in a mission and your character must be alive!";

            if (Agent.Main.MountAgent == null)
                return "You must be mounting a horse!";

            bool isNumber = int.TryParse(strings[0], out int amount);

            if (!isNumber)
                return "Amount parameter must be a number!";

            Agent horseAgent = Agent.Main.MountAgent;
            horseAgent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MountSpeed, horseAgent.GetAgentDrivenPropertyValue(DrivenProperty.MountSpeed) + amount);
            horseAgent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.TopSpeedReachDuration, horseAgent.GetAgentDrivenPropertyValue(DrivenProperty.TopSpeedReachDuration) + amount);
            horseAgent.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MountDashAccelerationMultiplier, horseAgent.GetAgentDrivenPropertyValue(DrivenProperty.MountDashAccelerationMultiplier) + amount);
            horseAgent.UpdateCustomDrivenProperties();

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.11}Enabe/disable mount invincible ({VALUE})", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("make_mount_invincible", "bo_misson")]
        [UsedImplicitly]
        private static string MakeHorseInvincible(List<string> strings)
        {
            if (Mission.Current == null || Agent.Main == null)
                return "You must be in a mission and your character must be alive!";

            if (Agent.Main.MountAgent == null)
                return "You must be mounting a horse!";

            Agent horseAgent = Agent.Main.MountAgent;
            horseAgent.OnAgentHealthChanged += (agent, health, newHealth) =>
            {
                agent.Health = agent.HealthLimit;
            };

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.12}Enabe/disable player invincible ({VALUE})", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("make_player_invincible", "bo_misson")]
        [UsedImplicitly]
        private static string MakePlayerInvincible(List<string> strings)
        {
            if (Mission.Current == null || Agent.Main == null)
                return "You must be in a mission and your character must be alive!";
            
            PlayerInvincible = !PlayerInvincible;

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.13}Enable/disable OP player damage ({VALUE})", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("switch_player_damage_op", "bo_misson")]
        [UsedImplicitly]
        private static string MakePlayerDamageOP(List<string> strings)
        {
            if (Mission.Current == null || Agent.Main == null)
                return "You must be in a mission and your character must be alive!";
            
            IsPlayerDamageOp = !IsPlayerDamageOp;

            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.14}Spawn character", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_character", "bo_misson")]
        [UsedImplicitly]
        private static string SpawnCharacter(List<string> strings)
        {
            if (Agent.Main == null)
                return "You must be in a mission and your hero must be alive.";
            
            List<InquiryElement> characterElements = MBObjectManager.Instance.GetObjectTypeList<BasicCharacterObject>()
                .Where(x=>x.IsSoldier && !x.IsObsolete || (x is CharacterObject characterObject && characterObject.Occupation == Occupation.Mercenary))
                .Select(x=>new InquiryElement(x, x.Name.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(x))))
                .OrderBy(x=>x.Title).ToList();
            
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData("Characters", "", characterElements, true, 0, 1,
                    "Select", "", elements =>
                    {
                        if (Agent.Main == null || elements.IsEmpty())
                            return;
                        
                        BasicCharacterObject characterObject = (BasicCharacterObject)elements[0].Identifier;
                        InformationManager.ShowTextInquiry(new TextInquiryData("Amount", "", true, false, "Next",
                            "", s =>
                            {
                                if (int.TryParse(s, out int number))
                                {
                                    InformationManager.ShowTextInquiry(new TextInquiryData("Ally or Enemy ?", "", true, false, "Spawn",
                                        "", s =>
                                        {
                                            s = s.ToLower();
                                            if(s == "ally" || s == "enemy")
                                                for (int i = 0; i < number; i++)
                                                    SpawnCharacterAgent(characterObject, s == "ally");
                                            else
                                                InformationManager.DisplayMessage(new InformationMessage("Value must be 'ally' or 'enemy'."));
                                            
                                        }, null), true);
                                }
                            }, null), true);
                    }, null), true);
            return GameTexts.FindText("str_done").ToString();
        }
        
        [BasicOption("{=cheat_desc.15}Spawn weapon", isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_weapon", "bo_misson")]
        [UsedImplicitly]
        private static string SpawnWeapon(List<string> strings)
        {
            if (Agent.Main == null)
                return "You must be in a mission and your hero must be alive.";

            List<InquiryElement> weaponElements = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
                .Where(x=>x.HasWeaponComponent)
                .Select(x=>new InquiryElement(x, x.Name.ToString(), new ImageIdentifier(x)))
                .OrderBy(x=>x.Title).ToList();
            
            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData("Weapons", "", weaponElements, true, 0, 1,
                    "Spawn", "", elements =>
                    {
                        if (Agent.Main == null)
                            return;
                        ItemObject itemObject = (ItemObject)elements[0].Identifier;
                        MissionWeapon missionWeapon = new MissionWeapon(itemObject, new ItemModifier(), Banner.CreateOneColoredEmptyBanner(1));
                        MatrixFrame frame = Agent.Main.Frame;
                        Mission.Current?.SpawnWeaponWithNewEntityAux(missionWeapon, Mission.WeaponSpawnFlags.WithPhysics, frame, 0, null, false);
                    }, null), true);
            
            return GameTexts.FindText("str_done").ToString();
        }

        [BasicOption("{=cheat_desc.16}Disable/enable agents AI", new []{ "{=0_disable_1_enable}0 = Disable | 1 = Enable" }, isCheat: true)]
        [CommandLineFunctionality.CommandLineArgumentFunction("disable_agents_ai", "bo_misson")]
        [UsedImplicitly]
        private static string DisableAi(List<string> strings)
        {
            string formatError = "Format uses 1 parameter: bo_misson.spawn_character [0 | 1]";
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return formatError;

            if (Mission.Current == null)
                return "You must be in a mission!";

            bool isNumber = int.TryParse(strings[0], out int number);
            if (!isNumber || number < 0 || number > 1)
                return "First parameter must be 1 or 0.";

            bool boolean = !Convert.ToBoolean(number);

            foreach (Agent agent in Mission.Current.Agents)
            {
                agent.SetIsAIPaused(boolean);
            }
            
            return GameTexts.FindText("str_done").ToString();
        }
        private static void SpawnCharacterAgent(BasicCharacterObject character, bool isAlly)
        {
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(character.Race, FaceGen.MonsterSuffixSettlement);

            Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements(character, true);

            PartyBase enemyParty = null;
            if(Campaign.Current != null)
                enemyParty = PlayerEncounter.EncounteredParty != null ? PlayerEncounter.EncounteredParty : PartyBase.MainParty;
            
            IAgentOriginBase originBase;
            if (Mission.Current.Mode == MissionMode.Battle)
                originBase = Campaign.Current != null
                    ? new PartyAgentOrigin(isAlly ? PartyBase.MainParty : enemyParty, character as CharacterObject)
                    : new BasicBattleAgentOrigin(character);
            else
                originBase = new SimpleAgentOrigin(character);

            AgentBuildData agentBuildData = new AgentBuildData(originBase).Equipment(randomEquipmentElements)
                .Monster(monsterWithSuffix);
            
            if (Mission.Current.Mode == MissionMode.Battle)
                agentBuildData.Formation(Mission.Current.DefenderTeam?.GetFormation(character.GetFormationClass()));
            else
            {
                Vec3 initialPosition = Agent.Main.Position;
                Vec2 rotation = initialPosition.AsVec2;

                agentBuildData.InitialPosition(in initialPosition).InitialDirection(in rotation);
            }

            agentBuildData.Team(isAlly ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam);
            
            Agent agent = Mission.Current.SpawnAgent(agentBuildData, true);
            agent.TeleportToPosition(Agent.Main.Position);
            agent.SetWatchState(Agent.WatchState.Alarmed);
        }
    }
}
