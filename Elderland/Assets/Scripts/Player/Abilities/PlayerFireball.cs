using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

//Weapon ability that casts Fireball projectiles.

public sealed class PlayerFireball : PlayerAbility
{
    private AbilitySegment actSegment;
    private AbilityProcess waitProcess;
    private AbilityProcess shootProcess;

    private const float damage = 0.25f;
    private const float speed = 50;
    private const float walkSlowRate = 3;

    // Animations
    private AnimationClip actSummon;
    private AnimationClip actHold;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        actSummon = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FireballRightSummon);
        actHold =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FireballRightHold);

        waitProcess = new AbilityProcess(WaitBegin, null, null, 0.25f);
        shootProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.75f);
        actSegment = new AbilitySegment(null, waitProcess, shootProcess);
        actSegment.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        //segments.AddSegment(chargeSegment);
        segments.AddSegment(actSegment);
        segments.NormalizeSegments();

        //Durations
        continous = true;

        staminaCost = 0.125f;

        GenerateCoolDownIcon(
            staminaCost,
            Resources.Load<Sprite>(ResourceConstants.Player.Abilities.FireballTier1Icon),
            "I");
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCost &&
               Physics.OverlapSphere(CalculateStartPosition(), 1f, LayerConstants.GroundCollision).Length == 0;
    }

    protected override void GlobalStart()
    {
        actSegment.Clip = (replayed) ? actHold : actSummon;
    }

    public override void GlobalConstantUpdate()
    {
        PlayerInfo.MovementManager.UpdateWalkMovement();
        PlayerInfo.AnimationManager.UpdateWalkProperties();
    }

    protected override void ContinousStart()
    {
        PlayerInfo.AnimationManager.WalkAbilityLayer.ClaimTurnOn(this);
    }

    protected override void ContinousEnd()
    {
        PlayerInfo.AnimationManager.WalkAbilityLayer.ClaimTurnOff(this);
    }

    public void WaitBegin()
    {
        GameInfo.CameraController.SensitivityModifier = 0.4f;
    }

	public void ActBegin()
    {
        Vector3 startPosition = CalculateStartPosition();
        Vector3 direction = CalculateProjectileDirection(startPosition);
    
        SpawnProjectiles(direction, startPosition);
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
    }

    public void ActEnd()
    {
        GameInfo.CameraController.SensitivityModifier = 1f;
    }

    private Vector3 CalculateStartPosition()
    {
        return PlayerInfo.Capsule.TopSpherePosition() +
               Vector3.up * 0.9f +
               GameInfo.CameraController.transform.right * -1 * 0.654f +
               GameInfo.CameraController.transform.forward * -1 * 0.5f;
    }

    private Vector3 CalculateProjectileDirection(Vector3 startPosition)
    {
        Vector2 analog = GameInfo.Settings.RightDirectionalInput;
        Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
        
        Ray cursorRay = GameInfo.CameraController.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit cursorHit;
        Vector3 direction = Vector3.zero;
        if (Physics.Raycast(cursorRay, out cursorHit, 100f, LayerConstants.GroundCollision | LayerConstants.Enemy | LayerConstants.Destructable))
        {
            direction = (cursorHit.point - startPosition).normalized;
        }
        else
        {
            direction = ((cursorRay.origin + 100f * cursorRay.direction) - startPosition).normalized;
        }
        return direction;
    }

    private void SpawnProjectiles(Vector3 direction, Vector3 startPosition)
    {
        Vector3 velocity = speed * direction;

        GameInfo.ProjectilePool.Create<FireboltProjectile>(
            Resources.Load<GameObject>(ResourceConstants.Player.Projectiles.Fireball), 
            startPosition,
            velocity,
            2,
            TagConstants.EnemyHitbox,
            OnHit,
            ProjectileArgs.Empty);
    }

    /*
    protected override void Stop()
    {
        //Reset movespeed if not edited
        if (PlayerInfo.StatsManager.MovespeedEditor == gameObject)
        {
            PlayerInfo.StatsManager.MovespeedModifier = 1;
            PlayerInfo.StatsManager.MovespeedEditor = null;
        }
    }
    */

    public override bool OnHit(GameObject character)
    {
        if (character != null)
        {
            EnemyManager enemy = character.GetComponent<EnemyManager>();
            enemy.ChangeHealth(
                -damage * PlayerInfo.StatsManager.DamageMultiplier.Value);
        }
        return true;
    }

    public override void ShortCircuitLogic()
    {
        
    }

    public override void DeleteResources()
    {
        DeleteAbilityIcon();
    }
}