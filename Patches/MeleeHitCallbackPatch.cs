using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Mission), "MeleeHitCallback")]
public static class MeleeHitCallbackPatch
{
    public static bool Prefix(
        ref AttackCollisionData collisionData,
        Agent attacker,
        Agent victim,
        GameEntity realHitEntity,
        ref float inOutMomentumRemaining,
        ref MeleeCollisionReaction colReaction,
        CrushThroughState crushThroughState,
        Vec3 blowDir,
        Vec3 swingDir,
        ref HitParticleResultData hitParticleResultData,
        bool crushedThroughWithoutAgentCollision)
    {
        if (attacker != null && victim != null && BasicOverhaulConfig.Instance?.DisableAllyCollision == true)
        {
            if (!attacker.IsEnemyOf(victim) && victim.IsHuman)
            {
                colReaction = MeleeCollisionReaction.ContinueChecking;
                return false;
            }
        }

        return true;
    }
}