﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Manages adding/removing abilities(equiping) and updates each ability every frame. When weapon abilities are active (primary and secondary abilities) other abilities and skills can override them.

public class PlayerAbilityManager : AbilitySystem
{
    //Abilities Slots
    private PlayerAbility melee;
    private PlayerAbility dodge;
    public PlayerAbility dash;
    public PlayerAbility ranged;
    public PlayerAbility aoe;

    private Transform cooldownOriginTransform;
    private float cooldownHeightDelta;

    //Ability Properties
    public PlayerAbility Melee { get { return melee; } }
    public PlayerAbility Dodge { get { return dodge; } }
    [Obsolete]
    public PlayerAbility Ranged { get { return ranged; } }
    [Obsolete]
    public PlayerAbility AOE { get { return aoe; } }
    public new PlayerAbility CurrentAbility { get { return (PlayerAbility) currentAbility; } set { currentAbility = value; } }

    public bool MeleeAvailable { get; set; }
    public bool DodgeAvailable { get; set; }
    public bool DashAvailable { get; set; }
    public bool RangedAvailable { get; set; }
    public bool HealAvailable { get; set; }
    public bool AbilitiesAvailable { get; set; }

    public const float MaxStamina = 4;
    public float Stamina { get; private set; }
    public float SavedStamina { get; set; }

    public PlayerAbilityManager(
        Animator animator,
        PhysicsSystem physics,
        MovementSystem movement,
        GameObject parent,
        Transform cooldownOriginTransform,
        float cooldownHeightDelta) : base(animator, physics, movement, parent)
    { 
        this.cooldownOriginTransform = cooldownOriginTransform;
        this.cooldownHeightDelta = cooldownHeightDelta;
        InitializePreferences();
        /*
        MeleeAvailable = true;
        RangedAvailable = true;
        HealAvailable = true;
        DodgeAvailable = true;
        DashAvailable = true;
        AbilitiesAvailable = true;
        */
        AbilitiesAvailable = true;

        //Stamina = 0;
    }

    //Updates each ability slot, called by PlayerManager.
	public override void UpdateAbilities() 
    {
        //Try to run specified ability if held down
        bool rangedInput = (GameInfo.Manager.ReceivingInput && RangedAvailable && AbilitiesAvailable) ? Input.GetAxis("Right Trigger") != 0 : false;
        bool aoeInput = (GameInfo.Manager.ReceivingInput && HealAvailable && AbilitiesAvailable) ? Input.GetKey(KeyCode.Joystick1Button4) : false;
        bool meleeInput = (GameInfo.Manager.ReceivingInput && MeleeAvailable && AbilitiesAvailable) ? 
            Input.GetKey(GameInfo.Settings.MeleeAbilityKey) || Input.GetKey(GameInfo.Settings.AlternateMeleeAbilityKey) : false;
        bool dashInput = (GameInfo.Manager.ReceivingInput && DashAvailable && AbilitiesAvailable) ? Input.GetKey(GameInfo.Settings.DashAbilityKey) : false;
        bool dodgeInput = (GameInfo.Manager.ReceivingInput && DodgeAvailable && AbilitiesAvailable) ? Input.GetKey(GameInfo.Settings.DodgeAbilityKey) : false;

        //Weapon prioritization
        PlayerAbility weaponHeldDown = null;
        
        if (rangedInput)
            weaponHeldDown = ranged;
        
        if (aoeInput)
            weaponHeldDown = aoe;

        if (meleeInput)
            weaponHeldDown = melee;

        if (dashInput)
            weaponHeldDown = dash;

        if (dodgeInput)
            weaponHeldDown = dodge;

        if (ranged != null)
            ranged.UpdateAbility(ranged == weaponHeldDown, rangedInput);

        if (aoe != null)
            aoe.UpdateAbility(aoe == weaponHeldDown, aoeInput);

        if (melee != null)
            melee.UpdateAbility(melee == weaponHeldDown, meleeInput);

        if (dash != null)
            dash.UpdateAbility(dash == weaponHeldDown, dashInput);

        if (dodge != null)
            dodge.UpdateAbility(dodge == weaponHeldDown, dodgeInput);
	}

    public override bool Ready()
    {
        return Physics.TouchingFloor && !PlayerInfo.Animator.GetBool("jump") && !PlayerInfo.Animator.GetBool("falling");;
    }

    //Equip an ability to an ability slot, overriding the current ability if available.
    public void EquipAbility<T>(ref PlayerAbility abilitySlot) where T : PlayerAbility
    {
        GameObject.Destroy(abilitySlot);
        T t = PlayerInfo.Player.AddComponent<T>();
        t.Initialize(this);
        abilitySlot = t;
        UpdateCooldownIconPositions();
    }

