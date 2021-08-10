//#define UT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ability that does AOE damage and pushes enemies in the area backwards.
public class PlayerKnockbackPush : PlayerAbility
{
    private float damage = 1.0f;
    private float knockbackStrength = 9.5f;
    private PlayerMultiDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(4f, 2, 5);

    private float zeroSpeedOnExit = 0.25f;

    private Vector3 targetDirection;
    private const float rotationSpeed = 15f;

    private AnimationClip chargeClip;
    private AnimationClip actClip;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilitySegment charge;
    private AbilitySegment act;

    private Animator warpPlaneAnimator;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;
        continous = false;
        coolDownDuration = 2f;
        
        //Hitbox initializations
        GameObject hitboxObject =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularMultiHitbox),
                transform.position,
                Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxObject.SetActive(false);

        hitbox = hitboxObject.GetComponent<PlayerMultiDamageHitbox>();
        hitbox.gameObject.transform.localScale = hitboxScale;

        chargeClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.KnockbackPushCharge);
        actClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.KnockbackPushAct);

        chargeProcess = new AbilityProcess(ChargeBegin, DuringCharge, null, 1);
        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1f);
        charge = new AbilitySegment(chargeClip, chargeProcess);
        act = new AbilitySegment(actClip, actProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        segments.AddSegment(act);
        segments.NormalizeSegments();

        InitializeWarpPlane();
    }

    public void ChargeBegin()
    {
        targetDirection = 
            Matho.StdProj3D(GameInfo.CameraController.transform.forward).normalized;
        if (targetDirection.magnitude == 0) 
            targetDirection = PlayerInfo.Player.transform.forward;

        //GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, 1, 1f, 2f));

        warpPlaneAnimator.gameObject.transform.position =
            PlayerInfo.Player.transform.position +
            targetDirection * 1.5f +
            Matho.Rotate(targetDirection, Vector3.up, 90f) * -1;
        warpPlaneAnimator.gameObject.transform.rotation =
            Quaternion.LookRotation(targetDirection, Vector3.up);
            
        warpPlaneAnimator.Play(ResourceConstants.Player.Art.KnockbackPushWarpAnimation);
    }

    public void DuringCharge()
    {
        Vector3 incrementedRotation = Vector3.zero;
        incrementedRotation =
            Vector3.RotateTowards(
                PlayerInfo.Player.transform.forward,
                targetDirection,
                rotationSpeed * Time.deltaTime,
                Mathf.Infinity);
        PlayerInfo.Player.transform.rotation =
            Quaternion.LookRotation(incrementedRotation, Vector3.up);
    }

    public void ActBegin()
    {
        Quaternion horizontalRotation;
        Quaternion normalRotation;
        PlayerInfo.AbilityManager.GenerateHitboxRotations(
            out horizontalRotation,
            out normalRotation);
            
        hitbox.transform.rotation = normalRotation * horizontalRotation;
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this, PlayerMultiDamageHitbox.ObstructionType.PlayerOrigin);

        GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -4, 1f, 0.4f));
    }

    public void DuringAct()
    {
        hitbox.gameObject.transform.position =
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 2.5f;
    }

    public void ActEnd()
    {
        GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0, 0, 1));
        if (GameInfo.Settings.LeftDirectionalInput.magnitude < zeroSpeedOnExit)
            PlayerInfo.MovementManager.ZeroSpeed();
    }

    public override bool OnHit(GameObject character)
    {
        #if UT
        Debug.Log("Hit Enemy: " + character.name);
        #endif

        #if !UT
        EnemyManager enemy = character.GetComponent<EnemyManager>();
        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value);
        enemy.Push(PlayerInfo.Player.transform.forward * knockbackStrength);

        if (enemy.Health > enemy.ZeroHealth)
        {
            enemy.TryFlinch();
        }
        #endif

        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }

    private void InitializeWarpPlane()
    {
        string knockbackWarpPath = ResourceConstants.Player.Abilities.KnockbackPushWarpObject;

        GameObject warpObject =
            GameObject.Instantiate(
                Resources.Load<GameObject>(knockbackWarpPath),
                PlayerInfo.MeleeObjects.transform);
        warpPlaneAnimator = warpObject.GetComponent<Animator>();
        warpPlaneAnimator.gameObject.SetActive(true);
    }

    public override void DeleteResources()
    {
        GameObject.Destroy(warpPlaneAnimator.gameObject);
        //DeleteAbilityIcon();
        //GameObject.Destroy(dashParticles);
    }
}
