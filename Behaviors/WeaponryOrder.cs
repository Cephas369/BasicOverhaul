using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Core.ItemObject;
using static TaleWorlds.MountAndBlade.Agent;

namespace BasicOverhaul.Behaviors
{
    public enum WeaponryOrderTypes
    {
        WeaponClass,
        WeaponType,
        DamageType,
        DismissOrder
    }
    
    internal class WeaponryOrderMissionBehavior : MissionBehavior
    {
        public static TextObject WeaponryOrderTitle => new("{=str_title_weaponry_order}Restrict weapon use");
        public static TextObject DismissOrderText => new("{=str_dismiss_order}Dismiss Order");
        public static TextObject WeaponClassesText => new("{=str_weapon_classes}By weapon class -->");
        public static TextObject DamageTypesText => new("{=str_damage_types_inquiry}By damage type -->");
        public static TextObject Done => GameTexts.FindText("str_done");
        public static TextObject Cancel => GameTexts.FindText("str_cancel");
        
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        private readonly Dictionary<Agent, AgentWeaponryData> _agentsProperties = new();

        private static List<Formation>? _selectedFormations;

        private readonly WeaponClass[] _weaponClasses = { WeaponClass.OneHandedAxe, WeaponClass.OneHandedPolearm, WeaponClass.OneHandedSword, WeaponClass.TwoHandedAxe, WeaponClass.TwoHandedMace,
            WeaponClass.TwoHandedPolearm, WeaponClass.TwoHandedSword, WeaponClass.Mace };

        private InputKey _weaponryOrderKey = InputKey.Numpad5;

