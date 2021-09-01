using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Animations;

//Manages the player's subpart update order.

public class PlayerManager : MonoBehaviour, ICharacterManager
{
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private RectTransform healthSlidersParent;
    [SerializeField]
    private float healthSlidersSpacingPercentage;
    [SerializeField]
    private VitalityMenuButton healthUpgradeButton;
    [SerializeField]
    private Slider[] healthSliders;
    [SerializeField]
    private Slider staminaSlider1;
    [SerializeField]
    private Slider staminaSlider2;
    [SerializeField]
    private Slider staminaSlider3;
    [SerializeField]
    private Slider staminaSlider4;
    [SerializeField]
    private GameObject glitchRenderersParent;
    [SerializeField]
    private AbilityMenuButton fireballTierOneButton;
    [SerializeField]
    private AbilityMenuButton firechargeTierOneButton;
    [SerializeField]
    private AbilityMenuButton dashTierOneButton;

    // Fields
    private SkinnedMeshRenderer[] glitchRenderers;
        
    private const float healFresnelDuration = 0.5f;
    private IEnumerator healFresnelEnumerator;

    // Properties
    public SkinnedMeshRenderer[] GlitchRenderers { get { return glitchRenderers; } }

    public Slider StaminaSlider1 { get { return staminaSlider1; } }
    public Slider StaminaSlider2 { get { return staminaSlider2; } }
    public Slider StaminaSlider3 { get { return staminaSlider3; } }
    public Slider StaminaSlider4 { get { return staminaSlider4; } }

    public float Health { get; protected set; }
    public float MaxHealth { get; protected set; }
    public float ZeroHealth { get { return 0.1f; } }

    private float savedHealth;

    public StandardInteraction Interaction { get; set; }

    public event EventHandler OnBreak;
    public event EventHandler OnBlock;

    private void Start()
    {
        healthUpgradeButton.Initialize();
        UnlockSprint();
        glitchRenderers =
            glitchRenderersParent.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

	private void Update()
    {
        //Systems//
        //PlayerInfo.MovementSystem.UpdateSystem();

        //Managers//
        //Data based
        PlayerInfo.BuffManager.UpdateBuffs();
        
        //Input based
        PlayerInfo.AnimationManager.UpdateAnimations();
        PlayerInfo.AbilityManager.UpdateAbilities();
        PlayerInfo.MovementManager.UpdateMovement();
        PlayerInfo.CharMoveSystem.UpdateSystem();
    }

    private void LateUpdate()
    {
        //Frame specific data
        //PlayerInfo.MovementSystem.LateUpdateSystem();
        PlayerInfo.MovementManager.LateUpdateMovement();
        PlayerInfo.CharMoveSystem.LateUpdateSystem();
        GameInfo.CameraController.UpdateController();
        PlayerInfo.AnimationManager.LateUpdateAnimations();
    }

    private void FixedUpdate()
    {
        //PlayerInfo.MovementSystem.FixedUpdateSystem();
        PlayerInfo.AbilityManager.FixedUpdateAbilities();
    }

    public void ForceClamp()
    {
        PlayerInfo.PhysicsSystem.ForceTouchingFloor();
    }

    public void OnDeath()
    {
        GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
        PlayerInfo.BuffManager.ClearBuffs();
        PlayerInfo.BuffManager.ClearDebuffs();
        PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
    }

    public void Respawn()
    {
        GameInfo.Manager.ReceivingInput.TryReleaseLock(this, GameInput.Full);
        PlayerInfo.AnimationManager.IgnoreFallingAnimation = false;
    }

    //Collision
    private void OnCollisionEnter(Collision other)
    {
        VelocityCollision(other);
        OverlapCollision(other);
        GroundCollision(other);
    }

    private void OnCollisionStay(Collision other)
    {
        VelocityCollision(other);
        OverlapCollision(other);
    }

    private void OnCollisionExit(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleOverlapCollisions(PlayerInfo.PhysicsSystem, PlayerInfo.Capsule, transform.position, null);
    }

    //Handle dynamic velocity alteration when player gets pushed or moved into ground collision.
    private void VelocityCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleVelocityCollisions(PlayerInfo.PhysicsSystem, PlayerInfo.Capsule, transform.position, other);
    }

