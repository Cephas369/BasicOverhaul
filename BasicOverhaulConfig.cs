using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.ModuleManager;

namespace BasicOverhaul
{
    internal sealed class BasicOverhaulConfig : AttributeGlobalSettings<BasicOverhaulConfig> 
    {
        public override string Id => "basic_overhaul";
        public override string DisplayName => $"Basic Overhaul {Regex.Replace(ModuleHelper.GetModuleInfo(Assembly.GetExecutingAssembly().GetName().Name).Version.ToString(), @"v|\.0$", string.Empty)}";
        public override string FolderName => "BasicOverhaul";
        public override string FormatType => "json";
        
        [SettingPropertyGroup("General")]
        [SettingPropertyBool("Enable slave system", Order = 1, RequireRestart = false)]
        public bool EnableSlaveSystem { get; set; } = true;
        
        [SettingPropertyGroup("General")]
        [SettingPropertyBool("Enable desertion system", HintText = "If you enable this parties of deserters will spawn everytime an after-battle have deserters.", RequireRestart = true, Order = 1)]
        public bool EnableDeserterParties { get; set; } = false;
        
        [SettingPropertyInteger("Party Size Limit Multiplier", minValue: 0, maxValue: 20, "#0x", Order = 1,
            RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public int PartySizeLimitMultiplier { get; set; } = 2;
        
        [SettingPropertyFloatingInteger("Global Loot Chance", minValue: 0f, maxValue: 1f, "#0%", Order = 2, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public float GlobalLootChance { get; set; } = 3;
        
        [SettingPropertyInteger("Battle Renown Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 3, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public int BattleRenownGainMultiplier { get; set; } = 4;
        
        [SettingPropertyInteger("Battle Influence Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 4, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public int BattleInfluenceGainMultiplier { get; set; } = 5;
        
        [SettingPropertyFloatingInteger("Battle Morale Gain Multiplier", minValue: 0f, maxValue: 10f, "#0x", Order = 5, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public float BattleMoraleGainMultiplier { get; set; } = 6;
        
        [SettingPropertyInteger("Recruitment multiplier for parties", minValue: 0, maxValue: 20, "#0x", Order = 6, RequireRestart = false, HintText = "Increases the amount that parties can recruit from settlements and the volunteers production. Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers")]
        public int RecruitmentRate { get; set; } = 7;
        
        [SettingPropertyBool("Disable attack collisions for allies", Order = 8, RequireRestart = false)]
        [SettingPropertyGroup("In Battle")]
        public bool DisableAllyCollision { get; set; } = true;
        
        [SettingPropertyBool("Disable intro", Order = 9, RequireRestart = false)]
        [SettingPropertyGroup("Miscellaneous")]
        public bool DisableIntro { get; set; } = false;
        
        [SettingPropertyBool("Disable new game intro", Order = 10, RequireRestart = false)]
        [SettingPropertyGroup("Miscellaneous")]
        public bool DisableNewIntro { get; set; } = false;
    }
}