        public override void OnCreated()
        {
            base.OnCreated();
            _weaponryOrderKey = (InputKey)Enum.Parse(typeof(InputKey),
                BasicOverhaulGlobalConfig.Instance?.WeaponryOrderKey?.SelectedValue ?? "Numpad5");
        }
        private void ShowWeaponClassInquiry()
        {
            List<InquiryElement> elements = new List<InquiryElement>();

            foreach (WeaponClass weaponClass in _weaponClasses)
                  elements.Add(new InquiryElement(weaponClass, GameTexts.FindText("str_inventory_weapon", ((int)weaponClass).ToString()).ToString(), null));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(WeaponryOrderTitle.ToString(), new TextObject("{=str_weaponry_order_description_2}Choose the weapon damage type").ToString(), elements, true, 1, 1, Done.ToString(), Cancel.ToString(),
                inquiryElements =>
                {
                    WeaponClass selected = (WeaponClass)inquiryElements[0].Identifier;
                    ChangeFormationWeapon(WeaponryOrderTypes.WeaponClass, selected);
                }, inquiryElements => isInquiryOpen = false));
        }
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if(!agent.IsHero && agent.Team == Mission.PlayerTeam && agent.Equipment != null)
            {
                _agentsProperties.Add(agent, new AgentWeaponryData(agent.Equipment));
            }
        }
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (_agentsProperties.ContainsKey(affectedAgent))
                _agentsProperties.Remove(affectedAgent);
        }
        private bool isInquiryOpen = false;
        private void ShowDamageTypeInquiry()
        {

            List<InquiryElement> elements = new List<InquiryElement>();
            List<TextObject> damageTypesText = GameTexts.FindAllTextVariations("str_inventory_dmg_type").ToList();

            for (DamageTypes damageType = DamageTypes.Cut; damageType <= DamageTypes.Blunt; damageType++)
                elements.Add(new InquiryElement(damageType, damageTypesText[(int)damageType].ToString(), null));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(WeaponryOrderTitle.ToString(), new TextObject("{=str_weaponry_order_description_3}Choose the weapon class").ToString(), elements, true, 1, 1, Done.ToString(), Cancel.ToString(),
                inquiryElements =>
                {
                    DamageTypes selected = (DamageTypes)inquiryElements[0].Identifier;
                    ChangeFormationWeapon(WeaponryOrderTypes.DamageType, default, default, selected);
                }, inquiryElements=>isInquiryOpen = false));
        }
        
        public override void OnMissionTick(float dt)
        {
            if (!isInquiryOpen && Input.IsKeyReleased(_weaponryOrderKey) && _selectedFormations?.Any() == true)
            {
                List<InquiryElement> elements = new List<InquiryElement>();

                elements.Add(new InquiryElement(0, WeaponClassesText.ToString(), null));
                elements.Add(new InquiryElement(1, DamageTypesText.ToString(), null));
                
                for (ItemTypeEnum weaponType = ItemTypeEnum.OneHandedWeapon; weaponType <= ItemTypeEnum.Polearm; weaponType++)
                    elements.Add(new InquiryElement(weaponType, GameTexts.FindText("str_inventory_type_"+(int)weaponType).ToString(), null));

                elements.Add(new InquiryElement(5, DismissOrderText.ToString(), null));

                isInquiryOpen = true;
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(WeaponryOrderTitle.ToString(), 
                    new TextObject("{=str_weaponry_order_description_1}Choose which weapon you want to restrict for this formation").ToString(), elements, true, 1, 1, Done.ToString(), Cancel.ToString(),
                    inquiryElements => 
                    {
                        int selected = (int)inquiryElements[0].Identifier;
                        if(selected == 0)
                            ShowWeaponClassInquiry();
                        else if(selected == 1)
                            ShowDamageTypeInquiry();
                        else if(selected > 1 && selected < 5)
                            ChangeFormationWeapon(WeaponryOrderTypes.WeaponType, default, (ItemTypeEnum)selected);
                        else if(selected == 5)
                            ChangeFormationWeapon(WeaponryOrderTypes.DismissOrder);
                    }, inquiryElements => isInquiryOpen = false));
            }
        }
        private void ResetAgentEquipment(Agent agent)
        {
            for (EquipmentIndex equipmentIndex = EquipmentIndex.Weapon0; equipmentIndex <= EquipmentIndex.Weapon3; equipmentIndex++)
            {
                MissionWeapon weapon = _agentsProperties[agent][equipmentIndex];
                agent.EquipWeaponWithNewEntity(equipmentIndex, ref weapon);
                agent.UpdateWeapons();
            }
            _agentsProperties[agent].isFollowingOrder = false;
        }
        private bool DoDamageTypeResearch(WeaponComponentData weapon, DamageTypes damageType) => (damageType == DamageTypes.Pierce && weapon.IsPolearm) || weapon.SwingDamageType == damageType;
        private void ChangeFormationWeapon(WeaponryOrderTypes type, WeaponClass weaponClass = WeaponClass.Undefined, ItemTypeEnum itemType = ItemTypeEnum.Invalid, DamageTypes damageType = DamageTypes.Invalid)
        {
            foreach(Formation formation in _selectedFormations)
                foreach (Agent agent in formation.GetUnitsWithoutDetachedOnes())
                {
                    if (agent == null || !agent.IsActive() || agent.IsMount || !_agentsProperties.ContainsKey(agent))
                        continue;

                    if(type == WeaponryOrderTypes.DismissOrder)
                    {
                        ResetAgentEquipment(agent);
                        continue;
                    }

                    //Search the weapon at the initial agent equipment
                    EquipmentIndex weaponTargetIndex = EquipmentIndex.None;
                    for (EquipmentIndex equipmentIndex = EquipmentIndex.Weapon0; equipmentIndex <= EquipmentIndex.Weapon3; equipmentIndex++)
                    {
                        MissionWeapon missionWeapon = _agentsProperties[agent][equipmentIndex];
                        if (!missionWeapon.IsEmpty && !missionWeapon.IsAnyAmmo() && !missionWeapon.IsShield())
                        {
                            switch (type)
                            {
                                case WeaponryOrderTypes.WeaponClass:
                                    if (missionWeapon.CurrentUsageItem.WeaponClass == weaponClass)
                                        weaponTargetIndex = equipmentIndex;
                                    break;
                                case WeaponryOrderTypes.WeaponType:
                                    if (missionWeapon.Item.Type == itemType)
                                        weaponTargetIndex = equipmentIndex;
                                    break;
                                case WeaponryOrderTypes.DamageType:
                                    if (DoDamageTypeResearch(missionWeapon.CurrentUsageItem, damageType))
                                        weaponTargetIndex = equipmentIndex;
                                    break;
                            }
                        }
                    }

                    
                    if (_agentsProperties[agent].isFollowingOrder && weaponTargetIndex != EquipmentIndex.None) //If agent is already following order, resets the equipment to order it again
                    {
                        ResetAgentEquipment(agent);
                    }
                    else if (weaponTargetIndex == EquipmentIndex.None) //If he don't have this kind of weapon, cancel
                        continue;

                    //Remove other weapons
                    _agentsProperties[agent].isFollowingOrder = true;
                    for (EquipmentIndex equipmentIndex = EquipmentIndex.Weapon0; equipmentIndex <= EquipmentIndex.Weapon3; equipmentIndex++)
                    {
                        if (equipmentIndex != weaponTargetIndex && !agent.Equipment[equipmentIndex].IsShield())
                        {
                            agent.RemoveEquippedWeapon(equipmentIndex);
                        }
                    }
                   
                    //Use weapon
                    if (weaponTargetIndex != EquipmentIndex.None)
                    {
                        agent.TryToWieldWeaponInSlot(weaponTargetIndex, WeaponWieldActionType.WithAnimation, false);
                    }
                }
            isInquiryOpen = false;
        }

       
        [HarmonyPatch(typeof(OrderController), "OnSelectedFormationsCollectionChanged")]
        private class SelectFormation
        {
            [HarmonyPrefix]
            static void Prefix(OrderController __instance)
            {
                _selectedFormations = __instance.SelectedFormations.Where(x => x.PlayerOwner == Main).ToList();
            }
        }
    }

    internal class AgentWeaponryData
    {
        private readonly MissionWeapon[] _weaponSlots;

        public MissionWeapon this[int index]
        {
            get => _weaponSlots[index];
            set => _weaponSlots[index] = value;
        }

        public MissionWeapon this[EquipmentIndex index]
        {
            get => _weaponSlots[(int)index];
            set => this[(int)index] = value;
        }
        public bool isFollowingOrder;
        public AgentWeaponryData(MissionEquipment missionEquipment, bool IsFollowingOrder = false)
        {
            isFollowingOrder = IsFollowingOrder;
            _weaponSlots = new MissionWeapon[5];
            for (int i = 0; i < _weaponSlots.Length - 1; i++)
            {
                _weaponSlots[i] = missionEquipment[i];
            }
        }
    }
}
