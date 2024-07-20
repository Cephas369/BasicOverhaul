using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.TwoDimension;

namespace BasicOverhaul.Behaviors
{
    public enum DestinationTypes
    {
        Clan,
        TownProsperity,
        Construction
    }

    internal class TownSlaveData
    {
        [SaveableField(1)]
        public int SlaveAmount;
        [SaveableField(2)]
        public DestinationTypes DestinationType;
        public TownSlaveData(int slaves, DestinationTypes destination)
        {
            SlaveAmount = slaves;
            DestinationType = destination;
        }
    }
    internal class SlaveBehavior : CampaignBehaviorBase
    {
        public Dictionary<string, TownSlaveData> SlaveData = new();

        private Dictionary<Settlement, CampaignTime> _buildEndDates = new();
        private const int PlantationBuildDuration = 4; 
        public static SlaveBehavior? Instance { get; private set; }

        private const int SlavePlantationFactor = 6;

        private readonly Dictionary<DestinationTypes, TextObject> _destinationNames = new()
        {
            { DestinationTypes.Clan, new TextObject("{=bo_player_income}Player Income") },
            { DestinationTypes.Construction, new TextObject("{=bo_construction}Town Constructions") },
            { DestinationTypes.TownProsperity, new TextObject("{=bo_town_prosperity}Town Prosperity") },
        };

