using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Mission), "GetAttackCollisionResults")]
public static class GetAttackCollisionResultsPatch
{
    public static void Postfix(
        Agent attackerAgent,
        Agent victimAgent,
        GameEntity hitObject,
        ref float momentumRemaining,
        in MissionWeapon attackerWeapon,
        ref bool crushedThrough,
        ref bool cancelDamage,
        ref bool crushedThroughWithoutAgentCollision,
        ref AttackCollisionData attackCollisionData,
        ref WeaponComponentData shieldOnBack,
        ref CombatLogData combatLog)
    {
        if (MissionOptions.IsPlayerDamageOp && attackerAgent.IsMainAgent)
        {
            if (victimAgent?.IsMainAgent == false)
            {
                crushedThrough = true;
                if (attackCollisionData.AttackBlockedWithShield)
                {
                    attackCollisionData.IsShieldBroken = true;
                    attackCollisionData.InflictedDamage = victimAgent.WieldedOffhandWeapon.HitPoints + 10;
                }
                else
                    attackCollisionData.InflictedDamage = (int)victimAgent.HealthLimit + 10;
            
                combatLog.InflictedDamage = (int)victimAgent.HealthLimit + 10;
            }
            else if (victimAgent == null && hitObject != null)
            {
                attackCollisionData.InflictedDamage = combatLog.InflictedDamage = 1000;
            }
        }
        else if (MissionOptions.PlayerInvincible && victimAgent?.IsMainAgent == true)
        {
            momentumRemaining = 0;
            cancelDamage = false;
            attackCollisionData.InflictedDamage = 0;
            combatLog.InflictedDamage = 0;
        }
    }
}

[HarmonyPatch(typeof(MissionCombatMechanicsHelper), "GetDefendCollisionResults")]
public static class GetDefendCollisionResultsPatch
{
    public static void Postfix(
        Agent attackerAgent,
        Agent defenderAgent,
        ref CombatCollisionResult collisionResult,
        int attackerWeaponSlotIndex,
        bool isAlternativeAttack,
        StrikeType strikeType,
        Agent.UsageDirection attackDirection,
        float collisionDistanceOnWeapon,
        float attackProgress,
        bool attackIsParried,
        bool isPassiveUsageHit,
        ref bool isHeavyAttack,
        ref float defenderStunPeriod,
        ref float attackerStunPeriod,
        ref bool crushedThrough,
        ref bool chamber)
    {
        if (MissionOptions.IsPlayerDamageOp && attackerAgent.IsMainAgent)
        {
            isHeavyAttack = true;
            crushedThrough = true;
        }

        if (BasicOverhaulGlobalConfig.Instance?.DisableAllyCollision == true &&
            defenderAgent?.IsFriendOf(attackerAgent) == true)
        {
            collisionResult = CombatCollisionResult.None;
            chamber = true;
            crushedThrough = true;
        }
    }
}