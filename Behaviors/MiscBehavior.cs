using System;
using System.Collections.Generic;
using System.Reflection;
using BasicOverhaul.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Behaviors;

internal class MiscBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
    }

    private void OnGameLoadFinished()
    {
        if (BasicOverhaulCampaignConfig.Instance != null)
            AccessTools.Property(typeof(NativeConfig), "CheatMode").SetValue(null, BasicOverhaulCampaignConfig.Instance.CheatModeEnabled);
    }
    
    public override void SyncData(IDataStore dataStore)
    {
        
    }
}

internal class MiscMissionLogic : MissionLogic
{
    private InputKey _fastForwardKey = InputKey.Numpad9;
    private MethodInfo SetInitialAgentScale = AccessTools.Method(typeof(Agent), "SetInitialAgentScale");
    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        base.OnAgentHit(affectedAgent, affectorAgent, in affectorWeapon, in blow, in attackCollisionData);
        if (MissionOptions.PlayerInvincible && affectedAgent.IsMainAgent)
            affectedAgent.Health = affectedAgent.HealthLimit;
        else if (MissionOptions.MountInvincible && affectedAgent.IsMount && affectedAgent.RiderAgent?.IsMainAgent == true)
            affectedAgent.Health = affectedAgent.HealthLimit;
    }

    public override void OnCreated()
    {
        base.OnCreated();
        _fastForwardKey = (InputKey)Enum.Parse(typeof(InputKey),
            BasicOverhaulGlobalConfig.Instance?.FastForwardMissionKey?.SelectedValue ?? "Numpad9");
        
        BasicStatCalculateModel.ModifiedAgents.Clear();
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        if (BasicOverhaulGlobalConfig.Instance?.EnableRandomHumanSizes == true && agent.IsHuman)
        {       
            FaceGenerationParams faceGenerationParams = FaceGenerationParams.Create();
            MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, agent.BodyPropertiesValue, true, true);
            faceGenerationParams.HeightMultiplier = MBRandom.RandomFloatRanged(0.2f, 1f);
            BodyProperties bodyProperties = agent.BodyPropertiesValue;
            MBBodyProperties.ProduceNumericKeyWithParams(faceGenerationParams, true, true, ref bodyProperties);
            agent.UpdateBodyProperties(bodyProperties);
            agent.UpdateSpawnEquipmentAndRefreshVisuals(agent.SpawnEquipment);
        }
        
        if (BasicOverhaulGlobalConfig.Instance?.EnableRandomMountSizes == true && agent.IsMount && agent.RiderAgent?.IsHero == false)
        {
            EquipmentElement equipmentElement = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
            if (equipmentElement.Item.HorseComponent.BodyLength != 0)
            {
                float initialScale = (0.01f * (float)equipmentElement.Item.HorseComponent.BodyLength);
                SetInitialAgentScale.Invoke(agent, new object[] {
                    MBRandom.RandomFloatRanged(initialScale * 0.835f, initialScale * 1.08f)});
            }
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (Input.IsKeyReleased(_fastForwardKey))
        {
            if (Mission.Current == null)
                return;
            
            Mission.Current.SetFastForwardingFromUI(!Mission.Current.IsFastForward);
        }
    }
}