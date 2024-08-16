using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BasicOverhaul.Behaviors;
using BasicOverhaul.Models;
using BasicOverhaul.Patches;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
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
        private static readonly List<(BasicOption? Properties, MethodInfo Method)> CampaignCheats = new();
        private static readonly List<(BasicOption? Properties, MethodInfo Method)> MissionCheats = new();
        private static List<string> _currentParameters = new();
        private bool _isMenuOpened = false;
        private static Harmony Harmony;
        public static readonly Dictionary<string, InputKey> PossibleKeys = new();
        private InputKey _menuKey = InputKey.U;
        
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
        private void MakeMenuClosed() => _isMenuOpened = false;
        private void ApplyCheat(List<InquiryElement> inquiryElements)
        {
            MakeMenuClosed();
            if (!inquiryElements.Any())
                return;
            InquiryElement inquiry = inquiryElements[0];
            var cheatTuple = ((BasicOption? Properties, MethodInfo Method))inquiry.Identifier;
            
            string[]? parameters = cheatTuple.Properties?.Parameters?.Select(text=>text.ToString()).ToArray();

            if (parameters == null)
            {
                MakeMenuClosed();
                InformationManager.DisplayMessage(new InformationMessage((string)cheatTuple.Method.Invoke(null, new object[]{ null })));
                return;
            }
            
            List<Action<string>> affirmativeActions = new();
            _currentParameters = new();

            for (int i = 0; i < parameters?.Length; i++)
            {
                int index = i;
                Action<string> currentAction = null!;
                
                currentAction += input =>
                {
                    if (input.Length > 0)
                        _currentParameters.Add(input);
                };

                if (i == parameters.Length - 1)
                    currentAction += input =>
                    {
                        MakeMenuClosed();
                        InformationManager.DisplayMessage(new InformationMessage((string)cheatTuple.Method.Invoke(null, new object[] { _currentParameters })));
                        _currentParameters.Clear();
                    };
                else
                {
                    currentAction += input =>
                    {
                        InformationManager.ShowTextInquiry(new TextInquiryData(parameters[index + 1], null, true, 
                            false, "Ok", null, affirmativeActions[index + 1], null));
                    };
                }
                
                affirmativeActions.Add(currentAction);
            }
            
            InformationManager.ShowTextInquiry(new TextInquiryData(cheatTuple.Properties.Parameters?[0].ToString(), null, true, false, 
                "Ok", null, affirmativeActions[0], null));
        }

        private bool IsHotKeyPressed => Input.IsKeyReleased(_menuKey);
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            if (!MBCommon.IsPaused && IsHotKeyPressed && Mission.Current?.IsInPhotoMode != true && !CampaignCheats.IsEmpty() && !_isMenuOpened)
            {
                var elementCheats = Mission.Current != null ? MissionCheats : Campaign.Current != null ? CampaignCheats : null;

                if (elementCheats == null)
                    return;
                
                List<InquiryElement> inquiryElements = elementCheats.Select(element => new InquiryElement(element, element.Properties?.Description, null)).ToList();

                MultiSelectionInquiryData inquiryData = new("Basic Overhaul", new TextObject("{=select_option}Select a option to apply.").ToString(), 
                    inquiryElements, false, 0,1, "Done", "Cancel",
                    ApplyCheat, elements => _isMenuOpened = false);
                
                MBInformationManager.ShowMultiSelectionInquiry(inquiryData, true);
                _isMenuOpened = true;
            }
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
                    campaignGameStarter.AddModel(new BOClanFinanceModel());
                    campaignGameStarter.AddModel(new BOBuildingConstructionModel());
                    
                }

                if (BasicOverhaulGlobalConfig.Instance?.EnableGovernorNotables == true)
                {
                    campaignGameStarter.AddBehavior(new NotableBehavior());
                    campaignGameStarter.AddModel(new BONotableSpawnModel());
                }
                
                campaignGameStarter.AddModel(new BOSettlementProsperityModel());
                campaignGameStarter.AddModel(new BOPartyModel());
                campaignGameStarter.AddModel(new BOBattleRewardModel());
                campaignGameStarter.AddModel(new BOVolunteerModel());
                campaignGameStarter.AddModel(new BOAgentStatCalculateModel());
                campaignGameStarter.AddModel(new BOWorkshopModel());
                campaignGameStarter.AddModel(new BOExecutionRelationModel());
            }
            else
            {
                try
                {
                    gameStarterObject.AddModel(new BOCustomAgentStatCalculateModel());
                }
                catch (Exception){}
            }

            InitializeCheats();
            PossibleKeys.TryGetValue(BasicOverhaulGlobalConfig.Instance?.MenuHotKey?.SelectedValue ?? "U", out _menuKey);
        }

        private void InitializeCheats()
        {
            if(!CampaignCheats.Any())
            {
                CampaignCheats.AddRange(
                    from method in AccessTools.GetDeclaredMethods(typeof(Options)).Concat(AccessTools.GetDeclaredMethods(typeof(NativeCheats)))
                    let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicOption)) as BasicOption
                    where basicCheat != null
                    orderby basicCheat.Description.StartsWith("[")
                    select (basicCheat, method)
                );
            }
            
            if(!MissionCheats.Any())
                MissionCheats.AddRange(
                    from method in AccessTools.GetDeclaredMethods(typeof(MissionOptions))
                    let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicOption)) as BasicOption
                    where basicCheat != null
                    orderby basicCheat.Description.StartsWith("[")
                    select (basicCheat, method)
                );
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

