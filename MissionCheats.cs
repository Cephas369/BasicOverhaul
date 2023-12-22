using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
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
using TaleWorlds.Engine;

namespace BasicOverhaul
{
    internal sealed class MissionCheats
    {
        
        [BasicCheat("Increase player speed", new []{ "Speed amount" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("increase_player_speed", "bo_misson")]
        [UsedImplicitly]
        private static string IncreasePlayerSpeed(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;


            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 parameter: bo_misson.increase_player_stat [Amount]";

            if (Mission.Current == null)
                return "You must be in a mission!";

            bool isNumber = int.TryParse(strings[0], out int amount);

            if (!isNumber)
                return "Amount parameter must be a number!";

            if (Agent.Main != null)
            {
                Agent main = Agent.Main;
                main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, main.GetAgentDrivenPropertyValue(DrivenProperty.MaxSpeedMultiplier) + amount);
                main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, main.GetAgentDrivenPropertyValue(DrivenProperty.CombatMaxSpeedMultiplier) + amount);
                main.UpdateCustomDrivenProperties();
            }
            
            return "Done!";
        }
        
        [BasicCheat("Spawn character", new []{ "Character ID" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_character", "bo_misson")]
        [UsedImplicitly]
        private static string SpawnCharacter(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            string formatError = "Format uses 2 parameters: bo_misson.spawn_character [CharacterId] [ally | enemy]";
            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
                return formatError;

            if (Agent.Main == null)
                return "You must be in a mission and your hero must be alive.";

            CharacterObject characterObject = CharacterObject.Find(strings[0]);
            if (characterObject == null)
                return "Character doens't exist.";

            string team = strings[1];
            if (team != "ally" && team != "enemy")
                return formatError;

            SpawnCharacterAgent(characterObject, team == "ally");

            return "Done!";
        }
        
        [BasicCheat("Disable/enable agents ai", new []{ "0 = Disable | 1 = Enable" })]
        [CommandLineFunctionality.CommandLineArgumentFunction("disable_agents_ai", "bo_misson")]
        [UsedImplicitly]
        private static string DisableAi(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            string formatError = "Format uses 1 parameter: bo_misson.spawn_character [0 | 1]";
            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return formatError;

            if (Mission.Current == null)
                return "You must be in a mission!";

            bool isNumber = int.TryParse(strings[0], out int number);
            if (!isNumber || number < 0 || number > 1)
                return "First parameter must be 1 or 0.";

            foreach (Agent agent in Mission.Current.Agents)
            {
                agent.SetIsAIPaused(isNumber);
            }
            
            return "Done!";
        }

        /*[CommandLineFunctionality.CommandLineArgumentFunction("test", "bo_misson")]
        [UsedImplicitly]
        private static string Test(List<string> strings)
        {


            if (Mission.Current == null)
                return "You must be in a mission!";

            Mission.Current.AddMissionBehavior(new MissionTest());

           Agent agent = Mission.Current.Agents.Find(x => !Agent.Main.IsEnemyOf(x) && x != Agent.Main);

           WorldPosition wp = Mission.Current.GetStraightPathToTarget(MissionTest.positions.Item1, agent.GetWorldPosition());
            agent.SetScriptedPositionAndDirection(ref wp, wp.AsVec2.RotationInRadians, true, Agent.AIScriptedFrameFlags.GoToPosition);
            agent.ResetGuard();
            agent.SetWatchState(Agent.WatchState.Alarmed);
            MissionTest.target = agent;
            
           ActionIndexCache myAction = ActionIndexCache.Create("act_pickup_down_begin");
            Agent.Main.SetActionChannel(0, myAction, true);

            return "Done!";
        }

        private class MissionTest : MissionBehavior
        {
            public static (Vec2, Vec3) positions;
            public static Agent target;
            public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

            public override void OnAgentDeleted(Agent affectedAgent)
            {
                positions = (affectedAgent.Position.AsVec2, affectedAgent.Position);
            }

            public override void OnMissionTick(float dt)
            {
                if (target?.Position.DistanceSquared(positions.Item1.ToVec3()) <= 2)
                {
                    target.ClearTargetFrame();
                    ActionIndexCache myAction = ActionIndexCache.Create("act_pickup_down_begin");
                    Agent.Main.SetActionChannel(0, myAction, true);
                }
            }
        }*/


        private static void SpawnCharacterAgent(CharacterObject character, bool isAlly)
        {
            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(character.Race, FaceGen.MonsterSuffixSettlement);

            Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements(character, true);

            PartyBase enemyParty = PlayerEncounter.EncounteredParty != null ? PlayerEncounter.EncounteredParty : PartyBase.MainParty;

            AgentBuildData agentBuildData = new AgentBuildData(new PartyAgentOrigin(isAlly ? PartyBase.MainParty : enemyParty, character)).Equipment(randomEquipmentElements).Monster(monsterWithSuffix).Team(isAlly ? Mission.Current.PlayerTeam : Mission.Current.PlayerEnemyTeam).Formation(Mission.Current.DefenderTeam.GetFormation(character.GetFormationClass()));

            Agent agent = Mission.Current.SpawnAgent(agentBuildData, true);
            agent.TeleportToPosition(Agent.Main.Position);
            agent.SetWatchState(Agent.WatchState.Alarmed);
        }
    }
}
