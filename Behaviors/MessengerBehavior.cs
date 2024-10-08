﻿using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace BasicOverhaul.Behaviors;

public enum MessengerObjective
{
    Sending,
    ReturningSuccess,
    ReturningFailed
}

public class MessengerBehavior : CampaignBehaviorBase
{
    private List<MobileParty> _messengerParties = new();
    
    private List<MessengerPartyComponent> _nearToTarget = new();
    private const int MaxDeliveryDays = 10;
    private const float MinimumTargetDistance = 1;
    
    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnPartyHourlyTick);
        CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
    }

    private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
    {
        if (_messengerParties.Contains(mobileParty))
        {
            if (mobileParty.PartyComponent is MessengerPartyComponent messengerPartyComponent &&
                messengerPartyComponent.TargetHero.CurrentSettlement == settlement)
            {
                OnPartyReachedDestination(messengerPartyComponent);
            }
        }
    }

    private void OnTick(float dt)
    {
        if (_nearToTarget.Any())
        {
            List<MessengerPartyComponent> reached = new();
            foreach (var partyComponent in _nearToTarget)
            {
                if (partyComponent.TargetHero.PartyBelongedTo != null && 
                    partyComponent.MobileParty.Position2D.DistanceSquared(partyComponent.TargetHero.PartyBelongedTo.Position2D) <= MinimumTargetDistance)
                {
                    OnPartyReachedDestination(partyComponent);
                    reached.Add(partyComponent);
                }
            }
            
            foreach (var partyComponent in reached)
            {
                _nearToTarget.Remove(partyComponent);
            }
        }
    }

    private void OnPartyHourlyTick(MobileParty mobileParty)
    {
        if (_messengerParties.Contains(mobileParty))
        {
            DecideMessengerDestination(mobileParty);

            if (mobileParty.TotalFoodAtInventory == 0)
            {
                ItemObject random = MBObjectManager.Instance.GetObject<ItemObject>("fish");
                mobileParty.ItemRoster.AddToCounts(random, 10);
            }
        }
    }
    
    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        //Send the message
        campaignGameStarter.AddPlayerLine("companion_send_message_1", "hero_main_options", "companion_send_message_out_1",
            "{=send_message_talk_1}I want you to send a message to someone.", () => Hero.OneToOneConversationHero?.CompanionOf == Clan.PlayerClan,
            null);
        
        campaignGameStarter.AddDialogLine("companion_send_message_2", "companion_send_message_out_1", "companion_send_message_out_2",
            "{=send_message_talk_2}Who should i send it ?", () => true, null);
        
        campaignGameStarter.AddPlayerLine("companion_send_message_3", "companion_send_message_out_2", "close_window",
            "{=send_message_talk_3}(Select...)", () => true,
            () =>
            {
                ShowReceiverHeroesInquiry(Hero.OneToOneConversationHero);
            });

        campaignGameStarter.AddPlayerLine("companion_send_message_4", "companion_send_message_out_2",
            "companion_okay",
            "{=send_message_talk_4}Forget it.", () => true, null);
            
            
        //Failed return
        campaignGameStarter.AddDialogLine("send_message_failed_1", "start", "close_window",
            "{=send_message_talk_5}Unfortunately i couldn't find {RECEIVER} sir.", () =>
            {
                if (Hero.OneToOneConversationHero?.PartyBelongedTo?.PartyComponent is MessengerPartyComponent messengerPartyComponent 
                    && messengerPartyComponent.Objective == MessengerObjective.ReturningFailed)
                {
                    MBTextManager.SetTextVariable("RECEIVER", messengerPartyComponent.TargetHero);
                    return true;
                }

                return false;
            }, null);
    }

    private void ShowReceiverHeroesInquiry(Hero companion)
    {
        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
            new TextObject("{=send_message}Send Message").ToString(), new TextObject("{=select_receiver}Select the receiver").ToString(),
            GetHeroElements().ToList(), false, 0, 1, GameTexts.FindText("str_done").ToString(),
            null, (selectedElements) =>
            {
                if (selectedElements.IsEmpty())
                {
                    return;
                }
        
                object identifier = selectedElements[0].Identifier;
                if (identifier is Hero hero)
                {
                    SendMessengerToHero(companion, hero);
                }
            }, null, "", true), true);
    }
    private IEnumerable<InquiryElement> GetHeroElements()
    {
        foreach (var hero in Hero.AllAliveHeroes.OrderBy(h => h.Name.ToString()))
        {
            yield return new InquiryElement(hero, hero.Name.ToString(), new ImageIdentifier(CharacterCode.CreateFrom(hero.CharacterObject)));
        }
    }
    private void SendMessengerToHero(Hero messenger, Hero receiver)
    {
        messenger.PartyBelongedTo.MemberRoster.AddToCounts(messenger.CharacterObject, -1);
        
        MobileParty mobileParty = MessengerPartyComponent.InitializeParty(messenger, receiver);
        TeleportHeroAction.ApplyImmediateTeleportToPartyAsPartyLeader(messenger, mobileParty);
        mobileParty.Position2D = MobileParty.MainParty.Position2D;
        
        _messengerParties.Add(mobileParty);
        
        MBInformationManager.AddQuickInformation(new TextObject("{=messenger_left_info}The messenger left your party."));
        DecideMessengerDestination(mobileParty);
    }

    private void MakePartyReachHero(MobileParty mobileParty, Hero hero)
    {
        bool isEnemy = hero.MapFaction.IsAtWarWith(mobileParty.MapFaction);
        if (hero.CurrentSettlement != null)
        {
            if (isEnemy)
            {
                if (mobileParty.Position2D.DistanceSquared(hero.CurrentSettlement.GatePosition) < 30f)
                {
                    mobileParty.IgnoreForHours(2);
                    mobileParty.IgnoreByOtherPartiesTill(CampaignTime.Hours(2));
                    mobileParty.Ai.SetDoNotMakeNewDecisions(true);
                }
                if (mobileParty.Position2D.DistanceSquared(hero.CurrentSettlement.GatePosition) < 2f)
                {
                    OnPartyReachedDestination(mobileParty.PartyComponent as MessengerPartyComponent);
                }
                mobileParty.Ai.SetMoveGoToPoint(hero.PartyBelongedTo.Position2D);
            }
            else
            {
                mobileParty.Ai.SetMoveGoToSettlement(hero.CurrentSettlement);
            }
            return;
        }
        if (hero.PartyBelongedTo != null)
        {
            bool isPartyNear;
            if (!isEnemy)
            {
                isPartyNear = mobileParty.Position2D.DistanceSquared(hero.PartyBelongedTo.Position2D) < 12f;
            }
            else
            {
                mobileParty.IgnoreForHours(2);
                mobileParty.IgnoreByOtherPartiesTill(CampaignTime.Hours(2));
                mobileParty.Ai.SetDoNotMakeNewDecisions(true);
                isPartyNear = mobileParty.Position2D.DistanceSquared(hero.PartyBelongedTo.Position2D) < 50f;
            }
            
            if (isPartyNear && mobileParty.PartyComponent is MessengerPartyComponent partyComponent)
            {
                if(!_nearToTarget.Contains(partyComponent))
                    _nearToTarget.Add(partyComponent);
            }
            
            mobileParty.Ai.SetMoveGoToPoint(hero.PartyBelongedTo.Position2D);
            return;
        }
        
        mobileParty.Ai.SetMoveGoToSettlement(hero.HomeSettlement);
    }

    private void DecideMessengerDestination(MobileParty messengerParty)
    {
        if (messengerParty.PartyComponent is MessengerPartyComponent messengerComponent)
        {
            if (messengerComponent.Objective == MessengerObjective.Sending)
            {
                if (messengerComponent.TargetHero == null || messengerComponent.TargetHero.IsDead || messengerComponent.StartedTime.ElapsedDaysUntilNow > MaxDeliveryDays)
                {
                    messengerComponent.Objective = MessengerObjective.ReturningFailed;
                    DecideMessengerDestination(MobileParty.MainParty);
                }

                MakePartyReachHero(messengerParty, messengerComponent.TargetHero);
            }
            else
            {
                if (Hero.MainHero.IsPrisoner)
                {
                    Settlement settlement = SettlementHelper.FindNearestSettlement(settlement =>
                        !settlement.MapFaction.IsAtWarWith(messengerParty.MapFaction));

                    if (settlement != null)
                    {
                        messengerParty.Ai.SetMoveGoToSettlement(settlement);
                    }
                    else
                    {
                        messengerParty.Ai.RecalculateShortTermAi();
                    }
                }
                else
                {
                    MakePartyReachHero(messengerParty, Hero.MainHero);
                }
            }
            
            messengerParty.Aggressiveness = 0;
        }
        
        messengerParty.IgnoreForHours(2);
    }

    private void OnPartyReachedDestination(MessengerPartyComponent partyComponent)
    {
        if (partyComponent.Objective == MessengerObjective.Sending)
        {
            TextObject textObject = new TextObject("{=messenger_reached_text}{MESSENGER.NAME} reached {RECEIVER.NAME}");
            textObject.SetCharacterProperties("MESSENGER", partyComponent.Owner.CharacterObject);
            textObject.SetCharacterProperties("RECEIVER", partyComponent.TargetHero.CharacterObject);
        
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=message_delivered}Message delivered").ToString(), textObject.ToString(), true, false, 
                GameTexts.FindText("str_done").ToString(), "",
                () =>
                {
                    CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter), new ConversationCharacterData(partyComponent.TargetHero.CharacterObject));
                    partyComponent.Objective = MessengerObjective.ReturningSuccess;
                    partyComponent.TargetHero = Hero.MainHero;
                    partyComponent.MobileParty.Ai.SetDoNotMakeNewDecisions(false);
                    DecideMessengerDestination(MobileParty.MainParty);
                }, null), true);
        }
        else
        {
            if (partyComponent.Objective == MessengerObjective.ReturningFailed)
                CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter), new ConversationCharacterData(partyComponent.Owner.CharacterObject));
            
            DestroyPartyAction.Apply(null, partyComponent.MobileParty);
            TeleportHeroAction.ApplyImmediateTeleportToParty(partyComponent.Owner, MobileParty.MainParty);
            _messengerParties.Remove(partyComponent.MobileParty);
        }
    }

    internal void ReturnAllMessengers()
    {
        MobileParty[] parties = { };
        _messengerParties.CopyTo(parties);

        foreach (var party in parties)
        {
            if (_nearToTarget.Contains(party.PartyComponent))
                _nearToTarget.Remove(party.PartyComponent as MessengerPartyComponent);
            
            DestroyPartyAction.Apply(null, party);
            TeleportHeroAction.ApplyImmediateTeleportToParty(party.Owner, MobileParty.MainParty);
            _messengerParties.Remove(party);
        }
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("_messengerParties", ref _messengerParties);
    }
    
    public class MessengerPartyComponent : LordPartyComponent
    {
        [SaveableField(0)]
        public Hero TargetHero;
        [SaveableField(1)]
        public MessengerObjective Objective = MessengerObjective.Sending;
        [SaveableField(2)]
        public CampaignTime StartedTime;
        
        protected internal MessengerPartyComponent(Hero owner, Hero leader, Hero targetHero) : base(owner, leader)
        {
            TargetHero = targetHero;
        }

        public static MobileParty InitializeParty(Hero messenger, Hero targetHero)
        {
            MobileParty mobileParty =
                MobileParty.CreateParty("messenger_party" + messenger.StringId, new MessengerPartyComponent(messenger, messenger, targetHero));
            
            mobileParty.SetPartyUsedByQuest(true);
            mobileParty.Party.SetVisualAsDirty();
            PartyVisualManager.Current.GetVisualOfParty(mobileParty.Party).ValidateIsDirty(0,0);
            mobileParty.ActualClan = Clan.PlayerClan;

            return mobileParty;
        }

        protected override void OnInitialize()
        {
            StartedTime = CampaignTime.Now;
        }

        protected override void OnFinalize() {}
    }
}