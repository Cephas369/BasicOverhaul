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
        if (!agent.IsHuman) 
            return;

        if (agent.Character.DefaultFormationClass == FormationClass.Cavalry)
        {
            agent.AgentDrivenProperties.WeaponInaccuracy = 0;
            agent.AgentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0;
            agent.AgentDrivenProperties.WeaponBestAccuracyWaitTime = 0.5f;
        }
        
        if (ModifiedAgents.TryGetValue(agent, out var properties))
        {
            foreach (var tuple in properties)
            {
                agent.SetAgentDrivenPropertyValueFromConsole(tuple.property, tuple.value);
            }
        }
        agent.UpdateCustomDrivenProperties();
    }
}
internal class BOAgentStatCalculateModel : SandboxAgentStatCalculateModel
{
    private AgentStatCalculateModel _previousModel;

    public BOAgentStatCalculateModel(AgentStatCalculateModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        _previousModel.UpdateAgentStats(agent, agentDrivenProperties);
        BasicStatCalculateModel.ModifyAgentProperties(agent);
    }
}

internal class BOCustomAgentStatCalculateModel : CustomBattleAgentStatCalculateModel
{
    private AgentStatCalculateModel _previousModel;

    public BOCustomAgentStatCalculateModel(AgentStatCalculateModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        _previousModel.UpdateAgentStats(agent, agentDrivenProperties);
        BasicStatCalculateModel.ModifyAgentProperties(agent);
    }
}