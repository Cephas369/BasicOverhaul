﻿using BasicOverhaul.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models;

internal class BOBuildingConstructionModel : DefaultBuildingConstructionModel
{
    private BuildingConstructionModel _previousModel;

    public BOBuildingConstructionModel(BuildingConstructionModel previousModel)
    {
        _previousModel = previousModel;
    }
    public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
    {
        ExplainedNumber baseNumber = _previousModel.CalculateDailyConstructionPower(town, includeDescriptions);
        Settlement settlement = town.Settlement;
        if (settlement == null)
            return baseNumber;
        string settlementId = settlement.StringId;
        if (SlaveBehavior.Instance.SlaveData.ContainsKey(settlementId) && SlaveBehavior.Instance.SlaveData[settlementId].DestinationType == DestinationTypes.Construction)
        {
            baseNumber.Add(SlaveBehavior.Instance.SlaveData[settlementId].SlaveAmount / 10f,
                new TextObject("{=bo_slavery}Slavery"));
        }

        return baseNumber;
    }
}