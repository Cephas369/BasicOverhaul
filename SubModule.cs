using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BasicOverhaul.Behaviors;
using BasicOverhaul.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace BasicOverhaul
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly List<(BasicCheat? Properties, MethodInfo Method)> CampaignCheats = new();
        public static readonly List<(BasicCheat? Properties, MethodInfo Method)> MissionCheats = new();
        private static List<string> _currentParameters = new();
        private bool isMenuOpened = false;
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("com.basic_overhaul").PatchAll();
            
            
            foreach (var method in AccessTools.GetDeclaredMethods(typeof(Cheats)))
                if(Attribute.GetCustomAttribute(method, typeof(BasicCheat)) is BasicCheat cheatAttribute)
                    CampaignCheats.Add((cheatAttribute, method));
            
            foreach (var method in AccessTools.GetDeclaredMethods(typeof(MissionCheats)))
                if(Attribute.GetCustomAttribute(method, typeof(BasicCheat)) is BasicCheat cheatAttribute)
                    MissionCheats.Add((cheatAttribute, method));
        }

        private void MakeMenuFalse() => isMenuOpened = false;
        private void ApplyCheat(List<InquiryElement> inquiryElements)
        {
            InquiryElement inquiry = inquiryElements[0];
            var cheatTuple = Mission.Current != null ? MissionCheats[(int)inquiry.Identifier] : CampaignCheats[(int)inquiry.Identifier];
            string[]? parameters = cheatTuple.Properties!.Parameters;

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
            
            InformationManager.ShowTextInquiry(new TextInquiryData(cheatTuple.Properties.Parameters?[0], null, true, false, 
                "Ok", null, affirmativeActions[0], null));
        }
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            if (!MBCommon.IsPaused && Input.IsKeyReleased(InputKey.Q) && Mission.Current?.IsInPhotoMode != true && !CampaignCheats.IsEmpty() && !isMenuOpened)
            {
                List<InquiryElement> inquiryElements = new();
                
                if(Mission.Current != null)
                    for (int i = 0; i < MissionCheats.Count; i++)
                        inquiryElements.Add(new InquiryElement(i, MissionCheats[i].Properties?.Description, null));
                else if (Campaign.Current != null)
                    for (int i = 0; i < CampaignCheats.Count; i++)
                        inquiryElements.Add(new InquiryElement(i, CampaignCheats[i].Properties?.Description, null));
                else
                    return;

                MultiSelectionInquiryData inquiryData = new("Cheats", "Select a cheat to apply.", 
                    inquiryElements, true, 0,1, "Done", "Cancel",
                    ApplyCheat, elements => isMenuOpened = false);
                
                MBInformationManager.ShowMultiSelectionInquiry(inquiryData, true);
                isMenuOpened = true;
            }
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            BasicOverhaul.MissionCheats.SpeedOnCheat = false;
            mission.AddMissionBehavior(new WeaponryOrderMissionBehavior());
            mission.AddMissionBehavior(new HorseCallMissionLogic());
        }
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if(gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                if(BasicOverhaulConfig.Instance?.EnableDeserterParties == true)
                    campaignGameStarter.AddBehavior(new DesertionBehavior());
                
                if (BasicOverhaulConfig.Instance?.EnableSlaveSystem == true)
                {
                    campaignGameStarter.AddBehavior(new SlaveBehavior());
                    campaignGameStarter.AddModel(new SlaveClanFinanceModel());
                    campaignGameStarter.AddModel(new SlaveBuildingConstructionModel());
                    campaignGameStarter.AddModel(new SlaveSettlementProsperityModel());
                }
                
                campaignGameStarter.AddBehavior(new NotableBehavior());
                campaignGameStarter.AddModel(new BONotableSpawnModel());
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
        }
    }
}