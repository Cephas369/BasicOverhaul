using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.FaceGenerator;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace BasicOverhaul.Behaviors;

internal class HorseCallMissionLogic : MissionLogic
{
    private readonly Timer keyPressTimer = new(Time.ApplicationTime, 1f, false);
    private Agent? _mainHorse;
    
    public static HorseCallMissionLogic? Instance { get; private set; }
    public HorseCallMissionLogic()
    {
        Instance = this;
    }
    public override void OnAgentMount(Agent agent)
    {
        base.OnAgentMount(agent);
        if (agent.IsMainAgent)
            _mainHorse = agent.MountAgent;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        if (agent.IsMainAgent)
            _mainHorse = agent.MountAgent;
    }

    public override void OnAgentDeleted(Agent affectedAgent)
    {
        base.OnAgentDeleted(affectedAgent);
        if (affectedAgent == _mainHorse)
            _mainHorse = null;
    }

    public void OnCallHorse()
    {
        if (_mainHorse == null || BasicOverhaulGlobalConfig.Instance.HorseCallSkill < 0)
            return;
        
        if (Agent.Main?.Controller == Agent.ControllerType.Player && Agent.Main.Character.GetSkillValue(DefaultSkills.Riding) < BasicOverhaulGlobalConfig.Instance.HorseCallSkill)
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

    private void CallHorse()
    {
        if (Agent.Main?.MountAgent?.Health < 1)
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=horse_call_warning.2}Your horse has died.").ToString()));
        else
        {
            if (Agent.Main?.HasMount == true)
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=horse_call_warning.1}You're already riding!").ToString()));
            else
            {
                Mission.MakeSound(SoundEvent.GetEventIdFromString("event:/voice/combat/whistle"), Agent.Main.Position, false, false, Agent.Main.Index, _mainHorse.Index);
                //InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=mount_heard_call}Your mount has heard your call...").ToString(), Colors.Blue));
                WorldPosition worldPosition = Agent.Main.GetWorldPosition();
                _mainHorse.SetScriptedPosition(ref worldPosition, false);
            }
        }
    }
}