using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BasicOverhaul.Behaviors;
using BasicOverhaul.Manager;
using BasicOverhaul.Models;
using BasicOverhaul.Patches;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace BasicOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        private bool _isMenuOpened = false;
        private static Harmony Harmony;
        public static readonly Dictionary<string, InputKey> PossibleKeys = new();
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony = new Harmony("com.basic_overhaul");
            Harmony.PatchAll();

            Harmony.Patch(AccessTools.Method(typeof(System.Xml.XmlNode).Assembly.GetTypes().First(x=>x.Name =="XmlLoader"), "Load"),
                postfix: AccessTools.Method(typeof(XmlGUILoadPatch), "Postfix"));
            Harmony.Patch(AccessTools.Method(typeof(MapEvent).Assembly.GetTypes().First(x => x.Name == "LootCollector"), "LootCasualties"),
                prefix: AccessTools.Method(typeof(LootCollectorPatch), "Prefix"));
            
            foreach (string inputKey in Enum.GetNames(typeof(InputKey)))
            {
                PossibleKeys.Add(inputKey, (InputKey)Enum.Parse(typeof(InputKey), inputKey));
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            BasicOverhaulHotkeyManager.Initialize();
        }
        
        private bool IsSiege => Campaign.Current != null && (MapEvent.PlayerMapEvent?.IsSiegeAssault == true || MapEvent.PlayerMapEvent?.IsSiegeAmbush == true || MapEvent.PlayerMapEvent?.IsSiegeOutside== true);
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if(BasicOverhaulGlobalConfig.Instance?.EnableWeaponryOrder == true && mission.Mode != MissionMode.Battle && mission.CombatType != Mission.MissionCombatType.ArenaCombat
               && mission.CombatType != Mission.MissionCombatType.NoCombat && !mission.HasMissionBehavior<TournamentBehavior>())
                mission.AddMissionBehavior(new WeaponryOrderMissionBehavior());
            
            mission.AddMissionBehavior(new HorseCallMissionLogic());
            
            if (Campaign.Current != null && mission.CombatType == Mission.MissionCombatType.Combat  && BasicOverhaulGlobalConfig.Instance?.EnablePackMule == true && !IsSiege)
                mission.AddMissionBehavior(new PackMuleBehavior());
            
            mission.AddMissionBehavior(new MiscMissionLogic());
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if(gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new MiscBehavior());
                
                if(BasicOverhaulGlobalConfig.Instance?.EnableDialogs == true)
                    campaignGameStarter.AddBehavior(new MessengerBehavior());
                
                if(BasicOverhaulGlobalConfig.Instance?.EnableDeserterParties == true)
                    campaignGameStarter.AddBehavior(new DesertionBehavior());
                
                if (BasicOverhaulGlobalConfig.Instance?.EnableSlaveSystem == true)
                {
                    campaignGameStarter.AddBehavior(new SlaveBehavior());
                    campaignGameStarter.AddModel(new BOClanFinanceModel(campaignGameStarter.GetExistingModel<ClanFinanceModel>()));
                    campaignGameStarter.AddModel(new BOBuildingConstructionModel(campaignGameStarter.GetExistingModel<BuildingConstructionModel>()));
                    
                }

                if (BasicOverhaulGlobalConfig.Instance?.EnableGovernorNotables == true)
                {
                    campaignGameStarter.AddBehavior(new NotableBehavior());
                    campaignGameStarter.AddModel(new BONotableSpawnModel(campaignGameStarter.GetExistingModel<NotableSpawnModel>()));
                }
                
                campaignGameStarter.AddModel(new BOSettlementProsperityModel(campaignGameStarter.GetExistingModel<SettlementProsperityModel>()));
                campaignGameStarter.AddModel(new BOPartySizeLimitModel(campaignGameStarter.GetExistingModel<PartySizeLimitModel>()));
                campaignGameStarter.AddModel(new BOBattleRewardModel(campaignGameStarter.GetExistingModel<BattleRewardModel>()));
                campaignGameStarter.AddModel(new BOVolunteerModel(campaignGameStarter.GetExistingModel<VolunteerModel>()));
                campaignGameStarter.AddModel(new BOAgentStatCalculateModel(campaignGameStarter.GetExistingModel<AgentStatCalculateModel>()));
                campaignGameStarter.AddModel(new BOWorkshopModel(campaignGameStarter.GetExistingModel<WorkshopModel>()));
                campaignGameStarter.AddModel(new BOExecutionRelationModel(campaignGameStarter.GetExistingModel<ExecutionRelationModel>()));
            }
            else
            {
                try
                {
                    gameStarterObject.AddModel(
                        new BOCustomAgentStatCalculateModel(
                            gameStarterObject.GetExistingModel<AgentStatCalculateModel>()));
                }
                catch (Exception)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Basic Overhaul: Error applying models on custom battle", Colors.Red));
                }
            }

            MenuManager.InitializeCheats();
        }
    }
    public class BasicOverhaulSaveSystem : SaveableTypeDefiner
    {
        public BasicOverhaulSaveSystem() : base(63643_4241) { }
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(DesertionBehavior.DeserterPartyComponent), 1);
            AddEnumDefinition(typeof(DestinationTypes), 2);
            AddClassDefinition(typeof(TownSlaveData), 3);
            AddClassDefinition(typeof(MessengerBehavior), 4);
            AddClassDefinition(typeof(MessengerBehavior.MessengerPartyComponent), 5);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<MobileParty, Settlement>));
            ConstructContainerDefinition(typeof(Dictionary<string, TownSlaveData>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, CampaignTime>));
            ConstructContainerDefinition(typeof(List<MobileParty>));
        }
    }
}

