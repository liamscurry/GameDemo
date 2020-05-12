using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class HeavyEnemyHorizontalSword : EnemyAbility 
{
    [SerializeField]
    private EnemyDamageHitbox hitbox;
    [SerializeField]
    private BoxCollider hitboxTrigger;

    [SerializeField]
    private AnimationClip rotateClip;
    [SerializeField]
    private AnimationClip pauseClip;
    [SerializeField]
    private AnimationClip attackClip;

    private AbilityProcess rotateProcess;
    private AbilityProcess pauseProcess;
    private AbilityProcess attackProcess;
    private AbilitySegment rotate;
    private AbilitySegment pause;
    private AbilitySegment attack;

    private const float damage = 2f;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        //Segment setup
        rotateProcess = new AbilityProcess(null, DuringRotate, null, 1);
        pauseProcess = new AbilityProcess(null, null, null, 1);
        attackProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.25f);
        rotate = new AbilitySegment(rotateClip, rotateProcess);
        pause = new AbilitySegment(pauseClip, pauseProcess);
        attack = new AbilitySegment(attackClip, attackProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(rotate);
        segments.AddSegment(pause);
        segments.AddSegment(attack);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;

        AttackDistance = 1.75f;
        AttackDistanceMargin = 0.5f;
        AttackAngleMargin = 25;
    }

    private void DuringRotate()
    {
        if (type == EnemyAbilityType.None || type == EnemyAbilityType.First)
        {
            Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
            Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }

    private void ActBegin()
    {
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, system.Physics.Normal);
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