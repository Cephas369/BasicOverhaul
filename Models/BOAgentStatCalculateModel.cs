using System.Collections.Generic;
using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Models;

public static class BasicStatCalculateModel
{
    public static Dictionary<Agent, (DrivenProperty property, float value)[]> ModifiedAgents = new();
    public static void Add(Agent agent, (DrivenProperty property, float value)[] properties)
    {
        if (!ModifiedAgents.ContainsKey(agent))
        {
            ModifiedAgents.Add(agent, properties);
        }
        else
        {
            ModifiedAgents[agent] = properties;
        }
    }
    public static void ModifyAgentProperties(Agent agent)
    {
        if (!agent.IsHuman || !ModifiedAgents.TryGetValue(agent, out var properties)) 
            return;
        
        foreach (var tuple in properties)
        {
            agent.SetAgentDrivenPropertyValueFromConsole(tuple.property, tuple.value);
            agent.UpdateCustomDrivenProperties();
        }
    }
}
internal class BOAgentStatCalculateModel : SandboxAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        base.UpdateAgentStats(agent, agentDrivenProperties);
        BasicStatCalculateModel.ModifyAgentProperties(agent);
    }
}

internal class BOCustomAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
{
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        base.UpdateAgentStats(agent, agentDrivenProperties);
        BasicStatCalculateModel.ModifyAgentProperties(agent);
    }
}