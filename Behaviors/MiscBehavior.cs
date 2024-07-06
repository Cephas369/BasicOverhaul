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

[HarmonyPatch(typeof(NativeConfig), "CheatMode", MethodType.Setter)]
public static class NativeConfigPatch
{
    public static void Prefix(ref bool value)
    {
        if (BasicOverhaulCampaignConfig.Instance != null)
            value = BasicOverhaulCampaignConfig.Instance.CheatModeEnabled;
    }
}
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