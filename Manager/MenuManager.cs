using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace BasicOverhaul.Manager;

public static class MenuManager
{
    private static readonly List<(BasicOption? Properties, MethodInfo Method)> CampaignCheats = new();
    private static readonly List<(BasicOption? Properties, MethodInfo Method)> MissionCheats = new();
    private static List<string> _currentParameters = new();
    private static bool _isMenuOpened;

    private static void ApplyCheat(List<InquiryElement> inquiryElements)
    {
        MakeMenuClosed();
        if (!inquiryElements.Any())
            return;
        InquiryElement inquiry = inquiryElements[0];
        var cheatTuple = ((BasicOption? Properties, MethodInfo Method))inquiry.Identifier;

        string[]? parameters = cheatTuple.Properties?.Parameters?.Select(text => text.ToString()).ToArray();

        if (parameters == null)
        {
            MakeMenuClosed();
            InformationManager.DisplayMessage(
                new InformationMessage((string)cheatTuple.Method.Invoke(null, new object[] { null })));
            return;
        }

        List<Action<string>> affirmativeActions = new();
        _currentParameters = new();

        for (int i = 0; i < parameters?.Length; i++)
        {
            int index = i;
            Action<string> currentAction = null!;

            currentAction += input =>
            {
                if (input.Length > 0)
                    _currentParameters.Add(input);
            };

            if (i == parameters.Length - 1)
                currentAction += input =>
                {
                    MakeMenuClosed();
                    InformationManager.DisplayMessage(
                        new InformationMessage(
                            (string)cheatTuple.Method.Invoke(null, new object[] { _currentParameters })));
                    _currentParameters.Clear();
                };
            else
            {
                currentAction += input =>
                {
                    InformationManager.ShowTextInquiry(new TextInquiryData(parameters[index + 1], null, true,
                        false, "Ok", null, affirmativeActions[index + 1], null));
                };
            }

            affirmativeActions.Add(currentAction);
        }

        InformationManager.ShowTextInquiry(new TextInquiryData(cheatTuple.Properties.Parameters?[0].ToString(), null,
            true, false,
            "Ok", null, affirmativeActions[0], null));
    }

    public static void OnMenuHotkeyReleased()
    {
        if (MBCommon.IsPaused || Mission.Current?.IsInPhotoMode == true || CampaignCheats.IsEmpty() || _isMenuOpened) 
            return;
        
        var elementCheats = Mission.Current != null ? MissionCheats : Campaign.Current != null ? CampaignCheats : null;

        if (elementCheats == null)
            return;

        List<InquiryElement> inquiryElements = elementCheats
            .Select(element => new InquiryElement(element, element.Properties?.Description, null)).ToList();

        MultiSelectionInquiryData inquiryData = new("Basic Overhaul",
            new TextObject("{=select_option}Select a option to apply.").ToString(),
            inquiryElements, false, 0, 1, "Done", "Cancel",
            ApplyCheat, elements => _isMenuOpened = false);

        MBInformationManager.ShowMultiSelectionInquiry(inquiryData, true);
        _isMenuOpened = true;
    }

    public static void InitializeCheats()
    {
        if (!CampaignCheats.Any())
        {
            CampaignCheats.AddRange(
                from method in AccessTools.GetDeclaredMethods(typeof(Options))
                    .Concat(AccessTools.GetDeclaredMethods(typeof(NativeCheats)))
                let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicOption)) as BasicOption
                where basicCheat != null
                orderby basicCheat.Description.StartsWith("[")
                select (basicCheat, method)
            );
        }

        if (!MissionCheats.Any())
            MissionCheats.AddRange(
                from method in AccessTools.GetDeclaredMethods(typeof(MissionOptions))
                let basicCheat = Attribute.GetCustomAttribute(method, typeof(BasicOption)) as BasicOption
                where basicCheat != null
                orderby basicCheat.Description.StartsWith("[")
                select (basicCheat, method)
            );
    }

    private static void MakeMenuClosed() => _isMenuOpened = false;
}