using System;
using BasicOverhaul.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
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