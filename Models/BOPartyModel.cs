using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models
{
   internal class BOPartySizeLimitModel : DefaultPartySizeLimitModel
    {
        private PartySizeLimitModel _previousModel;

        public BOPartySizeLimitModel(PartySizeLimitModel previousModel)
        {
            _previousModel = previousModel;
        }
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseNumber = _previousModel.GetPartyMemberSizeLimit(party, includeDescriptions);
            int multiplier = BasicOverhaulCampaignConfig.Instance != null
                ? BasicOverhaulCampaignConfig.Instance.PartySizeLimitMultiplier
                    : 0;
            
            if (multiplier > 0)
                baseNumber.AddFactor(multiplier, new TextObject("Basic Overhaul"));
            return baseNumber;
        }
        
    }
}