    private void GroundCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleGroundCollisions(PlayerInfo.PhysicsSystem, PlayerInfo.Capsule, transform.position, other);
    }

    private void OverlapCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleOverlapCollisions(PlayerInfo.PhysicsSystem, PlayerInfo.Capsule, transform.position, other);
    }

    public void ChangeHealth(float value, bool unblockable = false, bool shakeCamera = true)
    {
        //|| (!PlayerInfo.StatsManager.Blocking || unblockable))
        if (value >= 0)
        {
            ChangeHealthBar(value);
        }
        else if (!PlayerInfo.StatsManager.Invulnerable.Value)
        {
            float preHealth = Health;
        
            if (PlayerInfo.StatsManager.Blocking)
            {
                if (unblockable)
                {
                    if (OnBreak != null)    
                        OnBreak.Invoke(this, EventArgs.Empty);
                    ChangeHealthBar(value);
                }
                else
                {
                    if (OnBlock != null)    
                        OnBlock.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                ChangeHealthBar(value);
            }

            if (shakeCamera)
                GameInfo.CameraController.ShakeCamera();

            if (preHealth != 0 && Health == 0)
            {
                OnDeath();
                GameInfo.Manager.Respawn();
            }
        }
    }

    private void ChangeHealthBar(float value)
    {
        Health = Mathf.Clamp(Health + value, 0, MaxHealth);

        float percentage = Health / MaxHealth;

        int currentTier = PlayerInfo.StatsManager.HealthTier;
        int maxTier = PlayerInfo.StatsManager.HealthTierMax;

        float leftOverPercentage = percentage;
        float sliderPercentage = 1f / (3f + currentTier);
        for (int i = 0; i < 3 + currentTier; i++)
        {
            if (leftOverPercentage > 0)
            {
                healthSliders[i].value = Mathf.Clamp01(leftOverPercentage / sliderPercentage);
                leftOverPercentage -= sliderPercentage;
            }
            else
            {
                healthSliders[i].value = 0;
            }
        }
    }

    public void HealFresnel()
    {
        if (healFresnelEnumerator != null)
        {
            StopCoroutine(healFresnelEnumerator);
        }

        healFresnelEnumerator = HealFresnelCoroutine();
        StartCoroutine(healFresnelEnumerator);
    }    

    private IEnumerator HealFresnelCoroutine()
    {
        float timer = 0;
        while (timer < healFresnelDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            float fresnelStrength = 
                Mathf.Sin((Mathf.PI) * timer / healFresnelDuration);
            foreach (var renderer in GlitchRenderers)
            {
                renderer.material.SetFloat("_FresnelStrength", fresnelStrength);
            }
        }
        
        healFresnelEnumerator = null;
    }

    public void ChangeStamina(float f)
    {
        PlayerInfo.AbilityManager.ChangeStamina(f);
    }

    public void InitializeHealth(int maxTier)
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
        PlayerInfo.StatsManager.HealthTier = 0;
        PlayerInfo.StatsManager.HealthTierMax = maxTier;
        MaxOutHealth();

        int currentTier = 0;

        for (int i = 0; i < 3 + currentTier; i++)
        {
            healthSliders[i].value = 1;
            healthSliders[i].gameObject.SetActive(true);
        }

        // Need to turn off not in use sliders
        for (int i = 0; i < maxTier - currentTier; i++)
        {
            healthSliders[3 + i + currentTier].gameObject.SetActive(false);
        }

        RescaleHealth();
        ChangeHealth(0);
    }

    public void IncreaseMaxHealth(int tier)
    {
        MaxHealth = MaxHealth / (tier + 2) * (tier + 3);
        PlayerInfo.StatsManager.HealthTier = tier;
        healthSliders[3 + tier - 1].gameObject.SetActive(true);
        RescaleHealth();
        ChangeHealth(0);
    }

    private void RescaleHealth()
    {
        int currentTier = PlayerInfo.StatsManager.HealthTier;
        int maxTier = PlayerInfo.StatsManager.HealthTierMax;

        Rect parentRect = healthSlidersParent.rect;
        float sliderWidth =
            (parentRect.width - healthSlidersSpacingPercentage * parentRect.width) /
            (3 + currentTier);

        float sliderSpacing = (healthSlidersSpacingPercentage * parentRect.width) /
            (3 + currentTier - 1);

        for (int i = 0; i < 3 + currentTier; i++)
        {
            ((RectTransform) healthSliders[i].transform).
                SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sliderWidth);
            ((RectTransform) healthSliders[i].transform).anchoredPosition = 
                new Vector2(-parentRect.width / 2 + sliderWidth / 2 + i * (sliderSpacing + sliderWidth), 0);

            healthSliders[i].value = 1;
            healthSliders[i].gameObject.SetActive(true);
        }
    }

    public void IncreaseStaminaYield(int tier)
    {
        if (tier == 1 || tier == 2)
        {
            PlayerInfo.StatsManager.StaminaYieldMultiplier.AddModifier((tier + 2f) / (tier + 1f));
        }
        else
        {
            PlayerInfo.StatsManager.StaminaYieldMultiplier.AddModifier((tier + 3f) / (tier + 1f));
        }
    }

    private void EquipTier1Fireball()
    {
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireball>(ref PlayerInfo.AbilityManager.ranged);
    }

    private void EquipTier1Firecharge()
    {
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireChargeTier1>(ref PlayerInfo.AbilityManager.aoe);
    }

    private void EquipTier1Dash()
    {
        PlayerInfo.AbilityManager.EquipAbility<PlayerDash>(ref PlayerInfo.AbilityManager.dash);
    }

    public void EquipTier2Fireball()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerFireball>(ref PlayerInfo.AbilityManager.ranged);
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireballTier2>(ref PlayerInfo.AbilityManager.ranged);
    }

    public void EquipTier3Fireball()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerFireballTier2>(ref PlayerInfo.AbilityManager.ranged);
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireballTier3>(ref PlayerInfo.AbilityManager.ranged);
    }

    public void EquipTier2Firecharge()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerFireChargeTier1>(ref PlayerInfo.AbilityManager.aoe);
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireChargeTier2>(ref PlayerInfo.AbilityManager.aoe);
    }

    public void EquipTier3Firecharge()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerFireChargeTier2>(ref PlayerInfo.AbilityManager.aoe);
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerFireChargeTier3>(ref PlayerInfo.AbilityManager.aoe);
    }

    public void EquipTier2Dash()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerDash>(ref PlayerInfo.AbilityManager.dash);
        PlayerInfo.AbilityManager.
            EquipAbility<PlayerDashTier2>(ref PlayerInfo.AbilityManager.dash);
    }

    public void EquipTier3Dash()
    {
        PlayerInfo.AbilityManager.
            UnequipAbility<PlayerDashTier2>(ref PlayerInfo.AbilityManager.dash);
        
        for (int i = PlayerInfo.BuffManager.Buffs.Count - 1;
             i >= 0; i--)
        {
            Buff<PlayerManager> buff = PlayerInfo.BuffManager.Buffs[i];
            if (buff is PlayerDashTier2Buff)
            {
                PlayerInfo.BuffManager.Clear(buff);
                break;
            }
        }

        PlayerInfo.AbilityManager.
            EquipAbility<PlayerDashTier3>(ref PlayerInfo.AbilityManager.dash);
    }

    public void Halt()
    {
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
    }

    public void TeleportToTransform(Transform otherTransform)
    {
        transform.position = otherTransform.position;
        transform.rotation = otherTransform.rotation;
        PlayerInfo.TeleportingThisFrame = true;
    }

    public void WalkToTransform(WalkToTuple tuple)
    {
        PlayerInfo.PhysicsSystem.Animating = true;
        StartCoroutine(WalkToTransformCoroutine(tuple));
    }

    public void MirrorTransform(Transform otherTransform)
    {
        transform.position = otherTransform.position;
        Vector3 rotationAngles = otherTransform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, rotationAngles.y, rotationAngles.z);
        GameInfo.CameraController.SetDirection(otherTransform.rotation);
    }

    public void CallFirewallExplicitly()
    {
        PlayerInfo.AbilityManager.aoe.UpdateAbility(true, true);
    }

    public void CallFireballExplicitly()
    {
        PlayerInfo.AbilityManager.ranged.UpdateAbility(true, true);
    }

    public void UnlockSprint()
    {
        PlayerInfo.MovementManager.UnlockSprint();
    }

    public void UnlockMelee()
    {
        PlayerInfo.AbilityManager.MeleeAvailable = true;
    }

    public void UnlockDodge()
    {
        PlayerInfo.AbilityManager.DodgeAvailable = true;
    }

    public void UnlockDash()
    {
        PlayerInfo.AbilityManager.DashAvailable = true;
        EquipTier1Dash();
        dashTierOneButton.Unlock(false);
        dashTierOneButton.TryAcquire();
    }

    public void UnlockRanged()
    {
        PlayerInfo.AbilityManager.RangedAvailable = true;
        EquipTier1Fireball();
        fireballTierOneButton.Unlock(false);
        fireballTierOneButton.TryAcquire();
    }

    public void UnlockHeal()
    {
        PlayerInfo.AbilityManager.AOEAvailable = true;
        EquipTier1Firecharge();
        firechargeTierOneButton.Unlock(false);
        firechargeTierOneButton.TryAcquire();
    }

    public void LockMelee()
    {
        PlayerInfo.AbilityManager.MeleeAvailable = false;
    }

    public void LockDodge()
    {
        PlayerInfo.AbilityManager.DodgeAvailable = false;
    }

    public void LockRanged()
    {
        PlayerInfo.AbilityManager.RangedAvailable = false;
    }

    public void LockHeal()
    {
        PlayerInfo.AbilityManager.AOEAvailable = false;
    }

    private IEnumerator WalkToTransformCoroutine(WalkToTuple tuple)
    {
        while (true)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = tuple.TargetTransform.position;
            Vector3 incrementedPosition = Vector3.MoveTowards(currentPosition, targetPosition, tuple.Speed * Time.deltaTime);
            transform.position = incrementedPosition;

            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = tuple.TargetTransform.rotation;
            Quaternion incrementedRotation = Quaternion.RotateTowards(currentRotation, targetRotation, tuple.RotationSpeed * Time.deltaTime);
            transform.rotation = incrementedRotation;

            if (Vector3.Distance(currentPosition, targetPosition) < 0.05f)
            {
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void MaxOutHealth()
    {
        ChangeHealth(MaxHealth);
    }

    public void MaxOutStamina()
    {
        PlayerInfo.AbilityManager.ChangeStamina(PlayerAbilityManager.MaxStamina);
    }

    public void ZeroStamina()
    {
        PlayerInfo.AbilityManager.ChangeStamina(-PlayerAbilityManager.MaxStamina);
    }

    public void QuarterStamina()
    {
        PlayerInfo.AbilityManager.ChangeStamina(-PlayerAbilityManager.MaxStamina);
        PlayerInfo.AbilityManager.ChangeStamina(PlayerAbilityManager.MaxStamina / 4f);
    }

    public void SaveStamina()
    {
        PlayerInfo.AbilityManager.SavedStamina = PlayerInfo.AbilityManager.Stamina;
    }

    public void RestoreStamina()
    {
        PlayerInfo.AbilityManager.ChangeStamina(-PlayerAbilityManager.MaxStamina);
        PlayerInfo.AbilityManager.ChangeStamina(PlayerInfo.AbilityManager.SavedStamina);
    }

    public void SaveHealth()
    {
        savedHealth = Health;
    }

    public void RestoreHealth()
    {
        ChangeHealth(-MaxHealth);
        ChangeHealth(savedHealth);
    }

    public void IncreaseAbilityPoints()
    {
        PlayerInfo.StatsManager.UpgradePoints++;
    }

    public void IncreaseAbilityPoints(int amount)
    {
        PlayerInfo.StatsManager.UpgradePoints += amount;
    }

    public void IncreaseVitalityPoints()
    {
        PlayerInfo.StatsManager.VitalityPoints++;
    }

    public void ZeroVitalityPoints()
    {
        PlayerInfo.StatsManager.VitalityPoints = 0;
    }

    public void DisableAbilities()
    {
        PlayerInfo.AbilityManager.AbilitiesAvailable = false;
        GameInfo.CameraController.AllowZoom = false;
    }

    public void EnableAbilities()
    {
        PlayerInfo.AbilityManager.AbilitiesAvailable = true;
        GameInfo.CameraController.AllowZoom = true;
    }

    public void Reset()
    {
        PlayerInfo.AbilityManager.ResetCooldowns();

        PlayerInfo.PhysicsSystem.TotalZero(true, true, true);
        PlayerInfo.PhysicsSystem.ForceTouchingFloor();
        PlayerInfo.Animator.Play("Movement");
        PlayerInfo.Animator.SetFloat("speed", 0.0f);
        PlayerInfo.Animator.SetBool("jump", false);
        PlayerInfo.Animator.ResetTrigger("runAbility");
        PlayerInfo.Animator.ResetTrigger("proceedAbility");
        PlayerInfo.Animator.SetBool("exitAbility", false);
        PlayerInfo.Animator.ResetTrigger("dash");
        PlayerInfo.Animator.ResetTrigger("dodge");
        PlayerInfo.Animator.ResetTrigger("interacting");
        PlayerInfo.Animator.ResetTrigger("climbing");
        PlayerInfo.Animator.ResetTrigger("climbEnterTop");
        PlayerInfo.Animator.ResetTrigger("climbEnterBottom");
        PlayerInfo.Animator.SetFloat("climbSpeedVertical", 0.0f);
        PlayerInfo.Animator.SetFloat("climbSpeedHorizontal", 0.0f);
        PlayerInfo.Animator.ResetTrigger("climbExitTop");
        PlayerInfo.Animator.ResetTrigger("climbExitBottom");
        PlayerInfo.Animator.ResetTrigger("climbExitBottom");
        PlayerInfo.Animator.ResetTrigger("lightSword");
        PlayerInfo.Animator.ResetTrigger("generalInteracting");
        PlayerInfo.Animator.SetBool("targetMatch", false);
        PlayerInfo.Animator.SetBool("falling", false);
        PlayerInfo.Animator.ResetTrigger("mantle");
        PlayerInfo.Animator.ResetTrigger("tallMantle");
        PlayerInfo.Animator.ResetTrigger("shortMantle");
        PlayerInfo.Animator.ResetTrigger("mantleTop");
        PlayerInfo.Animator.ResetTrigger("mantleBottom");
        PlayerInfo.Animator.ResetTrigger("fall");
    }

	public Vector3 currentTargetPosition;
    public Vector3 test;
    public Vector3 test2;
    public Vector3 test3;
    public List<Vector2> temps = new List<Vector2>();
	public void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(currentTargetPosition, Vector3.one * 0.25f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(test, Vector3.one * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(test2, Vector3.one * 0.5f);
        
        Gizmos.color = Color.magenta;
        foreach (Vector2 v in temps)
        {
            Gizmos.DrawCube(new Vector3(v.x, transform.position.y, v.y), Vector3.one * 0.3f);
        }
        //if (temps.Count > 0)
        //    Gizmos.DrawCube(new Vector3(temps[0].x, transform.position.y, temps[0].y), Vector3.one * 1f);
	}
}