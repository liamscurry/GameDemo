using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Weapon ability that casts Fireball projectiles.
// When making tier 4 need to implement DeleteResouce to this tier 3
public sealed class PlayerFireballTier3 : PlayerAbility
{
    private AbilitySegment act;
    private AbilityProcess waitProcess;
    private AbilityProcess shootProcess;

    private const float damage = 1f;

    private int currentGroupID;
    private const int groupIDMax = 10000;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        AnimationClip actClip = Resources.Load<AnimationClip>("Player/Abilities/Fireball/FireballTier3Act");

        waitProcess = new AbilityProcess(null, null, null, 0.25f);
        shootProcess = new AbilityProcess(ActBegin, null, null, 0.75f);
        act = new AbilitySegment(actClip, waitProcess, shootProcess);
        act.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Durations
        currentGroupID = 0;

        continous = true;

        staminaCost = 1f * 0.5f;
        GenerateCoolDownIcon(
            staminaCost,
            Resources.Load<Sprite>(ResourceConstants.Player.UI.Abilities.FireballTier3Icon),
            "III");
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCost &&
               Physics.OverlapSphere(CalculateStartPosition(), 1f, LayerConstants.GroundCollision).Length == 0;
    }

    // Called every frame of ability to keep movement during duration.
    public override void GlobalConstantUpdate()
    {
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = forwardDirection + sidewaysDirection;

        //Direction and speed targets
        if (GameInfo.Settings.LeftDirectionalInput.magnitude <= 0.5f)
        {
            PlayerInfo.MovementManager.LockDirection();
            PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        }
        else
        {
            Vector3 targetRotation = Matho.StandardProjection3D(GameInfo.CameraController.Direction).normalized;
            Vector3 currentRotation = Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;
            Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
            Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
            PlayerInfo.Player.transform.rotation = rotation;

            PlayerInfo.MovementManager.TargetDirection = movementDirection;

            float forwardsAngle = Matho.AngleBetween(Matho.StandardProjection2D(targetRotation), movementDirection);
            float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
        
            PlayerInfo.MovementManager.TargetPercentileSpeed = GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier;
        }

        PlayerInfo.MovementSystem.Move(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);

        PlayerInfo.Animator.SetFloat("speed", PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.MovespeedMultiplier.Value);
    }

	public void ActBegin()
    {
        // PlayerInfo.Capsule.TopSpherePosition()
        Vector3 startPosition = CalculateStartPosition();
        Vector3 direction = CalculateProjectileDirection(startPosition);
        SpawnProjectiles(direction, startPosition);
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
    }

    private Vector3 CalculateStartPosition()
    {
        return PlayerInfo.Capsule.TopSpherePosition() +
               Vector3.up * 0.75f +
               GameInfo.CameraController.transform.right * -1 * 0.5f;
    }

    private Vector3 CalculateProjectileDirection(Vector3 startPosition)
    {
        Vector2 analog = GameInfo.Settings.RightDirectionalInput;
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        
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
        var group = new List<HomingFireboltProjectile>();
        currentGroupID = (currentGroupID + 1) % groupIDMax;

        for (int i = 0; i < 2; i++)
        {
            Vector3 iterationRight = Vector3.Cross(direction, Vector3.up);
            Vector3 iterationUp = Vector3.Cross(iterationRight, direction);
            Vector3 iterationDirection = Matho.Rotate(direction, iterationUp, 5f * (i - 0.5f));
            Vector3 velocity = 60 * iterationDirection;

            HomingFireboltProjectile projectile = 
                GameInfo.ProjectilePool.Create<HomingFireboltProjectile>(
                    Resources.Load<GameObject>(ResourceConstants.Player.Projectiles.HomingFireball), 
                    startPosition,
                    velocity * (1f - 0.5f * (i / 4f)),
                    5,
                    TagConstants.EnemyHitbox,
                    OnHit,
                    ProjectileArgs.Empty);

            group.Add(projectile);
        }

        foreach (HomingFireboltProjectile projectile in group)
        {
            projectile.SetGroupInformation(group, currentGroupID);
        }
    }

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