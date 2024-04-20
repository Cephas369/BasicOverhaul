using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using SandBox;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using Module = TaleWorlds.MountAndBlade.Module;

namespace BasicOverhaul.Patches;

[HarmonyPatch(typeof(GameStateManager), "CleanAndPushState")]
public static class CleanAndPushStatePatch
{
    private static bool skipVideo;
    public static void Prefix(ref GameState gameState, int level = 0)
    {
        if (gameState is VideoPlaybackState videoState)
            if ((BasicOverhaulGlobalConfig.Instance?.DisableIntro == true && videoState.VideoPath.Contains("TWLogo_and_Partners")) ||
                (BasicOverhaulGlobalConfig.Instance?.DisableNewIntro == true && videoState.VideoPath.Contains("intro")))
            {
                AccessTools.Property(typeof(VideoPlaybackState), "AudioPath").SetValue(gameState, "");
                skipVideo = true;
            }
    }
    public static void Postfix(ref GameState gameState, int level = 0)
    {
        if (skipVideo && gameState is VideoPlaybackState videoState)
        {
            videoState.OnVideoFinished();
            skipVideo = false;
        }
    }
}