        public SlaveBehavior()
        {
            Instance = this;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, SessionLaunched);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        private void OnDailyTick()
        {
            List<Settlement> toRemove = new();
            foreach (var keyValue in _buildEndDates)
            {
                if (CampaignTime.Now >= keyValue.Value)
                {
                    TextObject textObject = new TextObject("{=plantation_building_finished}Your plantation at {SETTLEMENT} has finished being built.");
                    textObject.SetTextVariable("SETTLEMENT", keyValue.Key.Name);

                    SlaveData.Add(keyValue.Key.StringId, new TownSlaveData(0, DestinationTypes.Clan));
                    
                    toRemove.Add(keyValue.Key);
                    
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=event}Event").ToString(), textObject.ToString(), true, false, 
                        GameTexts.FindText("str_done").ToString(), null, null, null) , true);

                }
            }
            
            foreach (var element in toRemove)
                _buildEndDates.Remove(element);
        }

        private void WeeklyTick()
        {
            int lostSlaves = 0;
            foreach (string settlementId in SlaveData.Keys)
            {
                if (MBRandom.RandomFloat > 0.1f)
                {
                    float factor = MBRandom.RandomFloat;
                    lostSlaves += (int)(0.1f * SlaveData[settlementId].SlaveAmount * factor);
                    SlaveData[settlementId].SlaveAmount -= lostSlaves;
                }
            }
            if(lostSlaves > 0)
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=bo_slave_loss_title}Exhaustion").ToString(),
                    new TextObject("{=bo_slave_loss_description}{AMOUNT} slaves prisoners have died this week.").SetTextVariable("AMOUNT", lostSlaves).ToString(),
                    true, false, "Done", "", null, null), true);
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (SlaveData.ContainsKey(settlement.StringId) &&
                (newOwner.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) || newOwner.GetRelationWithPlayer() < 0))
            {
                SlaveData.Remove(settlement.StringId);
                TextObject textObject = new TextObject("{=slave_settlement_owner_change}{SETTLEMENT} have been occupied by an enemy and your plantation has been destroyed");
                textObject.SetTextVariable("SETTLEMENT", settlement.Name);
                InformationManager.ShowInquiry(new InquiryData("{=event}Event", textObject.ToString(), true, false, "{=done}Done", "", null, null));
            }
        }

        private bool IsBuildSlaveryPossible => Settlement.CurrentSettlement?.IsTown == true && !SlaveData.ContainsKey(Settlement.CurrentSettlement.StringId);

        private void SessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "town_build_slave",
                "{=bo_introduce_slavery}Build plantation ({COST}{GOLD_ICON})", args =>
                {
                    float totalCost = Settlement.CurrentSettlement.Town.Prosperity * SlavePlantationFactor;
                    MBTextManager.SetTextVariable("COST", string.Format("{0:n0}", totalCost));
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    if (Hero.MainHero.Gold < totalCost)
                    {
                        args.IsEnabled = false;
                        args.Tooltip = new TextObject("{=bo_not_enough_money.1}You don't have enough money");
                    }

                    if (Hero.MainHero?.Clan?.Tier < 3)
                    {
                        args.IsEnabled = false;
                        args.Tooltip = new TextObject("{=bo_not_enough_money.2}Your clan tier must be at least 3.");
                    }

                    return !_buildEndDates.ContainsKey(Settlement.CurrentSettlement) && IsBuildSlaveryPossible;
                }, args =>
                {
                    _buildEndDates.Add(Settlement.CurrentSettlement, CampaignTime.DaysFromNow(PlantationBuildDuration));
                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(int)(Settlement.CurrentSettlement.Town.Prosperity * SlavePlantationFactor));
                    args.MenuContext.Refresh();
                }, false, 4);
            
            
            campaignGameStarter.AddGameMenuOption("town", "town_building_progress",
                "{=town_building_progress}Plantation building in progress... ({DAYS_LEFT} days)", args =>
                {
                    if (_buildEndDates.TryGetValue(Settlement.CurrentSettlement, out CampaignTime time))
                    {
                        double daysLeft = Math.Round(time.RemainingDaysFromNow, 1);
                        if (daysLeft < 0)
                            daysLeft = 0;
                        
                        MBTextManager.SetTextVariable("DAYS_LEFT", daysLeft);
                        args.optionLeaveType = GameMenuOption.LeaveType.Wait;

                        args.IsEnabled = false;
                    
                        return true;
                    }
                    return false;
                }, null, false, 4);

            campaignGameStarter.AddGameMenu("town_manage_slavery", "", null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithBoth,
                GameMenu.MenuFlags.None, this);

            campaignGameStarter.AddGameMenuOption("town", "town_manage_slavery", "{=bo_manage_slavery}Manage plantation",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return Settlement.CurrentSettlement.IsTown &&
                           SlaveData.ContainsKey(Settlement.CurrentSettlement.StringId);
                }, args => { GameMenu.SwitchToMenu("town_manage_slavery"); }, false, 4);

            campaignGameStarter.AddGameMenuOption("town_manage_slavery", "give_slaves",
                "{=bo_enslave_prisoners}Enslave prisoners", args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;
                    return true;
                }, OpenManageSlave);


            campaignGameStarter.AddGameMenuOption("town_manage_slavery", "profit_allocation",
                "{=bo_profit_allocation}Profit allocation", args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                    return true;
                },
                args =>
                {
                    List<InquiryElement> elements = new List<InquiryElement>()
                    {
                        new(DestinationTypes.Clan,
                            _destinationNames[DestinationTypes.Clan].ToString(), null),

                        new(DestinationTypes.TownProsperity,
                            _destinationNames[DestinationTypes.TownProsperity].ToString(), null),

                        new(DestinationTypes.Construction,
                            _destinationNames[DestinationTypes.Construction].ToString(), null)
                    };
                    TextObject description = new TextObject(
                        "{=bo_profit_allocation_description}Current: {PROFIT}");

                    description.SetTextVariable("PROFIT",
                        _destinationNames[SlaveData[Settlement.CurrentSettlement.StringId].DestinationType]);
                    
                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=bo_profit_allocation}Profit allocation").ToString(), description.ToString(), elements
                        , true, 1, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                        doneElements =>
                        {
                            SlaveData[Settlement.CurrentSettlement.StringId].DestinationType =
                                (DestinationTypes)doneElements.First().Identifier;
                        }, null, string.Empty));
                }, false);

            campaignGameStarter.AddGameMenuOption("town_manage_slavery", "remove_slavery",
                "{=bo_remove_slavery}Destroy plantation", args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    return true;
                }, RemoveSlaveryFromMenu);

            campaignGameStarter.AddGameMenuOption("town_manage_slavery", "town_manage_slavery_back",
                "{=qWAmxyYz}Back to town center", args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, args => GameMenu.SwitchToMenu("town"));
        }

        private void OpenManageSlave(MenuCallbackArgs args)
        {
            TroopRoster slaveRoster = TroopRoster.CreateDummyTroopRoster();
            slaveRoster.AddToCounts(CharacterObject.Find("looter"),
                SlaveData[Settlement.CurrentSettlement.StringId].SlaveAmount);
            PartyScreenManager.OpenScreenWithCondition(
                (CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side,
                    PartyBase LeftOwnerParty) => !character.IsHero, null, OnSlaveDonationDone, null,
                PartyScreenLogic.TransferState.NotTransferable,
                PartyScreenLogic.TransferState.Transferable, new TextObject("{=slave}Slave"), 10000, false, true,
                PartyScreenMode.PrisonerManage, TroopRoster.CreateDummyTroopRoster(),
                slaveRoster);
        }

        private void RemoveSlaveryFromMenu(MenuCallbackArgs args)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=confirmation}Confirmation").ToString(),
                new TextObject(
                        "{=bo_remove_slavery_description}Are you sure you want to destroy the plantation from {SETTLEMENT_NAME} ?")
                    .SetTextVariable("SETTLEMENT_NAME", Settlement.CurrentSettlement.Name).ToString(), true,
                true, new TextObject("{=bo_yes}Yes").ToString(), new TextObject("{=bo_no}No").ToString(), () =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero,
                        SlaveData[Settlement.CurrentSettlement.StringId].SlaveAmount * 4);
                    SlaveData.Remove(Settlement.CurrentSettlement.StringId);
                    GameMenu.SwitchToMenu("town");

                }, null));
        }

        private bool OnSlaveDonationDone(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster,
            TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster,
            FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null,
            PartyBase rightParty = null)
        {
            SlaveData[Settlement.CurrentSettlement.StringId].SlaveAmount = leftPrisonRoster.TotalManCount;
            return true;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("slaveData", ref SlaveData);
            dataStore.SyncData("_buildEndDates", ref _buildEndDates);
            
            if (dataStore.IsLoading && SlaveData == null)
                SlaveData = new();
        }
    }
}