using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class HeavyEnemyVerticalSword : EnemyAbility 
{
    [SerializeField]
    private EnemyDamageHitbox hitbox;
    [SerializeField]
    private BoxCollider hitboxTrigger;

    [SerializeField]
    private AnimationClip leftRotate;
    [SerializeField]
    private AnimationClip leftPause;
    [SerializeField]
    private AnimationClip leftAttack;

    [SerializeField]
    private AnimationClip centerRotate;
    [SerializeField]
    private AnimationClip centerPause;
    [SerializeField]
    private AnimationClip centerAttack;

    [SerializeField]
    private AnimationClip rightRotate;
    [SerializeField]
    private AnimationClip rightPause;
    [SerializeField]
    private AnimationClip rightAttack;

    private AbilityProcess rotateProcess;
    private AbilityProcess pauseProcess;
    private AbilityProcess attackProcess;
    private AbilitySegment rotate;
    private AbilitySegment pause;
    private AbilitySegment attack;

    private System.Random random;

    private const float damage = 2f;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        //Segment setup
        rotateProcess = new AbilityProcess(null, DuringRotate, null, 1);
        pauseProcess = new AbilityProcess(null, null, null, 1);
        attackProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.25f);
        rotate = new AbilitySegment(null, rotateProcess);
        pause = new AbilitySegment(null, pauseProcess);
        attack = new AbilitySegment(null, attackProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(rotate);
        segments.AddSegment(pause);
        segments.AddSegment(attack);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;

        AttackDistance = 1.75f;
        AttackDistanceMargin = 0.5f;
        AttackAngleMargin = 15;

        random = new System.Random();
    }

    protected override void GlobalStart()
    {
        int swingType = (random.Next() % 3) - 1;

        switch (swingType)
        {
            case -1:
                PrimeLeft();
                break; 
            case 0:
                PrimeCenter();
                break; 
            case 1:
                PrimeRight();
                break; 
        }
    }

    private void PrimeLeft()
    {
        rotate.Clip = leftRotate;
        pause.Clip = leftPause;
        attack.Clip = leftAttack;

        Vector3 localPosition = hitbox.transform.localPosition;
        localPosition.x = 0.75f;
        localPosition.z = 0.75f;
        hitbox.transform.localPosition = localPosition;

        Vector3 localScale = hitbox.transform.localScale;
        localScale.x = 3.5f;
        localScale.z = 2.5f;
        hitbox.transform.localScale = localScale;
    }

    private void PrimeCenter()
    {   
        rotate.Clip = centerRotate;
        pause.Clip = centerPause;
        attack.Clip = centerAttack;

        Vector3 localPosition = hitbox.transform.localPosition;
        localPosition.x = 0f;
        localPosition.z = 1.125f;
        hitbox.transform.localPosition = localPosition;

        Vector3 localScale = hitbox.transform.localScale;
        localScale.x = 2f;
        localScale.z = 3.25f;
        hitbox.transform.localScale = localScale;
    }

    private void PrimeRight()
    {
        rotate.Clip = rightRotate;
        pause.Clip = rightPause;
        attack.Clip = rightAttack;

        Vector3 localPosition = hitbox.transform.localPosition;
        localPosition.x = -0.75f;
        localPosition.z = 0.75f;
        hitbox.transform.localPosition = localPosition;

        Vector3 localScale = hitbox.transform.localScale;
        localScale.x = 3.5f;
        localScale.z = 2.5f;
        hitbox.transform.localScale = localScale;
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    private void DuringRotate()
    {
        if (type == EnemyAbilityType.None || type == EnemyAbilityType.First)
        {
            Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
            Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 6f * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }

    private void ActBegin()
    {
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, ((EnemyAbilityManager) system).Manager.GetGroundNormal());
        hitbox.transform.rotation = normalRotation * transform.rotation;
    }

    private void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
    }

    public override bool OnHit(GameObject character)
    {
        character.GetComponentInParent<PlayerManager>().ChangeHealth(-damage);
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}