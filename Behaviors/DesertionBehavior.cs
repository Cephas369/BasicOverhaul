using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BasicOverhaul.Behaviors
{
    internal class DesertionBehavior : CampaignBehaviorBase
    {
        
        public static Clan DesertersClan => Clan.FindFirst(x => x.StringId == "deserters") ?? Clan.FindFirst(x => x.StringId == "looters");
        [SaveableField(0)]
        private Dictionary<MobileParty, Settlement> _partiesGoingToSettlement = new();
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, (campaign) => 
            {
                foreach (IFaction faction1 in Kingdom.All)
                    FactionManager.DeclareWar(faction1, DesertersClan);
                FactionManager.DeclareWar(Hero.MainHero.Clan, DesertersClan);
            });
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, CheckDesertion);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, JoinSettlementMilitia);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, GoToSettlementBehavior);

        }
        private void GoToSettlementBehavior(MobileParty party)
        {
            if(_partiesGoingToSettlement.ContainsKey(party))
            {
                if (party.Position2D.DistanceSquared(_partiesGoingToSettlement[party].GatePosition) <= 30)
                {
                    JoinSettlementMilitia(party, _partiesGoingToSettlement[party], null);
                    _partiesGoingToSettlement.Remove(party);
                } 
                else
                    party.Ai.SetMoveGoToSettlement(_partiesGoingToSettlement[party]);
            }

        }
        private void CheckDesertion(MapEvent mapEvent)
        {
            if (mapEvent.HasWinner && (mapEvent.IsFieldBattle || mapEvent.IsSiegeAssault) && mapEvent.IsFinished)
            {
                if (mapEvent.IsSiegeAssault && mapEvent.DefeatedSide == BattleSideEnum.Defender)
                    return;

                MapEventSide defeatedSide = mapEvent.GetMapEventSide(mapEvent.DefeatedSide);

                if (!defeatedSide.LeaderParty.MobileParty.IsLordParty || defeatedSide.LeaderParty.MobileParty.Owner == null)
                    return;

                float averageMorale = (float)defeatedSide.Parties.Average(x => x.Party?.MobileParty?.Morale);

                float factor = (averageMorale / 100 + defeatedSide.LeaderParty.MobileParty.Owner.GetSkillValue(DefaultSkills.Leadership) / 300) / 2;

                int desertorsAmount = (int)(defeatedSide.Parties.Sum(x => x.HealthyManCountAtStart) * factor);

                if (desertorsAmount == 0) return;


                List<(FlattenedTroopRosterElement, string)> allTroops = defeatedSide.Parties.SelectMany(mapeventParty => mapeventParty.Troops.Select(troop => (troop, mapeventParty.Party.MobileParty.StringId))).ToList();
                allTroops.RemoveAll(x => x.Item1.IsKilled);

                if (allTroops.Count == 0) return;

                allTroops.Randomize();
                List<MobileParty> deserterParties = new();
                MobileParty currentParty = null;

                for (int i = 0; i < desertorsAmount; i++)
                {
                    if (allTroops.Count - 1 < i)
                        break;
                    if (i % 100 == 0)
                    {

                        deserterParties.Add(MobileParty.CreateParty("deserter_party", new DeserterPartyComponent(defeatedSide.LeaderParty.MobileParty.HomeSettlement), mobileParty => mobileParty.ActualClan = DesertersClan));
                        currentParty = defeatedSide.Parties.First(x => x.Party.MobileParty.StringId == allTroops[i].Item2).Party.MobileParty;
                    }
                    if (currentParty.MemberRoster.Count > 0 && currentParty.MemberRoster.FindIndexOfTroop(allTroops[i].Item1.Troop) != -1)
                        currentParty.MemberRoster.RemoveTroop(allTroops[i].Item1.Troop, 1);
                    deserterParties[(int)Math.Floor((decimal)i / 100)].AddElementToMemberRoster(allTroops[i].Item1.Troop, 1);
                }

                foreach (var party in deserterParties)
                    CreateDeserterParty(party, defeatedSide.LeaderParty.MobileParty);
            }
        }
        private void CreateDeserterParty(MobileParty party, MobileParty leaveParty)
        {
            party.InitializeMobilePartyAroundPosition(party.MemberRoster, TroopRoster.CreateDummyTroopRoster(), leaveParty.Position2D, 20f, 5f);
            party.InitializePartyTrade(500);
            party.ItemRoster.AddToCounts(Items.All.Find(x => x.StringId == "fish"), party.MemberRoster.TotalManCount / 2);
            party.SetCustomName(new TextObject("{=deserters}Deserters"));


            Kingdom randomKingdom = Kingdom.All.GetRandomElementWithPredicate(x => x != leaveParty.ActualClan.Kingdom);

            Settlement poorTown = randomKingdom.Settlements.Where(x => x.IsTown).GetRandomElementInefficiently();


            if (poorTown != null && party.Party.NumberOfAllMembers < 50 && MBRandom.RandomFloat > 0.5)
            {
                party.Ai.SetInitiative(0.1f, 0.8f, 9999f);
                SetPartyAiAction.GetActionForVisitingSettlement(party, poorTown);
                _partiesGoingToSettlement.Add(party, poorTown);
            }
        }
        private void JoinSettlementMilitia(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty?.StringId.Contains("deserter_party") == true)
            {
                foreach (var soldier in mobileParty.MemberRoster.GetTroopRoster())
                {
                    CharacterObject newMilitia = CharacterObject.FindAll(x => !x.IsHero && x.IsSoldier && x.IsRegular && x.Culture == settlement.Culture &&
                    x.Tier == soldier.Character.Tier && x.DefaultFormationClass == soldier.Character.DefaultFormationClass && soldier.Character.Occupation == Occupation.Soldier).GetRandomElementInefficiently();

                    settlement.Town.GarrisonParty.AddElementToMemberRoster(newMilitia != null ? newMilitia : soldier.Character, soldier.Number);
                }

                DestroyPartyAction.Apply(settlement.Party, mobileParty);

            }
        }
        public class DeserterPartyComponent : BanditPartyComponent
        {

            protected internal DeserterPartyComponent(Settlement settlement) : base(settlement)
            {
            }
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

    }
}
