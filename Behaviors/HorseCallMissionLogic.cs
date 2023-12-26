using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Behaviors;

internal class HorseCallMissionLogic : MissionLogic
{
    private readonly Timer keyPressTimer = new Timer(Time.ApplicationTime, 1f, false);
    private Agent mainHorse;
    
    public override void OnAgentMount(Agent agent)
    {
        base.OnAgentMount(agent);
        if (agent.IsMainAgent)
            mainHorse = agent.MountAgent;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        if (agent.IsMainAgent)
            mainHorse = agent.MountAgent;
    }

    public override void OnAgentDeleted(Agent affectedAgent)
    {
        base.OnAgentDeleted(affectedAgent);
        if (affectedAgent == mainHorse)
            mainHorse = null;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (mainHorse == null || BasicOverhaulConfig.Instance.HorseCallSkill < 0)
            return;
        
        if (Input.IsKeyReleased(InputKey.X))    
        {
            if (Agent.Main?.Character.GetSkillValue(DefaultSkills.Riding) < BasicOverhaulConfig.Instance.HorseCallSkill)
            {
                InformationManager.DisplayMessage(new InformationMessage("You don't have the required skill to call your horse."));
                return;
            }
            if (keyPressTimer.Check(Time.ApplicationTime))
            {
                CallHorse();
                keyPressTimer.Reset(Time.ApplicationTime, 5f);
            }
        }
    }

    private void CallHorse()
    {
        if (Agent.Main.MountAgent?.Health < 1)
            InformationManager.DisplayMessage(new InformationMessage("Your horse has died."));
        else
        {
            if (Agent.Main.HasMount)
                InformationManager.DisplayMessage(new InformationMessage("You're already riding!"));
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("Your mount has heard your call...", Colors.Blue));
                WorldPosition worldPosition = Agent.Main.GetWorldPosition();
                mainHorse.SetScriptedPosition(ref worldPosition, false);
            }
        }
    }
}