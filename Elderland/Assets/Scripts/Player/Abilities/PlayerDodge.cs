using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDodge : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 5f;
    private const float maxSpeedOnExit = 2.5f;

    private AbilitySegment dodgeSegment;
    private AbilityProcess actProcess;
    private AbilityProcess slideProcess;

    private Vector3 targetPlayerPoint;
    private const float rotationSpeed = 9f;
    private const float lookDistance = 0.3f;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 0.82f);
        slideProcess = new AbilityProcess(null, DuringSlide, SlideEnd, 1 - 0.82f);
        dodgeSegment = new AbilitySegment(null, actProcess, slideProcess);
        dodgeSegment.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(dodgeSegment);
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

        dodgeSegment.Clip = actClip;

        targetPlayerPoint =
            PlayerInfo.Player.transform.position +
            PlayerInfo.Player.transform.forward * lookDistance;

        PlayerInfo.AbilityManager.LastDirFocus = Time.time;
        PlayerInfo.AbilityManager.DirFocus = PlayerInfo.Player.transform.forward;
    }

    public override void GlobalUpdate()
    {
        Vector3 targetPlayerDir = targetPlayerPoint - PlayerInfo.Player.transform.position;
        targetPlayerDir = Matho.StdProj3D(targetPlayerDir).normalized;

        if (targetPlayerDir.magnitude != 0)
        {
            Vector3 incrementedRotation = PlayerInfo.Player.transform.forward;
            incrementedRotation =
                Vector3.RotateTowards(
                    incrementedRotation,
                    targetPlayerDir,
                    rotationSpeed * Time.deltaTime,
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

        GameInfo.CameraController.Speed = 0.45f;
        GameInfo.CameraController.TargetSpeed = 0;
        GameInfo.CameraController.SpeedGradation = 0.22f;

        PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
        PlayerInfo.CharMoveSystem.MaxConstantOnExit.ClaimLock(this, maxSpeedOnExit);
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
        PlayerInfo.CharMoveSystem.MaxConstantOnExit.TryReleaseLock(this, float.MaxValue);
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