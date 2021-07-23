using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDodge : PlayerAbility 
{
    private enum DodgeType { Left = 1, Normal = 0, Right = -1 }

    //Fields
    private Vector2 direction;
    private float speed = 2.5f;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private AbilityProcess slideProcess;

    private Vector3 startPlayerDirection;
    private DodgeType dodgeType;
    private const float sideThreshold = 0.25f;
    private const float sideRotateDegrees = 40f;
    private const float sideRotateSpeed = 5f;

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
        Vector2 playerInput =
            PlayerInfo.MovementManager.DirectionToPlayerCoord(GameInfo.Settings.LeftDirectionalInput);
        playerInput.x *= 0.75f;
        AnimationClip actClip =
            GetDirAnim(ResourceConstants.Player.Art.Dodge, playerInput);

        act.Clip = actClip;

        if (playerInput.x > sideThreshold)
        {
            dodgeType = DodgeType.Right;
        }
        else if (playerInput.x < -sideThreshold)
        {
            dodgeType = DodgeType.Left;
        }
        else
        {
            dodgeType = DodgeType.Normal;
        }

        startPlayerDirection = PlayerInfo.Player.transform.forward;

        PlayerInfo.AbilityManager.LastDirFocus = Time.time;
        PlayerInfo.AbilityManager.DirFocus = PlayerInfo.Player.transform.forward;
    }

    public override void GlobalUpdate()
    {
        //PlayerInfo.MovementManager.UpdateRotation(true);

        if (dodgeType != DodgeType.Normal)
        {
            Vector3 targetPlayerDirection =
            Matho.Rotate(startPlayerDirection, Vector3.up, sideRotateDegrees * (int) dodgeType);
        
            Vector3 incrementedRotation = PlayerInfo.Player.transform.forward;
            incrementedRotation =
                Vector3.RotateTowards(
                    incrementedRotation,
                    targetPlayerDirection,
                    sideRotateSpeed * Time.deltaTime,
                    Mathf.Infinity);
            PlayerInfo.Player.transform.rotation =
                Quaternion.LookRotation(incrementedRotation, Vector3.up);
        }
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StdToCameraDir(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        //system.Movement.ExitEnabled = false;

        GameInfo.CameraController.Speed = 0.45f;
        GameInfo.CameraController.TargetSpeed = 0;
        GameInfo.CameraController.SpeedGradation = 0.22f;

        PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
    }

    private void DuringAct()
    {
        float compositeSpeed = 
            speed * PlayerInfo.StatsManager.MovespeedMultiplier.Value;
        system.CharMoveSystem.GroundMove(direction * compositeSpeed);
    }

    private void ActEnd()
    {  
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.TargetPercentileSpeed = GameInfo.Settings.LeftDirectionalInput.magnitude;
        PlayerInfo.MovementManager.SnapSpeed();
        system.Physics.GravityStrength = PhysicsSystem.GravitationalConstant;
        //system.Movement.ExitEnabled = true;
        GameInfo.CameraController.SpeedGradation = 0.08f;
    }

    private void DuringSlide()
    {
        float compositeSpeed = 
            speed * PlayerInfo.StatsManager.MovespeedMultiplier.Value * 0.5f;
        system.CharMoveSystem.GroundMove(direction * compositeSpeed);
    }

    private void SlideEnd()
    {
        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(this, false);
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