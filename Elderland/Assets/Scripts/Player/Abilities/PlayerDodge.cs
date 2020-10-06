using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDodge : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 6f;

    private AbilitySegment act;
    private AbilityProcess actProcess;

    private float swordSpeedModifier;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip actClip = Resources.Load<AnimationClip>("Player/Abilities/Dodge/DodgeAct");

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(actClip, actProcess);
        act.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        coolDownDuration = .2f;
    }

    protected override bool WaitCondition()
    {
        return GameInfo.Settings.LeftDirectionalInput.magnitude >= 0.25f &&
               PlayerInfo.Sensor.Interaction == null;
    }

    protected override void GlobalStart()
    {
        if (!((PlayerSword) PlayerInfo.AbilityManager.Melee).IsAbilitySpeedReset)
        {
            swordSpeedModifier = (0.25f + PlayerInfo.AbilityManager.Melee.AbilitySpeed);
            abilitySpeed = 1 / swordSpeedModifier;
        }
        else
        {
            swordSpeedModifier = 1;
            abilitySpeed = 1;
        }
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
    }

    private void DuringAct()
    {
        if (system.Physics.TouchingFloor)
        {
            actVelocity = system.Movement.Move(direction, speed * swordSpeedModifier, false);
        }
        else
        {
            system.Physics.AnimationVelocity += system.Movement.ExitVelocity;
            actVelocity = system.Movement.ExitVelocity;
        }  
    }

    private void ActEnd()
    {  
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
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