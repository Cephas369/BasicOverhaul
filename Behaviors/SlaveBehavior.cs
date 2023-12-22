using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
    internal class SlaveSettlementProsperityModel : DefaultSettlementProsperityModel
    {
        public override ExplainedNumber CalculateProsperityChange(Town fortification, bool includeDescriptions = false)
        {
            ExplainedNumber baseNumber = base.CalculateProsperityChange(fortification, includeDescriptions);
            Settlement settlement = fortification.Settlement;

            if (settlement == null) return baseNumber;
            string settlementId = settlement.StringId;
            if (SlaveBehavior.Instance.SlaveData.ContainsKey(settlementId) && SlaveBehavior.Instance.SlaveData[settlementId].DestinationType == DestinationTypes.TownProsperity)
            {
                baseNumber.Add(SlaveBehavior.Instance.SlaveData[settlementId].SlaveAmount / 2f,
                    new TextObject("{=bo_slavery}Slavery"));
            }

            return baseNumber;
        }
    }

    internal class SlaveBuildingConstructionModel : DefaultBuildingConstructionModel
    {
        public override ExplainedNumber CalculateDailyConstructionPower(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseNumber = base.CalculateDailyConstructionPower(town, includeDescriptions);
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

    internal class SlaveClanFinanceModel : DefaultClanFinanceModel
    {
        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false,
            bool applyWithdrawals = false, bool includeDetails = false)
        {
            

            ExplainedNumber baseNumber = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);
            if (clan == Clan.PlayerClan)
                foreach (Settlement settlement in clan.Settlements.Where(x =>
                             x.IsTown && SlaveBehavior.Instance.SlaveData.ContainsKey(x.StringId) &&
                             SlaveBehavior.Instance.SlaveData[x.StringId].DestinationType == DestinationTypes.Clan))
                {
                    float profit = SlaveBehavior.Instance.SlaveData[settlement.StringId].SlaveAmount * 4;
                    baseNumber.Add(profit,
                        new TextObject("{=bo_town_slavery}{TOWN} Slavery").SetTextVariable("TOWN", settlement.Name));
                }

            return baseNumber;
        }
    }
    internal class TownSlaveData
    {
        public TownSlaveData(int slaves, DestinationTypes destination)
        {
            SlaveAmount = slaves;
            DestinationType = destination;
        }
        [SaveableField(1)]
        public int SlaveAmount;
        [SaveableField(2)]
        public DestinationTypes DestinationType;
    }
    internal class SlaveBehavior : CampaignBehaviorBase
    {
        public Dictionary<string, TownSlaveData> SlaveData = new();

        public static SlaveBehavior? Instance;

        private readonly int _slavePlantationCost = 50000;

        public SlaveBehavior()
        {
            Instance = this;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, SessionLaunched);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
        }

        private void WeeklyTick()
        {
            foreach (string settlementId in SlaveData.Keys)
            {
                if (MBRandom.RandomFloat > 0.1f)
                {
                    Settlement settlement = Settlement.Find(settlementId);
                    float factor = MBRandom.RandomFloat;
                    int lostSlaves = (int)(0.1f * SlaveData[settlementId].SlaveAmount * factor);
                    SlaveData[settlementId].SlaveAmount =- lostSlaves;
                    if(lostSlaves > 0)
                        InformationManager.ShowInquiry(new InquiryData(new TextObject("{=bo_slave_loss_title}Exhaustion").ToString(),
                            new TextObject("{=bo_slave_loss_description}{AMOUNT} slaves prisoners have died this week.").SetTextVariable("AMOUNT", lostSlaves).ToString(),
                            true, false, "Done", "", null, null), true);
                }
            }
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (SlaveData.ContainsKey(settlement.StringId) &&
                (newOwner.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) || newOwner.GetRelationWithPlayer() < 0))
            {
                SlaveData.Remove(Settlement.CurrentSettlement.StringId);
                TextObject textObject = new TextObject("{=slave_settlement_owner_change}{SETTLEMENT} have been occupied by an enemy and you plantation have been destroyed");
                textObject.SetTextVariable("SETTLEMENT", settlement.Name);
                InformationManager.ShowInquiry(new InquiryData("{=event}Event", textObject.ToString(), true, false, "{=done}Done", "", null, null));
            }
        }

        private bool IsBuildSlaveryPossible => Settlement.CurrentSettlement?.IsTown == true && !SlaveData.ContainsKey(Settlement.CurrentSettlement.StringId);

        private void SessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town", "town_build_slave",
                "{=bo_introduce_slavery}Introduce slavery ({COST}{GOLD_ICON})", args =>
                {
                    MBTextManager.SetTextVariable("COST", string.Format("{0:n0}", _slavePlantationCost));
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    if (Hero.MainHero.Gold < _slavePlantationCost)
                    {
                        args.IsEnabled = false;
                        args.Tooltip = new TextObject("{=bo_not_enough_money}You don't have enough money");
                    }

                    return IsBuildSlaveryPossible;
                }, args =>
                {
                    SlaveData.Add(Settlement.CurrentSettlement.StringId, new TownSlaveData(0, DestinationTypes.Clan));
                    GameMenu.SwitchToMenu("town");
                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -_slavePlantationCost);
                }, false, 4);

            campaignGameStarter.AddGameMenu("town_manage_slavery", "", null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithBoth,
                GameMenu.MenuFlags.None, this);

            campaignGameStarter.AddGameMenuOption("town", "town_manage_slavery", "{=bo_manage_slavery}Manage slavery",
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
                            new TextObject("{=bo_player_income}Player Income").ToString(), null),

                        new(DestinationTypes.TownProsperity,
                            new TextObject("{=bo_town_prosperity}Town Prosperity").ToString(), null),

                        new(DestinationTypes.Construction,
                            new TextObject("{=bo_construction}Construction").ToString(), null)
                    };

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=bo_profit_allocation}Profit allocation").ToString(),
                        new TextObject(
                                "{=bo_profit_allocation_description}Choose where your slavery profit will be spent")
                            .ToString(), elements
                        , true, 1, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                        doneElements =>
                        {
                            SlaveData[Settlement.CurrentSettlement.StringId].DestinationType =
                                (DestinationTypes)doneElements.First().Identifier;
                        }, null, string.Empty));
                }, false);

            campaignGameStarter.AddGameMenuOption("town_manage_slavery", "remove_slavery",
                "{=bo_remove_slavery}Remove slavery", args =>
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
                PartyScreenLogic.TransferState.Transferable, new TextObject("Slave"), 10000, false, true,
                PartyScreenMode.PrisonerManage, TroopRoster.CreateDummyTroopRoster(),
                slaveRoster);
        }

        private void RemoveSlaveryFromMenu(MenuCallbackArgs args)
        {
            InformationManager.ShowInquiry(new InquiryData("Confirmation",
                new TextObject(
                        "{=bo_remove_slavery_description}Are you sure you want undo slavery from {SETTLEMENT_NAME} ?")
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
        }
    }
}