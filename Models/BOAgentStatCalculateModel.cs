using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Models;

internal class BOAgentStatCalculateModel : SandboxAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        (float combatmax, float max) cheatSpeed = (agentDrivenProperties.CombatMaxSpeedMultiplier, agentDrivenProperties.MaxSpeedMultiplier);
        base.UpdateAgentStats(agent, agentDrivenProperties);
        if (MissionCheats.SpeedOnCheat && Agent.Main != null)
        {
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, cheatSpeed.max);
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, cheatSpeed.combatmax);
            Agent.Main.UpdateCustomDrivenProperties();
        }
    }
}

internal class BOCustomAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        (float combatmax, float max) cheatSpeed = (agentDrivenProperties.CombatMaxSpeedMultiplier, agentDrivenProperties.MaxSpeedMultiplier);
        base.UpdateAgentStats(agent, agentDrivenProperties);
        if (MissionCheats.SpeedOnCheat && agent == Agent.Main)
        {
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.MaxSpeedMultiplier, cheatSpeed.max);
            Agent.Main.SetAgentDrivenPropertyValueFromConsole(DrivenProperty.CombatMaxSpeedMultiplier, cheatSpeed.combatmax);
            Agent.Main.UpdateCustomDrivenProperties();
        }
    }
}