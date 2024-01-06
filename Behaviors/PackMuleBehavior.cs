
using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ObjectSystem;

namespace BasicOverhaul.Behaviors;

internal class PackMuleBehavior : MissionBehavior
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private Agent packAgent;
    private AgentInteractionInterfaceVM InterfaceVm;
    private bool focused;
    private InputKey actionKey = (InputKey)Enum.Parse(typeof(InputKey), HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13));
    
    public override void OnTeamDeployed(Team team)
    {
        base.OnTeamDeployed(team);
        if (team.IsPlayerTeam && Hero.MainHero?.PartyBelongedTo != null)
        {
            string packAgentId;
            TerrainType terrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(Hero.MainHero.PartyBelongedTo.CurrentNavigationFace);

            string[] _muleLoadHarnesses;
            if (terrainType == TerrainType.Desert)
            {
                packAgentId = "camel";
                _muleLoadHarnesses = new []
                {
                  "camel_saddle_a",
                  "camel_saddle_b"
                };
            }
            else
            {
                packAgentId = "mule";
                _muleLoadHarnesses =new []
                {
                    "mule_load_a",
                    "mule_load_b",
                    "mule_load_c"
                };
            }
            
            ItemObject animal = MBObjectManager.Instance.GetObject<ItemObject>(packAgentId);
            if (animal != null)
            {
                string harnessId = _muleLoadHarnesses[MBRandom.RandomInt(_muleLoadHarnesses.Length)];
                ItemRosterElement harnessElement = new ItemRosterElement(Game.Current.ObjectManager.GetObject<ItemObject>(harnessId));
                ItemRosterElement muleElement = new ItemRosterElement(animal);
                Vec3 mainPosition = Agent.Main.Position;
                mainPosition.x -= 2;
                mainPosition.y -= 2;

                Vec3 random = Mission.GetRandomPositionAroundPoint(mainPosition, 1f, 2f);
                
                packAgent = Mission.SpawnMonster(muleElement, harnessElement, random, mainPosition.AsVec2, -1);
                packAgent.SetAgentFlags(AgentFlag.None);
                packAgent.SetIsAIPaused(true);
                packAgent.Controller = Agent.ControllerType.None;
                packAgent.AddComponent(new CommonAIComponent(packAgent));
                
                MissionGauntletAgentStatus status = Mission.GetMissionBehavior<MissionGauntletAgentStatus>();
                var _dataSource = AccessTools.Field(typeof(MissionGauntletAgentStatus), "_dataSource");
                var _interactionInterface = AccessTools.Field(typeof(MissionAgentStatusVM), "_interactionInterface");

                InterfaceVm = (AgentInteractionInterfaceVM)_interactionInterface.GetValue(_dataSource.GetValue(status));
            }
        }
    }

    public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
    {
        base.OnFocusGained(agent, focusableObject, isInteractable);
        if (focusableObject == packAgent)
        {
            TextObject textObject = new TextObject("Press {KEY} to open your inventory");
            MBTextManager.SetTextVariable("KEY", GameTexts.FindText("str_ui_agent_interaction_use"));
            MBTextManager.SetTextVariable("USE_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)), false);
            InterfaceVm.SecondaryInteractionMessage = textObject.ToString();
            InterfaceVm.IsActive = true;
            focused = true;
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (Input.IsKeyReleased(actionKey) && focused)
            InventoryManager.OpenScreenAsInventory(()=>
            {   
                if (Agent.Main?.Character is CharacterObject characterObject)
                    Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(characterObject.FirstBattleEquipment);
            });
    }

    public override void OnFocusLost(Agent agent, IFocusable focusableObject)
    {
        base.OnFocusLost(agent, focusableObject);
        if (focusableObject == packAgent)
            focused = false;
    }
}