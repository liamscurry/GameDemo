using System;
using UnityEngine;

//Manages adding/removing abilities(equiping) and updates each ability every frame. When weapon abilities are active (primary and secondary abilities) other abilities and skills can override them.

public class PlayerAbilityManager : AbilitySystem
{
    //Abilities Slots
    private PlayerAbility melee;
    private PlayerAbility dodge;
    private PlayerAbility ranged;
    private PlayerAbility heal;

    //Ability Properties
    public PlayerAbility Melee { get { return melee; } }
    public PlayerAbility Dodge { get { return dodge; } }
    public PlayerAbility Ranged { get { return ranged; } }
    public PlayerAbility Heal { get { return heal; } }
    public new PlayerAbility CurrentAbility { get { return (PlayerAbility) currentAbility; } set { currentAbility = value; } }

    public bool MeleeAvailable { get; set; }
    public bool DodgeAvailable { get; set; }
    public bool RangedAvailable { get; set; }
    public bool HealAvailable { get; set; }

    public const float MaxStamina = 4;
    public float Stamina { get; private set; }
    public float SavedStamina { get; set; }

    public PlayerAbilityManager(Animator animator, PhysicsSystem physics, MovementSystem movement, GameObject parent) : base(animator, physics, movement, parent)
    { 
        InitializePreferences();
        RangedAvailable = true;
        HealAvailable = true;
        DodgeAvailable = true;
        //Stamina = 0;
    }

    //Updates each ability slot, called by PlayerManager.
	public override void UpdateAbilities() 
    {
        //Try to run specified ability if held down
        bool rangedInput = (GameInfo.Manager.ReceivingInput && RangedAvailable) ? Input.GetAxis("Right Trigger") != 0 : false;
        bool healInput = (GameInfo.Manager.ReceivingInput && HealAvailable) ? Input.GetKey(KeyCode.Joystick1Button4) : false;
        bool meleeInput = (GameInfo.Manager.ReceivingInput && MeleeAvailable) ? Input.GetKey(GameInfo.Settings.MeleeAbilityKey) : false;
        bool dodgeInput = (GameInfo.Manager.ReceivingInput && DodgeAvailable) ? Input.GetKey(GameInfo.Settings.DodgeAbilityKey) : false;

        //Weapon prioritization
        PlayerAbility weaponHeldDown = null;
        
        if (rangedInput)
            weaponHeldDown = ranged;
        
        if (healInput)
            weaponHeldDown = heal;

        if (meleeInput)
            weaponHeldDown = melee;

        if (dodgeInput)
            weaponHeldDown = dodge;
        
        if (ranged != null)
            ranged.UpdateAbility(ranged == weaponHeldDown, rangedInput);

        if (heal != null)
            heal.UpdateAbility(heal == weaponHeldDown, healInput);

        if (melee != null)
            melee.UpdateAbility(melee == weaponHeldDown, meleeInput);

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
    }

    //Clear an ability slot.
    public void UnequipAbility<T>(ref PlayerAbility slot) where T : PlayerAbility
    {
        PlayerAbility temp = slot;
        GameObject.Destroy(temp);
        slot = null;
    }

    //Initializes equiped abilities. Implementation is temporary, will load from file.
    private void InitializePreferences()
    {  
        EquipAbility<PlayerSword>(ref melee);
        EquipAbility<PlayerDodge>(ref dodge);
        EquipAbility<PlayerFireballTier3>(ref ranged);
        EquipAbility<PlayerFireChargeTier3>(ref heal);
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