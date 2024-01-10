using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Mission), "GetAttackCollisionResults")]
public static class GetAttackCollisionResultsPatch
{
    public static void Postfix(
        Agent attackerAgent,
        Agent victimAgent,
        GameEntity hitObject,
        float momentumRemaining,
        in MissionWeapon attackerWeapon,
        ref bool crushedThrough,
        bool cancelDamage,
        bool crushedThroughWithoutAgentCollision,
        ref AttackCollisionData attackCollisionData,
        ref WeaponComponentData shieldOnBack,
        ref CombatLogData combatLog)
    {
        if (attackerAgent.IsMainAgent && MissionCheats.IsPlayerDamageOp)
        {
            crushedThrough = true;
            attackCollisionData.InflictedDamage = 1000;
            combatLog.InflictedDamage = 1000;
        }
    }
}

[HarmonyPatch(typeof(Mission), "GetDefendCollisionResults")]
public static class GetDefendCollisionResultsPatch
{
    public static void Postfix(
        Agent attackerAgent,
        Agent defenderAgent,
        CombatCollisionResult collisionResult,
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
        ref bool crushedThrough)
    {
        if (attackerAgent.IsMainAgent && MissionCheats.IsPlayerDamageOp)
        {
            isHeavyAttack = true;
            crushedThrough = true;
        }
    }
}