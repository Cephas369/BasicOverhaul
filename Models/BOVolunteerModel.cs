using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BasicOverhaul.Models;

internal class BOVolunteerModel : DefaultVolunteerModel
{
    public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero,
        int useValueAsRelation = -101)
    {
        int baseNumber = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
        if (BasicOverhaulCampaignConfig.Instance?.RecruitmentRate > 0 && sellerHero?.VolunteerTypes != null)
        {
            baseNumber = BasicOverhaulCampaignConfig.Instance.RecruitmentRate * baseNumber;
            baseNumber = baseNumber > sellerHero.VolunteerTypes.Length ? sellerHero.VolunteerTypes.Length : baseNumber;
        }
        return baseNumber;
             
    }
    public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
    {
        try
        {
            float baseNumber = base.GetDailyVolunteerProductionProbability(hero, index, settlement);
            if (BasicOverhaulCampaignConfig.Instance?.RecruitmentRate > 0)
                baseNumber = BasicOverhaulCampaignConfig.Instance.RecruitmentRate * baseNumber;
            return baseNumber;
        }
        catch (Exception e)
        {
            return 0;
        }
    }
}