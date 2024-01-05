using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul
{
    internal sealed class BasicOverhaulConfig : AttributeGlobalSettings<BasicOverhaulConfig> 
    {
        public override string Id => "basic_overhaul";
        public override string DisplayName => $"Basic Overhaul {Regex.Replace(ModuleHelper.GetModuleInfo(Assembly.GetExecutingAssembly().GetName().Name).Version.ToString(), @"v|\.0$", string.Empty)}";
        public override string FolderName => "BasicOverhaul";
        public override string FormatType => "json";
        
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyBool("Enable slave system", Order = 1, RequireRestart = false)]
        public bool EnableSlaveSystem { get; set; } = true;
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyBool("Enable desertion system", HintText = "If you enable this parties of deserters will spawn everytime an after-battle have deserters.", RequireRestart = true, Order = 2)]
        public bool EnableDeserterParties { get; set; } = false;
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyInteger("Count for each troop on the cheat party screen", minValue:0, maxValue: 2000, HintText = "Leave 0 for the default game count.", RequireRestart = false, Order = 3)]
        public int CheatTroopCount { get; set; } = 0;
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyInteger("Count for each item on the cheat inventory screen", minValue:0, maxValue: 2000, HintText = "Leave 0 for the default game count.", RequireRestart = false, Order = 4)]
        public int CheatItemCount { get; set; } = 0;
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyBool("Enable filters on cheat party screen", RequireRestart = true, Order = 5)]
        public bool EnablePartyScreenFilters { get; set; } = true;
        
        [SettingPropertyGroup("General", GroupOrder = 1)]
        [SettingPropertyBool("Enable more filters on inventory screen", RequireRestart = false, Order = 6)]
        public bool EnableInventoryScreenFilters { get; set; } = true;
        
        [SettingPropertyInteger("Party Size Limit Multiplier", minValue: 0, maxValue: 20, "#0x", Order = 1,
            RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public int PartySizeLimitMultiplier { get; set; } = 2;
        
        [SettingPropertyFloatingInteger("Global Loot Chance", minValue: 0f, maxValue: 1f, "#0%", Order = 2, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public float GlobalLootChance { get; set; } = 3;
        
        [SettingPropertyInteger("Battle Renown Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 3, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public int BattleRenownGainMultiplier { get; set; } = 4;
        
        [SettingPropertyInteger("Battle Influence Gain Multiplier", minValue: 0, maxValue: 100, "#0x", Order = 4, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public int BattleInfluenceGainMultiplier { get; set; } = 5;
        
        [SettingPropertyFloatingInteger("Battle Morale Gain Multiplier", minValue: 0f, maxValue: 10f, "#0x", Order = 5, RequireRestart = false, HintText = "Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public float BattleMoraleGainMultiplier { get; set; } = 6;
        
        [SettingPropertyInteger("Recruitment multiplier for parties", minValue: 0, maxValue: 20, "#0x", Order = 6, RequireRestart = false, HintText = "Increases the amount that parties can recruit from settlements and the volunteers production. Leave 0 for the default game chance.")]
        [SettingPropertyGroup("Campaign Modifiers", GroupOrder = 2)]
        public int RecruitmentRate { get; set; } = 7;
        
        [SettingPropertyGroup("In Battle", GroupOrder = 3)]
        [SettingPropertyBool("Disable attack collisions for allies", Order = 1, RequireRestart = false)]
        public bool DisableAllyCollision { get; set; } = true;
        
        [SettingPropertyGroup("In Battle", GroupOrder = 3)]
        [SettingPropertyInteger("Horse call required riding skill (Key X)", minValue: -1, maxValue: 300, Order = 2, RequireRestart = false, HintText = "If you're using another mod that have this mechanic just leave it 0 to disable it.")]
        public int HorseCallSkill { get; set; } = 0;
        
        [SettingPropertyGroup("In Battle", GroupOrder = 4)]
        [SettingPropertyBool("Disable boundaries crossing for player", Order = 3, RequireRestart = false)]
        public bool DisableMissionBoundaries { get; set; } = false;
        
        [SettingPropertyGroup("In Battle", GroupOrder = 4)]
        [SettingPropertyBool("Enable spawn pack animal in battles for accessing inventory", HintText = "Campaign only.", Order = 4, RequireRestart = false)]
        public bool EnablePackMule { get; set; } = false;

        [SettingPropertyBool("Disable intro", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Miscellaneous", GroupOrder = 4)]
        public bool DisableIntro { get; set; } = false;
        
        [SettingPropertyBool("Disable new game intro", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("Miscellaneous", GroupOrder = 4)]
        public bool DisableNewIntro { get; set; } = false;
    }
}
