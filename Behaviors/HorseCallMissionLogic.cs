using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        if (mainHorse == null || BasicOverhaulGlobalConfig.Instance.HorseCallSkill < 0)
            return;
        
        if (Input.IsKeyReleased(InputKey.X))    
        {
            if (Agent.Main?.Character.GetSkillValue(DefaultSkills.Riding) < BasicOverhaulGlobalConfig.Instance.HorseCallSkill)
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=horse_call_warning.3}You don't have the required skill to call your horse.").ToString()));
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
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=horse_call_warning.2}Your horse has died.").ToString()));
        else
        {
            if (Agent.Main.HasMount)
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=horse_call_warning.1}You're already riding!").ToString()));
            else
            {
                Mission.MakeSound(SoundEvent.GetEventIdFromString("event:/voice/combat/whistle"), Agent.Main.Position, false, false, Agent.Main.Index, mainHorse.Index);
                //InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=mount_heard_call}Your mount has heard your call...").ToString(), Colors.Blue));
                WorldPosition worldPosition = Agent.Main.GetWorldPosition();
                mainHorse.SetScriptedPosition(ref worldPosition, false);
            }
        }
    }
}