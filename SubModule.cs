using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bannerlord.ButterLib;
using BasicOverhaul.Behaviors;
using BasicOverhaul.Models;
using BasicOverhaul.Patches;
using Helpers;
using SandBox;
using SandBox.Conversation.MissionLogics;
using SandBox.GauntletUI;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.ScreenSystem;

namespace BasicOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly List<(BasicCheat? Properties, MethodInfo Method)> CampaignCheats = new();
        public static readonly List<(BasicCheat? Properties, MethodInfo Method)> MissionCheats = new();
        private static List<string> _currentParameters = new();
        private bool isMenuOpened = false;
        public static Harmony Harmony;
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony = new Harmony("com.basic_overhaul");
            Harmony.PatchAll();
        }

        public override void OnAfterGameInitializationFinished(Game game, object starterObject)
        {
            base.OnAfterGameInitializationFinished(game, starterObject);
            if (BasicOverhaulGlobalConfig.Instance?.EnablePartyScreenFilters == true)
            {
                Harmony.Patch(AccessTools.Method(typeof(ScreenBase), "AddLayer"), postfix: AccessTools.Method(typeof(PartyGUIPatch), "Postfix"));
                Harmony.Patch(AccessTools.Method(typeof(PartyVM), "OnFinalize"), postfix: AccessTools.Method(typeof(PartyGUIPatch), "OnPartyVMFinalize"));
            }
        }

        private void MakeMenuFalse() => isMenuOpened = false;
        private void ApplyCheat(List<InquiryElement> inquiryElements)
        {
            if (!inquiryElements.Any())
                return;
            InquiryElement inquiry = inquiryElements[0];
            var cheatTuple = ((BasicCheat? Properties, MethodInfo Method))inquiry.Identifier;
            
            string[]? parameters = cheatTuple.Properties?.Parameters?.Select(text=>text.ToString()).ToArray();

            if (parameters == null)
            {
                MakeMenuFalse();
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
                        MakeMenuFalse();
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

        private bool isHotKeyPressed => Input.IsKeyReleased(InputKey.U);
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            if (!MBCommon.IsPaused && isHotKeyPressed && Mission.Current?.IsInPhotoMode != true && !CampaignCheats.IsEmpty() && !isMenuOpened)
            {
                var elementCheats = Mission.Current != null ? MissionCheats : Campaign.Current != null ? CampaignCheats : null;

                if (elementCheats == null)
                    return;
                
                List<InquiryElement> inquiryElements = elementCheats.Select(element =>
                    {
                        if (element.Properties?.Description.Value.Contains("{VALUE}") == true &&
                            Helpers.CheatDescriptionAttributes.TryGetValue(element.Properties.Description.GetID(), out Delegate del))
                        {
                            element.Properties.Description.SetTextVariable("VALUE", (string)del.DynamicInvoke());
                        }
                        return new InquiryElement(element, element.Properties?.Description.ToString(), null);
                        
                    }).ToList();

                MultiSelectionInquiryData inquiryData = new("Basic Overhaul", "Select a option to apply.", 
                    inquiryElements, true, 0,1, "Done", "Cancel",
                    ApplyCheat, elements => isMenuOpened = false);
                
                MBInformationManager.ShowMultiSelectionInquiry(inquiryData, true);
                isMenuOpened = true;
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
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if(gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new MiscBehavior());
                
                if(BasicOverhaulGlobalConfig.Instance?.EnableDeserterParties == true)
                    campaignGameStarter.AddBehavior(new DesertionBehavior());
                
                if (BasicOverhaulGlobalConfig.Instance?.EnableSlaveSystem == true)
                {
                    campaignGameStarter.AddBehavior(new SlaveBehavior());
                    campaignGameStarter.AddModel(new SlaveClanFinanceModel());
                    campaignGameStarter.AddModel(new SlaveBuildingConstructionModel());
                    campaignGameStarter.AddModel(new SlaveSettlementProsperityModel());
                }

                if (BasicOverhaulGlobalConfig.Instance?.EnableGovernorNotables == true)
                {
                    campaignGameStarter.AddBehavior(new NotableBehavior());
                    campaignGameStarter.AddModel(new BONotableSpawnModel());
                }
                
                campaignGameStarter.AddModel(new BOPartyModel());
                campaignGameStarter.AddModel(new BOBattleRewardModel());
                campaignGameStarter.AddModel(new BOVolunteerModel());
                campaignGameStarter.AddModel(new BOAgentStatCalculateModel());
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

        }

        private void InitializeCheats()
        {
            if(!CampaignCheats.Any())
            {
                CampaignCheats.AddRange(
                    from method in AccessTools.GetDeclaredMethods(typeof(Cheats))
                    let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicCheat)) as BasicCheat
                    where basicCheat != null
                    orderby basicCheat.Description.ToString()
                    select (basicCheat, method)
                );

                CampaignCheats.AddRange(
                    from method in AccessTools.GetDeclaredMethods(typeof(NativeCheats))
                    where Attribute.GetCustomAttribute(method, typeof(BasicCheat)) is BasicCheat
                    select (Attribute.GetCustomAttribute(method, typeof(BasicCheat)) as BasicCheat, method)
                );
            }
            if(!MissionCheats.Any())
                MissionCheats.AddRange(
                    from method in AccessTools.GetDeclaredMethods(typeof(MissionCheats))
                    let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicCheat)) as BasicCheat
                    where basicCheat != null
                    orderby basicCheat.Description.ToString()
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
        }
        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<MobileParty, Settlement>));
            ConstructContainerDefinition(typeof(Dictionary<string, TownSlaveData>));
            ConstructContainerDefinition(typeof(Dictionary<Settlement, CampaignTime>));
        }
    }
}

