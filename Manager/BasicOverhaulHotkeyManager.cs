using System;
using System.Reflection;
using Bannerlord.ButterLib.HotKeys;
using BasicOverhaul.Behaviors;
using BasicOverhaul.Manager;
using HarmonyLib;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul;

public class BasicOverhaulHotkeyManager : HotKeyBase    
{
    private InputKey _defaultKey;
    protected override InputKey DefaultKey => _defaultKey;

    private string _displayName;
    protected override string DisplayName => _displayName;

    private Action _onReleased;

    private static bool _initialized;

    protected override void OnReleased()
    {
        _onReleased();
    }

    private BasicOverhaulHotkeyManager(string id, string displayName, InputKey key, Action onReleased) : base(id)
    {
        _displayName = displayName;
        _defaultKey = key;
        _onReleased += onReleased;
    }
    
    public static void Initialize()
    {
        if (_initialized)
            return;
        
        var hotKeyManager = Bannerlord.ButterLib.HotKeys.HotKeyManager.Create("BasicOverhaul");

        if (hotKeyManager == null)
            throw new Exception("Basic Overhaul could not set hotkeys due to hotkey manager being null.");
        
        InputKey fastForwardKey = (InputKey)Enum.Parse(typeof(InputKey),
            BasicOverhaulGlobalConfig.Instance?.FastForwardMissionKey?.SelectedValue ?? "Numpad9");
        hotKeyManager.Add(new BasicOverhaulHotkeyManager("BasicOverhaulSpeedUpHotkey" + fastForwardKey.ToString(), "Basic Overhaul Speed Up", fastForwardKey, OnSpeedUp));
        
        InputKey callHorseKey = SubModule.PossibleKeys[BasicOverhaulGlobalConfig.Instance?.CallHorseKey?.SelectedValue ?? "X"];
        hotKeyManager.Add(new BasicOverhaulHotkeyManager("BasicOverhaulHorseCallHotkey" + callHorseKey.ToString(), "Basic Overhaul Horse Call", callHorseKey, OnHorseCall));
        
        InputKey weaponryOrderKey = (InputKey)Enum.Parse(typeof(InputKey),
            BasicOverhaulGlobalConfig.Instance?.WeaponryOrderKey?.SelectedValue ?? "Numpad5");
        hotKeyManager.Add(new BasicOverhaulHotkeyManager("BasicOverhaulWeaponryOrderHotkey" + weaponryOrderKey.ToString(), "Basic Overhaul Weaponry Order", weaponryOrderKey, OnWeaponryOrder));
        
        InputKey openMenuKey = SubModule.PossibleKeys[BasicOverhaulGlobalConfig.Instance?.MenuHotKey?.SelectedValue ?? "U"];
        hotKeyManager.Add(new BasicOverhaulHotkeyManager("BasicOverhaulMenuHotkey" + openMenuKey.ToString(), "Basic Overhaul Menu", openMenuKey, MenuManager.OnMenuHotkeyReleased));
        
        hotKeyManager.Build();

        _initialized = true;
    }

    private static void OnSpeedUp()
    {
        if (Mission.Current == null || MBCommon.IsPaused)
            return;
            
        Mission.Current.SetFastForwardingFromUI(!Mission.Current.IsFastForward);
    }
    
    private static void OnHorseCall()
    {
        if (HorseCallMissionLogic.Instance == null || MBCommon.IsPaused)
            return;
            
        HorseCallMissionLogic.Instance.OnCallHorse();
    }
    
    private static void OnWeaponryOrder()
    {
        if (WeaponryOrderMissionBehavior.Instance == null || MBCommon.IsPaused)
            return;
        
        WeaponryOrderMissionBehavior.Instance.OnWeaponryOrderKeyReleased();
    }
}