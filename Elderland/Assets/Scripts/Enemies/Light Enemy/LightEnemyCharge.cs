using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class LightEnemyCharge : EnemyAbility 
{
    [SerializeField]
    private EnemyDamageHitbox hitbox;
    [SerializeField]
    private BoxCollider hitboxTrigger;
    [SerializeField]
    private AnimationClip actClip;
    [SerializeField]
    private AnimationClip slowClip;

    //Fields
    private float damage = 1;
 
    private Vector3 direction;
    private const float speed = 6.75f;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private AbilitySegment slow;
    private AbilityProcess slowProcess;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        this.system = abilityManager;

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(actClip, actProcess);
        act.Type = AbilitySegmentType.Physics;
        act.LoopFactor = 3;

        slowProcess = new AbilityProcess(null, null, null, 1);
        slow = new AbilitySegment(slowClip, slowProcess);

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.AddSegment(slow);
        segments.NormalizeSegments();

        AttackDistance = 6;
        AttackDistanceMargin = 0.5f;
        AttackAngleMargin = 5;
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    private void ActBegin()
    {
        direction = Matho.StdProj3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        hitbox.Invoke(this);
        hitbox.gameObject.SetActive(true);
    }

    private void DuringAct()
    {
        ((EnemyAbilityManager) system).Manager.Agent.Move(direction * speed * Time.deltaTime);

        Vector3 targetForward = direction;
        Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, ((EnemyAbilityManager) system).Manager.GetGroundNormal());
        hitbox.transform.rotation = normalRotation * transform.rotation;
    }

    private void ActEnd()
    {  
        hitbox.gameObject.SetActive(false);
    }

    public override bool OnHit(GameObject character)
    {
        ForceAdvanceSegment();
        character.GetComponentInParent<PlayerManager>().ChangeHealth(-damage);
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}