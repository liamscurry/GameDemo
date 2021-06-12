using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDodge : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 2.5f;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private AbilityProcess slideProcess;

    private float swordSpeedModifier;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 0.82f);
        slideProcess = new AbilityProcess(null, DuringSlide, SlideEnd, 1 - 0.82f);
        act = new AbilitySegment(null, actProcess, slideProcess);
        act.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        coolDownDuration = 0.5f;
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
            swordSpeedModifier = PlayerInfo.AbilityManager.Melee.AbilitySpeed;
            abilitySpeed = 1 / swordSpeedModifier;
        }
        else
        {
            swordSpeedModifier = 1;
            abilitySpeed = 1;
        }

        abilitySpeed *= 0.75f;

        /*
        if (PlayerInfo.StatsManager.AttackSpeedMultiplier.ModifierCount != 0)
        {
            abilitySpeed = 1 / (PlayerInfo.StatsManager.AttackSpeedMultiplier.Value);
            Debug.Log(PlayerInfo.StatsManager.AttackSpeedMultiplier.Value);
        }*/

        Vector2 playerInput =
            PlayerInfo.MovementManager.DirectionToPlayerCoord(GameInfo.Settings.LeftDirectionalInput);
        AnimationClip actClip =
            GetDirAnim(ResourceConstants.Player.Art.Dodge, playerInput);

        act.Clip = actClip;
    }

    public override void GlobalUpdate()
    {
        PlayerInfo.AnimationManager.UpdateRotation(true);
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
        PlayerInfo.AnimationManager.Interuptable = false;

        GameInfo.CameraController.Speed = 0.4f;
        GameInfo.CameraController.TargetSpeed = 0;
        GameInfo.CameraController.SpeedGradation = 0.08f;
    }

    private void DuringAct()
    {
        if (system.Physics.TouchingFloor)
        {
            float compositeSpeed = 
                speed * (1 / abilitySpeed) * PlayerInfo.StatsManager.MovespeedMultiplier.Value;
            actVelocity = system.Movement.Move(direction, compositeSpeed, false);
        }
        else
        {
            system.Physics.AnimationVelocity += system.Movement.ExitVelocity;
            actVelocity = system.Movement.ExitVelocity;
        }  
    }

    private void ActEnd()
    {  
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.TargetPercentileSpeed = GameInfo.Settings.LeftDirectionalInput.magnitude;
        PlayerInfo.MovementManager.SnapSpeed();
        system.Physics.GravityStrength = PhysicsSystem.GravitationalConstant;
        system.Movement.ExitEnabled = true;
    }

    private void DuringSlide()
    {
        if (system.Physics.TouchingFloor)
        {
            float compositeSpeed = 
                speed * (1 / abilitySpeed) * PlayerInfo.StatsManager.MovespeedMultiplier.Value * 0.5f;
            actVelocity = system.Movement.Move(direction, compositeSpeed, false);
        } 
    }

    private void SlideEnd()
    {
        PlayerInfo.AnimationManager.Interuptable = true;
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