using BasicOverhaul.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
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
    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        base.OnAgentHit(affectedAgent, affectorAgent, in affectorWeapon, in blow, in attackCollisionData);
        if (affectedAgent.IsMainAgent && MissionCheats.PlayerInvincible)
            affectedAgent.Health = affectedAgent.HealthLimit;
        else if (affectedAgent.IsMount && affectedAgent.RiderAgent?.IsMainAgent == true && MissionCheats.MountInvincible)
            affectedAgent.Health = affectedAgent.HealthLimit;
    }
}