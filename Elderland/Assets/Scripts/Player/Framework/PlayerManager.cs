using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

//Manages the player's subpart update order.

public class PlayerManager : MonoBehaviour 
{
    [SerializeField]
    private float maxHealth;

    [SerializeField]
    private Slider healthSlider1;
    [SerializeField]
    private Slider healthSlider2;
    [SerializeField]
    private Slider healthSlider3;
    [SerializeField]
    private Slider staminaSlider1;
    [SerializeField]
    private Slider staminaSlider2;
    [SerializeField]
    private Slider staminaSlider3;
    [SerializeField]
    private Slider staminaSlider4;

    public Slider StaminaSlider1 { get { return staminaSlider1; } }
    public Slider StaminaSlider2 { get { return staminaSlider2; } }
    public Slider StaminaSlider3 { get { return staminaSlider3; } }
    public Slider StaminaSlider4 { get { return staminaSlider4; } }

    public float Health { get; protected set; }
    public float MaxHealth { get; protected set; }

    private float savedHealth;

    public StandardInteraction Interaction { get; set; }

    private void Start()
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
    }

	private void Update()
    {
        //Systems//
        PlayerInfo.PhysicsSystem.UpdateSystem();
        PlayerInfo.MovementSystem.UpdateSystem();

        //Managers//
        //Data based
        
        //Input based
        PlayerInfo.AnimationManager.UpdateAnimations();
        PlayerInfo.AbilityManager.UpdateAbilities();
        PlayerInfo.MovementManager.UpdateMovement();

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 1;
            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            Time.timeScale = 0.1f;
            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            PlayerInfo.AbilityManager.ChangeStamina(10);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            Time.timeScale = 3f;
            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameInfo.Manager.Respawn();
        }
        #endif
    }

    private void LateUpdate()
    {
        //Frame specific data
        PlayerInfo.MovementSystem.LateUpdateSystem();
        PlayerInfo.PhysicsSystem.LateUpdateSystem();
        PlayerInfo.AnimationManager.LateUpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!PlayerInfo.PhysicsSystem.Animating)
        {
            PlayerInfo.Body.velocity = PlayerInfo.PhysicsSystem.CalculatedVelocity;
        } 
        else
        {
            PlayerInfo.Body.velocity = PlayerInfo.PhysicsSystem.AnimationVelocity;
        }       

        //Friction
        if (PlayerInfo.PhysicsSystem.TouchingFloor)
        {
            PlayerInfo.PhysicsSystem.DynamicDrag(12f);
        }

        PlayerInfo.MovementSystem.FixedUpdateSystem();
        PlayerInfo.PhysicsSystem.FixedUpdateSystem();
        PlayerInfo.AbilityManager.FixedUpdateAbilities();
    }
    
    //To be implemented
    public void OnDeath()
    {
        GameInfo.Manager.FreezeInput(this);
        PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
    }

    public void Respawn()
    {
        GameInfo.Manager.UnfreezeInput(this);
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
    
    public void ChangeHealth(float value)
    {
        if (PlayerInfo.AnimationManager.Interuptable)
        {
            float preHealth = Health;

            Health = Mathf.Clamp(Health + value, 0, MaxHealth);

            float percentage = Health / MaxHealth;

            if (percentage == 0f)
            {
                healthSlider1.value = 0;
                healthSlider2.value = 0;
                healthSlider3.value = 0;
            }
            else if (percentage <= 1 / 3f && percentage > 0f)
            {
                healthSlider1.value = percentage * 3f;
                healthSlider2.value = 0;
                healthSlider3.value = 0;
            }   
            else if (percentage <= 2 / 3f && percentage > 1 / 3f)
            {
                healthSlider1.value = 1;
                healthSlider2.value = (percentage - (1 / 3f)) * 3f;
                healthSlider3.value = 0;
            }
            else if (percentage < 1 && percentage > 2 / 3f)
            {
                healthSlider1.value = 1;
                healthSlider2.value = 1;
                healthSlider3.value = (percentage - (2 / 3f)) * 3f;
            }
            else
            {
                healthSlider1.value = 1;
                healthSlider2.value = 1;
                healthSlider3.value = 1;
            }
            
            if (preHealth != 0 && Health == 0)
            {
                OnDeath();
                GameInfo.Manager.Respawn();
            }
        }
    }

    public void Halt()
    {
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
    }

    public void TeleportToTransform(Transform otherTransform)
    {
        transform.position = otherTransform.position;
        transform.rotation = otherTransform.rotation;
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

    public void UnlockRanged()
    {
        PlayerInfo.AbilityManager.RangedAvailable = true;
    }

    public void UnlockHeal()
    {
        PlayerInfo.AbilityManager.HealAvailable = true;
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
        PlayerInfo.AbilityManager.HealAvailable = false;
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

    public void Reset()
    {
        PlayerInfo.AbilityManager.ShortCircuit(false);
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