using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDodge : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 4f;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private AbilityProcess slideProcess;

    private float swordSpeedModifier;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 0.82f);
        slideProcess = new AbilityProcess(null, DuringSlide, null, 1 - 0.82f);
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

        /*
        if (PlayerInfo.StatsManager.AttackSpeedMultiplier.ModifierCount != 0)
        {
            abilitySpeed = 1 / (PlayerInfo.StatsManager.AttackSpeedMultiplier.Value);
            Debug.Log(PlayerInfo.StatsManager.AttackSpeedMultiplier.Value);
        }*/

        // Convert left dir. input to player space.
        Vector2 up = Matho.StandardProjection2D(PlayerInfo.Player.transform.forward);
        Vector2 right = Matho.Rotate(up, 90f);
        Vector2 worldInput =
            GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        float projXInput = Matho.ProjectScalar(worldInput, right);
        float projYInput = Matho.ProjectScalar(worldInput, up);

        Debug.Log(Matho.AngleBetween(right, worldInput) + ", " + new Vector2(projXInput, projYInput));

        AnimationClip actClip =
            GetDirAnim(ResourceConstants.Player.Art.Dodge, new Vector2(projXInput, projYInput));

        act.Clip = actClip;
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
        PlayerInfo.AnimationManager.Interuptable = false;
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

    private void DuringSlide()
    {
        if (system.Physics.TouchingFloor)
        {
            float compositeSpeed = 
                speed * (1 / abilitySpeed) * PlayerInfo.StatsManager.MovespeedMultiplier.Value * 0.5f;
            actVelocity = system.Movement.Move(direction, compositeSpeed, false);
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