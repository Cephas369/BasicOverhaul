using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(Mission), "SpawnWeaponAsDropFromAgentAux")]
public static class SpawnWeaponAsDropFromAgentAuxPatch
{
    public static void Postfix(
        Agent agent,
        EquipmentIndex equipmentIndex,
        ref Vec3 velocity,
        ref Vec3 angularVelocity,
        Mission.WeaponSpawnFlags spawnFlags,
        int forcedSpawnIndex)
    {
        if (BasicOverhaulGlobalConfig.Instance?.EnableDeathDropEveryWeapon == true && agent.Health > 0)
            return;
        
        for (EquipmentIndex index = EquipmentIndex.Weapon0; index <= EquipmentIndex.Weapon3; index++)
        {
            if (index != equipmentIndex && agent.Equipment[index].Item != null)
                Mission.Current.SpawnWeaponAsDropFromAgentAux(agent, index, ref velocity, ref angularVelocity, 
                    agent.Equipment[index].IsAnyAmmo() && agent.Equipment[index].Item.HolsterMeshName != null ? spawnFlags | Mission.WeaponSpawnFlags.WithHolster : spawnFlags, forcedSpawnIndex);
        }
    }
}