    //Clear an ability slot.
    public void UnequipAbility<T>(ref PlayerAbility slot) where T : PlayerAbility
    {
        PlayerAbility temp = slot;
        if (currentAbility == temp)
            temp.ShortCircuit(true);
        temp.DeleteResources();
        GameObject.Destroy(temp);
        slot = null;
        UpdateCooldownIconPositions();
    }

    //Initializes equiped abilities. Implementation is temporary, will load from file.
    private void InitializePreferences()
    {  
        EquipAbility<PlayerSword>(ref melee);
        EquipAbility<PlayerDodge>(ref dodge);
        //EquipAbility<PlayerDash>(ref dash);
        //EquipAbility<PlayerFireball>(ref ranged);
        //EquipAbility<PlayerFireChargeTier1>(ref aoe);
    }

    private void UpdateCooldownIconPositions()
    {
        var cooldownIconOrder = new List<PlayerAbility>();
        cooldownIconOrder.Add(aoe);
        cooldownIconOrder.Add(dash);
        cooldownIconOrder.Add(ranged);
        //cooldownIconOrder.Add(aoe);

        float heightOffset = 0;

        for (int i = 0; i < cooldownIconOrder.Count; i++)
        {
            PlayerAbility a = cooldownIconOrder[i];
            if (a != null)
            {
                if (a.CooldownSlider != null)
                {
                    ((RectTransform) a.CooldownSlider.transform.parent).anchoredPosition =
                        ((RectTransform) cooldownOriginTransform).anchoredPosition + new Vector2(0, heightOffset);
                    
                    heightOffset += cooldownHeightDelta;
                }
            }
        }
    }

    public void ChangeStamina(float value)
    {
        Stamina = Mathf.Clamp(Stamina + value, 0, MaxStamina);

        float percentage = Stamina / MaxStamina;

        if (percentage == 0f)
        {
            PlayerInfo.Manager.StaminaSlider1.value = 0;
            PlayerInfo.Manager.StaminaSlider2.value = 0;
            PlayerInfo.Manager.StaminaSlider3.value = 0;
            PlayerInfo.Manager.StaminaSlider4.value = 0;
        }
        else if (percentage <= 1 / 4f && percentage > 0f)
        {
            PlayerInfo.Manager.StaminaSlider1.value = percentage * 4f;
            PlayerInfo.Manager.StaminaSlider2.value = 0;
            PlayerInfo.Manager.StaminaSlider3.value = 0;
            PlayerInfo.Manager.StaminaSlider4.value = 0;
        }   
        else if (percentage <= 2 / 4f && percentage > 1 / 4f)
        {
            PlayerInfo.Manager.StaminaSlider1.value = 1;
            PlayerInfo.Manager.StaminaSlider2.value = (percentage - (1 / 4f)) * 4f;
            PlayerInfo.Manager.StaminaSlider3.value = 0;
            PlayerInfo.Manager.StaminaSlider4.value = 0;
        }
        else if (percentage <= 3 / 4f && percentage > 2 / 4f)
        {
            PlayerInfo.Manager.StaminaSlider1.value = 1;
            PlayerInfo.Manager.StaminaSlider2.value = 1;
            PlayerInfo.Manager.StaminaSlider3.value = (percentage - (2 / 4f)) * 4f;
            PlayerInfo.Manager.StaminaSlider4.value = 0;
        }
        else if (percentage <= 1 && percentage > 3 / 4f)
        {
            PlayerInfo.Manager.StaminaSlider1.value = 1;
            PlayerInfo.Manager.StaminaSlider2.value = 1;
            PlayerInfo.Manager.StaminaSlider3.value = 1;
            PlayerInfo.Manager.StaminaSlider4.value = (percentage - (3 / 4f)) * 4f;
        }

        if (melee != null)
            melee.UpdateStaminaCostIcons();
        if (dodge != null)
            dodge.UpdateStaminaCostIcons();
        if (dash != null)
            dash.UpdateStaminaCostIcons();
        if (ranged != null)
            ranged.UpdateStaminaCostIcons();
        if (aoe != null)
            aoe.UpdateStaminaCostIcons();
    }

    public void ResetCooldowns()
    {
        if (melee != null && !melee.Continous)
            melee.ResetCooldown();

        if (dodge != null && !dodge.Continous)
            dodge.ResetCooldown();

        if (dash != null && !dash.Continous)
            dash.ResetCooldown();

        if (ranged != null && !ranged.Continous)
            ranged.ResetCooldown();

        if (aoe != null && !aoe.Continous)
            aoe.ResetCooldown();
    }

    public void ShortCircuit(bool allowNormalExit)
    {
        if (currentAbility != null)
        {
            currentAbility.ShortCircuit();
        }

        if (!allowNormalExit)
        {
            PlayerInfo.Animator.ResetTrigger("proceedAbility");
        }
    }
}