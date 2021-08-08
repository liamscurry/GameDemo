using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ability that does AOE damage and pushes enemies in the area backwards.
public class PlayerKnockbackPush : PlayerAbility
{
    private float damage = 1.5f;
    private float knockbackStrength = 7.5f;
    private PlayerMultiDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(4f, 2, 5);

    private Vector3 targetDirection;
    private const float rotationSpeed = 40f;

    private AnimationClip chargeClip;
    private AnimationClip actClip;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilitySegment charge;
    private AbilitySegment act;

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
    }

    public void ChargeBegin()
    {
        targetDirection = 
            Matho.StdProj3D(GameInfo.CameraController.transform.forward).normalized;
        if (targetDirection.magnitude == 0) 
            targetDirection = PlayerInfo.Player.transform.forward;
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
        hitbox.Invoke(this);
    }

    public void DuringAct()
    {
        hitbox.gameObject.transform.position =
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 2.5f;
    }

    public void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
    }

    public override bool OnHit(GameObject character)
    {
        EnemyManager enemy = character.GetComponent<EnemyManager>();
        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value);
        enemy.Push(PlayerInfo.Player.transform.forward * knockbackStrength);

        if (enemy.Health > enemy.ZeroHealth)
        {
            enemy.TryFlinch();
        }

        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }

    public override void DeleteResources()
    {
        //DeleteAbilityIcon();
        //GameObject.Destroy(dashParticles);
    }
}
