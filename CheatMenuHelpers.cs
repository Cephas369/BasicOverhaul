using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Localization;

namespace BasicOverhaul;

public static class Helpers
{
    public static readonly PropertyInfo CheatModeField = AccessTools.Property(typeof(NativeConfig), "CheatMode");
    
    public static readonly Dictionary<string, Delegate> CheatDescriptionAttributes = new()
    {
        {"cheat_desc.11", () => MissionCheats.MountInvincible.ToString().ToLower()},
        {"cheat_desc.12", () => MissionCheats.PlayerInvincible.ToString().ToLower()},
        {"cheat_desc.13", () => MissionCheats.IsPlayerDamageOp.ToString().ToLower()},
        {"cheat_desc.17", () => CheatModeField.GetValue(null).ToString().ToLower()},
    };
}
public class BasicCheat : Attribute
{
    public readonly TextObject Description;
    public readonly TextObject[]? Parameters;

    public BasicCheat(string description, string[]? parameters = null)
    {
        Description = new TextObject(description);
        Parameters = parameters?.Select(x=>new TextObject(x)).ToArray();
    }
}

