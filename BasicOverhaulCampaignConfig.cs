﻿using System.Reflection;
using System.Text.RegularExpressions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.PerCampaign;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;

namespace BasicOverhaul;

internal sealed class BasicOverhaulCampaignConfig : AttributePerCampaignSettings<BasicOverhaulCampaignConfig> 
{
    public override string Id => "basic_overhaul_campaign";
    public override string DisplayName => new TextObject("{=basic_overhaul_campaign}Basic Overhaul (Current Campaign)").ToString();
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyInteger("{=bo_campaign_config_title.1}Party Size Limit Multiplier", minValue: 0, maxValue: 20, "#0x", Order = 1,
        RequireRestart = false, HintText = "{=bo_campaign_config_desc.1}OBS: Putting a high value will lag your game. Leave 0 for the default game chance.")]
    public int PartySizeLimitMultiplier { get; set; } = 0;
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyFloatingInteger("{=bo_campaign_config_title.2}Global Loot Chance", minValue: 0f, maxValue: 1f, "#0%", Order = 2, RequireRestart = false, HintText = "{=leave_0_for_default_chance}Leave 0 for the default game chance.")]
    public float GlobalLootChance { get; set; } = 0;
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyInteger("{=bo_campaign_config_title.3}Battle Renown Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 3, RequireRestart = false, HintText = "{=leave_0_for_default_chance}Leave 0 for the default game chance.")]
    public int BattleRenownGainMultiplier { get; set; } = 0;
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyInteger("{=bo_campaign_config_title.4}Battle Influence Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 4, RequireRestart = false, HintText = "{=leave_0_for_default_chance}Leave 0 for the default game chance.")]
    public int BattleInfluenceGainMultiplier { get; set; } = 0;
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyFloatingInteger("{=bo_campaign_config_title.5}Battle Morale Gain Multiplier", minValue: 0f, maxValue: 10f, "#0x", Order = 5, RequireRestart = false, HintText = "{=leave_0_for_default_chance}Leave 0 for the default game chance.")]
    public float BattleMoraleGainMultiplier { get; set; } = 0;
    
    [SettingPropertyGroup("{=campaign_modifiers}Campaign Modifiers", GroupOrder = 1)]
    [SettingPropertyInteger("{=bo_campaign_config_title.6}Recruitment multiplier for parties", minValue: 0, maxValue: 20, "#0x", Order = 6, RequireRestart = false, HintText = "{=bo_campaign_config_desc.6}Increases the amount that parties can recruit from settlements and the volunteers production. Leave 0 for the default game chance.")]
    public int RecruitmentRate { get; set; } = 0;
    
    [SettingPropertyGroup("{=miscellaneous}Miscellaneous", GroupOrder = 2)]
    [SettingPropertyBool("{=bo_campaign_config_title.7}Enable cheat mode", HintText = "{=bo_campaign_config_desc.7}Enable 'switch cheat mode by this mod config' in the B.O global settings to use this. If you change this during the campaign, restart the save to apply.", Order = 2, RequireRestart = false)]
    public bool CheatModeEnabled { get; set; } = false;
}