using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDash : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 30f;

    private AbilitySegment act;
    private AbilityProcess actProcess;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip dashClip = Resources.Load<AnimationClip>("Player/Abilities/Dash/Dash");

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(dashClip, actProcess);
        act.Type = AbilitySegmentType.Physics;
        act.LoopFactor = 4;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        coolDownDuration = .2f;
    }

    private void ActBegin()
    {
        direction = PlayerInfo.MovementManager.CurrentDirection;
        PlayerInfo.MovementManager.LockDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
    }

    private void DuringAct()
    {
        if (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.25f)
        {
            direction = Matho.RotateTowards(direction, GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput.normalized), 150 * Time.deltaTime);
        }
        
        if (system.Physics.TouchingFloor)
        {
            actVelocity = system.Movement.Move(direction, speed);
        }
        else
        {
            system.Physics.AnimationVelocity += system.Movement.ExitVelocity;
            actVelocity = system.Movement.ExitVelocity;
        }  
        
        Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
        system.Parent.transform.rotation = rotation;
    }

    private void ActEnd()
    {  
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.TargetPercentileSpeed = 1;
        PlayerInfo.MovementManager.SnapSpeed();
        system.Physics.GravityStrength = PhysicsSystem.GravitationalConstant;
        system.Movement.ExitEnabled = true;
    }

    public override bool OnHit(GameObject character)
    {
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}