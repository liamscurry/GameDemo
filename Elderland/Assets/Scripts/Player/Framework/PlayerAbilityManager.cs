#define DevMode

using System;
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
    public PlayerAbility block;
    public PlayerAbility ranged;
    public PlayerAbility aoe;
    public PlayerAbility finisher;

    private Transform cooldownOriginTransform;
    private float cooldownHeightDelta;

    private bool inCombatStance;
    private bool inCombatTransition;
    private float combatTimer;
    private const float combatDuration = 1.5f;
    private float notUsedTimer;
    private const float notUsedDuration = 10f;

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
    public bool BlockAvailable { get; set; }
    public bool RangedAvailable { get; set; }
    public bool HealAvailable { get; set; }
    public bool AbilitiesAvailable { get; set; }

    public const float MaxStamina = 4;
    public float Stamina { get; private set; }
    public float SavedStamina { get; set; }

    public const float DirFocusDuration = 6f;
    public float LastDirFocus { get; set; }
    public Vector3 DirFocus { get; set; }
    public bool MovementDirFocus { get { return Time.time - LastDirFocus <= DirFocusDuration; } }

    public GameObject HoldBar { get; private set; }

    public PlayerAbilityManager(
        Animator animator,
        PhysicsSystem physics,
        MovementSystem movement,
        GameObject parent,
        Transform cooldownOriginTransform,
        float cooldownHeightDelta) : base(animator, PlayerInfo.Controller, physics, movement, parent)
    { 
        GameObject holdBarInstance =
            GameObject.Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Abilities.HoldBar),
                GameInfo.Menu.GameplayUI.transform);
        holdBarInstance.SetActive(false);
        HoldBar = holdBarInstance;

        this.cooldownOriginTransform = cooldownOriginTransform;
        this.cooldownHeightDelta = cooldownHeightDelta;
        InitializePreferences();

        #if DevMode
        MeleeAvailable = true;
        RangedAvailable = true;
        HealAvailable = true;
        DodgeAvailable = true;
        DashAvailable = true;
        BlockAvailable = true;
        AbilitiesAvailable = true;
        #endif
        
        AbilitiesAvailable = true;
        inCombatStance = false;
        //Stamina = 0;

        LastDirFocus = -DirFocusDuration * 2f;
    }

    //Updates each ability slot, called by PlayerManager.
	public override void UpdateAbilities() 
    {
        //Try to run specified ability if held down
        bool rangedInput = (GameInfo.Manager.ReceivingInput && RangedAvailable && AbilitiesAvailable) ? Mathf.Abs(GameInfo.Settings.FireballTrigger) > GameInfo.Settings.FireballTriggerOnThreshold: false;
        bool aoeInput = (GameInfo.Manager.ReceivingInput && HealAvailable && AbilitiesAvailable) ? Input.GetKey(KeyCode.Joystick1Button4) : false;
        bool meleeInput = (GameInfo.Manager.ReceivingInput && MeleeAvailable && AbilitiesAvailable) ? 
            Input.GetKey(GameInfo.Settings.MeleeAbilityKey) || Input.GetKey(GameInfo.Settings.AlternateMeleeAbilityKey) : false;
        bool dashInput = (GameInfo.Manager.ReceivingInput && DashAvailable && AbilitiesAvailable) ? Input.GetKey(GameInfo.Settings.DashAbilityKey) : false;
        bool dodgeInput = (GameInfo.Manager.ReceivingInput && DodgeAvailable && AbilitiesAvailable) ? Input.GetKey(GameInfo.Settings.DodgeAbilityKey) : false;
        bool blockInput = (GameInfo.Manager.ReceivingInput && BlockAvailable && AbilitiesAvailable) ? Input.GetKeyDown(GameInfo.Settings.BlockAbilityKey) : false;
        bool finisherInput = 
            (GameInfo.Manager.ReceivingInput && AbilitiesAvailable) ? 
            Input.GetKeyDown(GameInfo.Settings.FinisherAbilityKey): 
            false;

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
        
        if (blockInput)
            weaponHeldDown = block;

        if (finisherInput)
            weaponHeldDown = finisher;

        if (!inCombatStance)
        {
            bool tryingToUseWeapon = 
                weaponHeldDown != null &&
                weaponHeldDown == melee;
            if (!inCombatTransition && 
                (tryingToUseWeapon || GameInfo.Manager.InCombat))
            {
                PlayerInfo.AnimationManager.TryToCombatStance(OnCombatStanceOn);
                inCombatTransition = true;
            }
        }
        else
        {
            if (melee != null)
                melee.UpdateAbility(melee == weaponHeldDown, meleeInput);
            
            if (finisher != null)
                finisher.UpdateAbility(finisher == weaponHeldDown, finisherInput);
            
            if (currentAbility == null && !GameInfo.Manager.InCombat)
            {
                notUsedTimer += Time.deltaTime;
                if (notUsedTimer > notUsedDuration)
                {
                    inCombatStance = false;
                    inCombatTransition = true;
                    PlayerInfo.AnimationManager.TryAwayCombatStance(OnCombatStanceOff);
                }
            }
            else
            {
                notUsedTimer = 0;
            }
        }

        if (ranged != null)
            ranged.UpdateAbility(ranged == weaponHeldDown, rangedInput);

        if (aoe != null)
            aoe.UpdateAbility(aoe == weaponHeldDown, aoeInput);

        if (dash != null)
            dash.UpdateAbility(dash == weaponHeldDown, dashInput);

        if (dodge != null)
            dodge.UpdateAbility(dodge == weaponHeldDown, dodgeInput);

        if (block != null)
            block.UpdateAbility(block == weaponHeldDown, blockInput);
	}

    public void OnCombatStanceOn()
    {
        inCombatStance = true;
        inCombatTransition = false;
        notUsedTimer = 0;
    }

    public void OnCombatStanceOff()
    {
        inCombatTransition = false;
    }

    public override bool Ready()
    {
        return 
            Physics.TouchingFloor &&
            !PlayerInfo.Animator.GetBool("jump") &&
            !PlayerInfo.Animator.GetBool("falling") &&
            PlayerInfo.AnimationManager.Interuptable;
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
        EquipAbility<PlayerFinisher>(ref finisher);
        #if DevMode
        EquipAbility<PlayerDash>(ref dash);
        EquipAbility<PlayerFireball>(ref ranged);
        EquipAbility<PlayerFireChargeTier1>(ref aoe);
        EquipAbility<PlayerBlock>(ref block);
        #endif
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
        if (block != null)
            block.UpdateStaminaCostIcons();    
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

        if (block != null && !block.Continous)
            block.ResetCooldown();

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

    public void MoveDuringAbility(float walkSpeedPercentage)
    {
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = forwardDirection + sidewaysDirection;

        //Direction and speed targets
        if (GameInfo.Settings.LeftDirectionalInput.magnitude <= 0.5f)
        {
            PlayerInfo.MovementManager.LockDirection();
            PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        }
        else
        {
            Vector3 targetRotation = Matho.StandardProjection3D(GameInfo.CameraController.Direction).normalized;
            Vector3 currentRotation = Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;
            Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
            Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
            PlayerInfo.Player.transform.rotation = rotation;

            PlayerInfo.MovementManager.TargetDirection = movementDirection;

            float forwardsAngle = Matho.AngleBetween(Matho.StandardProjection2D(targetRotation), movementDirection);
            float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
        
            PlayerInfo.MovementManager.TargetPercentileSpeed =
                GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier;
        }

        PlayerInfo.MovementSystem.Move(
            PlayerInfo.MovementManager.CurrentDirection,
            PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed * walkSpeedPercentage);

        PlayerInfo.Animator.SetFloat("speed", PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.MovespeedMultiplier.Value);
    }
}