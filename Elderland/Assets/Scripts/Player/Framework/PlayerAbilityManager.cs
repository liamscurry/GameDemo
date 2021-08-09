#define DevMode

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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
    public bool InCombatStance { get { return inCombatStance; } }
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
    public bool AOEAvailable { get; set; }
    public bool AbilitiesAvailable { get; set; }

    public const float MaxStamina = 4;
    public float Stamina { get; private set; }
    public float SavedStamina { get; set; }

    public const float DirFocusDuration = 4f;
    public float LastDirFocus { get; set; }
    public Vector3 DirFocus { get; set; }
    public bool MovementDirFocus { get { return Time.time - LastDirFocus <= DirFocusDuration; } }

    public GameObject HoldBar { get; private set; }

    private ParticleSystem swordParticles;
    public ParticleSystem SwordParticles { get { return swordParticles; } }

    public const float deadzone = 0.2f; // deadzone of analog stick inputs.

    public PlayerAbilityManager(
        Animator animator,
        PhysicsSystem physics,
        MovementSystem movement,
        CharacterMovementSystem chaMoveSystem,
        GameObject parent,
        Transform cooldownOriginTransform,
        float cooldownHeightDelta) : 
        base(animator, PlayerInfo.Controller, physics, movement, chaMoveSystem, parent)
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
        AOEAvailable = true;
        DodgeAvailable = true;
        DashAvailable = true;
        BlockAvailable = true;
        AbilitiesAvailable = true;
        #endif
        
        AbilitiesAvailable = true;
        inCombatStance = false;
        //Stamina = 0;

        LastDirFocus = -DirFocusDuration * 2f;
        GenerateSwordParticles();
    }
    
    private void GenerateSwordParticles()
    {
        GameObject hitboxParticlesObject =
            GameObject.Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.SwordParticles),
                PlayerInfo.Player.transform.position,
                Quaternion.identity);
        hitboxParticlesObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        swordParticles = hitboxParticlesObject.GetComponent<ParticleSystem>();
    }

    /*
    * Need a separate method aligning the sword particles as the player may have match targeted.
    */
    public void AlignSwordParticles(
        Quaternion normalRotation,
        Quaternion horizontalRotation,
        Quaternion tiltRotation)
    {
        PlayerInfo.AbilityManager.SwordParticles.transform.position =
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 0.5f +
            PlayerInfo.Player.transform.up * 0.125f;
        PlayerInfo.AbilityManager.SwordParticles.transform.rotation =
             normalRotation * horizontalRotation * tiltRotation;
        PlayerInfo.AbilityManager.SwordParticles.transform.localScale =
            Vector3.one;
    }

    public void GenerateHitboxRotations(
        out Quaternion horizontalRotation,
        out Quaternion normalRotation)
    {
        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);
        if (PlayerInfo.CharMoveSystem.Grounded)
        {
            normalRotation = Quaternion.FromToRotation(Vector3.up, PlayerInfo.CharMoveSystem.GroundNormal);
        }
        else
        {
            normalRotation = Quaternion.identity;
        }
    }

    //Updates each ability slot, called by PlayerManager.
	public override void UpdateAbilities() 
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShortCircuit(true);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Time.timeScale = 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Time.timeScale = 1f;
        }

        //Try to run specified ability if held down
        bool rangedInput = false;
        bool aoeInput = false;
        bool meleeInput = false;
        bool dashInput = false;
        bool dodgeInput = false;
        bool finisherInput = false;

        GameInput inputType = 
            GameInfo.Manager.ReceivingInput.Value;
        //bool inputAvailable =
        //    (inputType == GameInput.Full || (inputType == GameInput.Gameplay && currentAbility != null));
        //||
        //    (inputType == GameInput.Gameplay && GameInfo.Manager.ReceivingInput.Tracker == currentAbility)

        if (AbilitiesAvailable)
        {
            rangedInput = 
                (RangedAvailable) ?
                Mathf.Abs(GameInfo.Settings.FireballRightTrigger) > GameInfo.Settings.FireballTriggerOnThreshold : false;
            aoeInput =
                (AOEAvailable) ?
                GameInfo.Settings.CurrentGamepad[GameInfo.Settings.AOEAbilityKey].isPressed : false;
            meleeInput =
                (MeleeAvailable) ? 
                GameInfo.Settings.CurrentGamepad[GameInfo.Settings.MeleeAbilityKey].isPressed : false;
            dashInput =
                (DashAvailable) ?
                GameInfo.Settings.CurrentGamepad[GameInfo.Settings.DashAbilityKey].isPressed : false;
            dodgeInput =
                (DodgeAvailable) ?
                GameInfo.Settings.CurrentGamepad[GameInfo.Settings.DodgeAbilityKey].isPressed : false;
            //bool blockInput = 
            //    (BlockAvailable) ? 
            //    Input.GetKeyDown(GameInfo.Settings.BlockAbilityKey) : false;
            finisherInput = 
                (AbilitiesAvailable) ? 
                GameInfo.Settings.CurrentGamepad[GameInfo.Settings.FinisherAbilityKey].isPressed: 
                false;
        }

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
        
        //if (blockInput)
        //    weaponHeldDown = block;

        if (finisherInput)
            weaponHeldDown = finisher;

        UpdateRangedZoom();

        UpdateCombatStance(weaponHeldDown, meleeInput, finisherInput);

        if (ranged != null)
            ranged.UpdateAbility(ranged == weaponHeldDown, rangedInput);

        if (aoe != null)
            aoe.UpdateAbility(aoe == weaponHeldDown, aoeInput);

        if (dash != null)
            dash.UpdateAbility(dash == weaponHeldDown, dashInput);

        if (dodge != null)
            dodge.UpdateAbility(dodge == weaponHeldDown, dodgeInput);

        //if (block != null)
        //    block.UpdateAbility(block == weaponHeldDown, blockInput);
	}

    private void UpdateCombatStance(
        PlayerAbility weaponHeldDown,
        bool meleeInput,
        bool finisherInput)
    {
        if (!inCombatStance)
        {
            bool tryingToUseWeapon = 
                weaponHeldDown != null &&
                weaponHeldDown == melee;
            if ((tryingToUseWeapon || GameInfo.Manager.InCombat) && 
                GameInfo.Manager.ReceivingInput.Value == GameInput.Full)
            {
                PlayerInfo.AnimationManager.ToCombatStance();
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
                //Debug.Log("waiting to remove: " + notUsedTimer + ", " + notUsedDuration + ", " + GameInfo.Manager.ReceivingInput.Value);
                notUsedTimer += Time.deltaTime;
                if (notUsedTimer > notUsedDuration &&
                    GameInfo.Manager.ReceivingInput.Value == GameInput.Full)
                {
                    inCombatStance = false;
                    PlayerInfo.AnimationManager.AwayCombatStance();
                }
            }
            else
            {
                notUsedTimer = 0;
            }
        }
    }

    private void UpdateRangedZoom()
    {
        if (Mathf.Abs(GameInfo.Settings.FireballLeftTrigger) > GameInfo.Settings.FireballTriggerOnThreshold &&
            GameInfo.Manager.ReceivingInput.Value != GameInput.None)
        {
            GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -10, 0.6f));
        }
        else
        {
            GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0, 0));
        }
    }

    public void OnCombatStanceOn()
    {
        inCombatStance = true;
        notUsedTimer = 0;
        GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.AnimationManager, GameInput.Full);
        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(PlayerInfo.AnimationManager, false);
    }

    public void ShortCircuitCombatStanceOn()
    {
        PlayerInfo.AnimConnector.GraspMelee();
        OnCombatStanceOn();
        Debug.Log("called");
    }

    public void OnCombatStanceOff()
    {
        inCombatStance = false;
        GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.AnimationManager, GameInput.Full);
        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(PlayerInfo.AnimationManager, false);
    }

    public void ShortCircuitCombatStanceOff()
    {
        PlayerInfo.AnimConnector.FadeOutMelee();
        PlayerInfo.AnimConnector.DisableMelee();
        OnCombatStanceOff();
    }

    public override bool Ready()
    {
        return PlayerInfo.CharMoveSystem.Grounded;
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
            temp.ShortCircuit();
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
        EquipAbility<PlayerKnockbackPush>(ref aoe);
        //EquipAbility<PlayerBlock>(ref block);
        #endif
    }

    private void UpdateCooldownIconPositions()
    {
        var cooldownIconOrder = new List<PlayerAbility>();
        //cooldownIconOrder.Add(aoe);
        cooldownIconOrder.Add(dash);
        cooldownIconOrder.Add(ranged);

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
}