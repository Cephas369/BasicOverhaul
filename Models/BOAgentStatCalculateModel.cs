using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Models;

internal class BOAgentStatCalculateModel : SandboxAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        base.UpdateAgentStats(agent, agentDrivenProperties);
        if (MissionCheats.SpeedOnCheat.max > 1 && Agent.Main != null)
        {
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, MissionCheats.SpeedOnCheat.max);
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, MissionCheats.SpeedOnCheat.combatmax);
            Agent.Main.UpdateCustomDrivenProperties();
        }
    }
}

internal class BOCustomAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        base.UpdateAgentStats(agent, agentDrivenProperties);
        if (MissionCheats.SpeedOnCheat.max > 1 && agent == Agent.Main)
        {
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, MissionCheats.SpeedOnCheat.max);
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, MissionCheats.SpeedOnCheat.combatmax);
            Agent.Main.UpdateCustomDrivenProperties();
        }
    }
}