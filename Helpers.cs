using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;

namespace BasicOverhaul;

public static class Helpers
{
    public static readonly PropertyInfo CheatModeField = AccessTools.Property(typeof(NativeConfig), "CheatMode");

    public static readonly Dictionary<string, Delegate> CheatDescriptionAttributes = new()
    {
        {"cheat_desc.11", () => MissionOptions.MountInvincible.ToString().ToLower()},
        {"cheat_desc.12", () => MissionOptions.PlayerInvincible.ToString().ToLower()},
        {"cheat_desc.13", () => MissionOptions.IsPlayerDamageOp.ToString().ToLower()},
        {"cheat_desc.17", () => NativeConfig.CheatMode.ToString().ToLower()},
    };

    public static TBaseModel GetExistingModel<TBaseModel>(this IGameStarter campaignGameStarter) where TBaseModel : GameModel
    {
        return (TBaseModel)campaignGameStarter.Models.Last(model => model.GetType().IsSubclassOf(typeof(TBaseModel)));
    }
}
public class BasicOption : Attribute
{
    private readonly TextObject _description;
    public string Description
    {
        get
        {
            if (_description.Value.Contains("{VALUE}") && Helpers.CheatDescriptionAttributes.TryGetValue(_description.GetID(), out Delegate del))
            {
                _description.SetTextVariable("VALUE", (string)del.DynamicInvoke());
            }
            
            return IsCheat ? "[CHEAT] " + _description.ToString() : _description.ToString();
        }
    }

    public readonly TextObject[]? Parameters;
    public readonly bool IsCheat;

    public BasicOption(string description, string[]? parameters = null, bool isCheat = false)
    {
        _description = new TextObject(description);
        Parameters = parameters?.Select(x=>new TextObject(x)).ToArray();
        IsCheat = isCheat;
    }
}

