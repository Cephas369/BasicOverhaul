using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BasicOverhaul.Models
{
   internal class BOPartyModel : DefaultPartySizeLimitModel
    {
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber baseNumber = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            int multiplier = BasicOverhaulCampaignConfig.Instance != null
                ? BasicOverhaulCampaignConfig.Instance.PartySizeLimitMultiplier
                    : 0;
            
            if (multiplier > 0)
                baseNumber.AddFactor(multiplier, new TextObject("Basic Overhaul"));
            return baseNumber;
        }
        
    }
}
