using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class RangedEnemyDash : EnemyAbility 
{
    //Fields
    [SerializeField]
    private AnimationClip dashClip;

    private Vector2 direction;
    private int transformDirection;
    private const float speed = 3f;

    private AbilitySegment dash;
    private AbilityProcess dashProcess;

    private System.Random random;
    private RangedEnemyManager manager;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        this.system = abilityManager;

        dashProcess = new AbilityProcess(DashBegin, DuringDash, DashFinish, 1);
        dash = new AbilitySegment(dashClip, dashProcess);
        dash.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(dash);
        segments.NormalizeSegments();

        AttackDistance = 7;
        AttackDistanceMargin = 2;
        AttackAngleMargin = float.PositiveInfinity;

        random = new System.Random();

        manager = ((RangedEnemyManager) abilityManager.Manager);
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    private void DashBegin()
    {
        transformDirection = (random.Next(2) == 0) ? -1 : 1;

        //system.Physics.GravityStrength = 0;
        //system.Movement.ExitEnabled = false;
    }

    private void DuringDash()
    {
        /*
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        direction = Matho.StandardProjection2D(transformDirection * transform.right);

        if (system.Physics.TouchingFloor)
        {
            actVelocity = system.Movement.Move(direction, speed);
        }
        else
        {
            system.Physics.AnimationVelocity += system.Movement.ExitVelocity;
            actVelocity = system.Movement.ExitVelocity;
        } 
        */ 
    }

    private void DashFinish()
    {  
        //system.Physics.GravityStrength = PhysicsSystem.GravitationalConstant;
        //system.Movement.ExitEnabled = true;
        DefensiveCheck();
    }

    public override bool OnHit(GameObject character)
    {
        ForceAdvanceSegment();
        return true;
    }

    private void DefensiveCheck()
    {
        if (manager.IsInDefensiveRange())
        {
            DefensiveExit();
        }
    }

    private void DefensiveExit()
    {
        manager.AbilityManager.CancelQueue();

        manager.NextAttack = manager.Slow;
        manager.Slow.Queue(EnemyAbilityType.First);
        manager.Slow.Queue(EnemyAbilityType.Middle);
        manager.Slow.Queue(EnemyAbilityType.Last);
        manager.Animator.SetBool("defensive", true);
        manager.Animator.ResetTrigger("runAbility");
    }

    public override void ShortCircuitLogic()
    {
        DashFinish();
    }
}