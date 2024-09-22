using BasicOverhaul.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
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
    public bool DeserterConflictAppeared;
    public static MiscBehavior? Instance { get; private set; }

    public MiscBehavior()
    {
        Instance = this;
    }
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
        dataStore.SyncData("_deserterConflictAppeared", ref DeserterConflictAppeared);
    }
}

internal class MiscMissionLogic : MissionLogic
{
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
        BasicStatCalculateModel.ModifiedAgents.Clear();
    }
}