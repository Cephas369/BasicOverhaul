using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using MCM.Common;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul
{
    internal sealed class BasicOverhaulGlobalConfig : AttributeGlobalSettings<BasicOverhaulGlobalConfig> 
    {
        public override string Id => "basic_overhaul";
        public override string DisplayName => $"Basic Overhaul {Regex.Replace(ModuleHelper.GetModuleInfo(Assembly.GetExecutingAssembly().GetName().Name).Version.ToString(), @"v|\.0$", string.Empty)}";
        public override string FolderName => "BasicOverhaul";
        public override string FormatType => "json";
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.1}Enable slave system", Order = 1, RequireRestart = true)]
        public bool EnableSlaveSystem { get; set; } = true;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.2}Enable desertion system", HintText = "{=bo_config_description.2}If you enable this parties of deserters will spawn everytime an after-battle have deserters.", RequireRestart = true, Order = 2)]
        public bool EnableDeserterParties { get; set; } = false;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyInteger("{=bo_config_title.3}Count for each troop on the cheat party screen", minValue:0, maxValue: 2000, HintText = "{=leave_0_for_default}Leave 0 for the default game count.", RequireRestart = false, Order = 3)]
        public int CheatTroopCount { get; set; } = 0;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyInteger("{=bo_config_title.4}Count for each item on the cheat inventory screen", minValue:0, maxValue: 2000, HintText = "{=leave_0_for_default}Leave 0 for the default game count.", RequireRestart = false, Order = 4)]
        public int CheatItemCount { get; set; } = 0;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.5}Enable filters on party screen", RequireRestart = true, Order = 5)]
        public bool EnablePartyScreenFilters { get; set; } = true;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.6}Enable more filters on inventory screen", RequireRestart = false, Order = 6)]
        public bool EnableInventoryScreenFilters { get; set; } = true;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.7}Enable being possible for notables being governors", RequireRestart = true, Order = 7)]
        public bool EnableGovernorNotables { get; set; } = true;
        
        [SettingPropertyGroup("{=general}General", GroupOrder = 1)]
        [SettingPropertyBool("{=bo_config_title.16}Enable limit max skill levels at 300", RequireRestart = false, Order = 8)]
        public bool EnableLimitSkill { get; set; } = true;

        [SettingPropertyGroup("{=in_battle}In Battle", GroupOrder = 2)]
        [SettingPropertyBool("{=bo_config_title.8}Disable attack collisions for allies", Order = 1, RequireRestart = false)]
        public bool DisableAllyCollision { get; set; } = true;
        
        [SettingPropertyGroup("{=in_battle}In Battle", GroupOrder = 2)]
        [SettingPropertyInteger("{=bo_config_title.9}Horse call required riding skill (Key X)", minValue: -1, maxValue: 300, Order = 2, RequireRestart = false, HintText = "{=bo_config_description.9}If you're using another mod that have this mechanic just leave it 0 to disable it.")]
        public int HorseCallSkill { get; set; } = 0;
        
        [SettingPropertyGroup("{=in_battle}In Battle", GroupOrder = 2)]
        [SettingPropertyBool("{=bo_config_title.10}Disable boundaries crossing for player", Order = 3, RequireRestart = false)]
        public bool DisableMissionBoundaries { get; set; } = false;
        
        [SettingPropertyGroup("{=in_battle}In Battle", GroupOrder = 2)]
        [SettingPropertyBool("{=bo_config_title.11}Enable spawn pack animal in battles for accessing inventory", HintText = "{=bo_config_description.11}Campaign only.", Order = 4, RequireRestart = false)]
        public bool EnablePackMule { get; set; } = false;
        
        [SettingPropertyGroup("{=in_battle}In Battle", GroupOrder = 2)]
        [SettingPropertyBool("{=bo_config_title.12}Enable weaponry order (CTRL + W)", HintText = "", Order = 5, RequireRestart = false)]
        public bool EnableWeaponryOrder { get; set; } = true;

        [SettingPropertyBool("{=bo_config_title.13}Disable intro", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=miscellaneous}Miscellaneous", GroupOrder = 3)]
        public bool DisableIntro { get; set; } = false;
        
        [SettingPropertyBool("{=bo_config_title.14}Disable new game intro", Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=miscellaneous}Miscellaneous", GroupOrder = 3)]
        public bool DisableNewIntro { get; set; } = false;
        
        [SettingPropertyGroup("{=miscellaneous}Miscellaneous", GroupOrder = 3)]
        [SettingPropertyBool("{=bo_config_title.15}Enable switch cheat mode by this mod config", HintText = "{=bo_config_description.15}Enable this if you want to change the cheat mode by the Basic Overhaul Campaign Configs instead of the game config files.", Order = 3, RequireRestart = false)]
        public bool EnableSwitchCheatMode { get; set; } = false;
        
        [SettingPropertyGroup("{=hotkeys}Hotkeys", GroupOrder = 4)]
        [SettingPropertyDropdown("{=bo_config_title.17}Open menu key", Order = 1, RequireRestart = true)]
        public Dropdown<string> MenuHotKey { get; set; } = new(Enum.GetNames(typeof(InputKey)), selectedIndex: 21);
        
        [SettingPropertyGroup("{=hotkeys}Hotkeys", GroupOrder = 4)]
        [SettingPropertyDropdown("{=bo_config_title.18}Call horse key", Order = 2, RequireRestart = true)]
        public Dropdown<string> CallHorseKey { get; set; } = new(Enum.GetNames(typeof(InputKey)), selectedIndex: 44);
    }
}
