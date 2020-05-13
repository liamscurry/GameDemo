using UnityEngine;

//Weapon ability that casts Fireball projectiles.

public sealed class PlayerFireballTier3 : PlayerAbility
{
    private AbilitySegment act;
    private AbilityProcess waitProcess;
    private AbilityProcess shootProcess;

    private const float damage = 3f;
    private const float staminaCost = 1;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        AnimationClip actClip = Resources.Load<AnimationClip>("Player/Abilities/Ranged/FireballAct");

        waitProcess = new AbilityProcess(null, null, null, 0.35f);
        shootProcess = new AbilityProcess(ActBegin, null, null, 0.65f);
        act = new AbilitySegment(actClip, waitProcess, shootProcess);
        act.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Durations
        coolDownDuration = .1f;
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCost;
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

        PlayerInfo.Animator.SetFloat("speed", PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.MovespeedModifier);
    }

	public void ActBegin()
    {
        Vector2 analog = GameInfo.Settings.RightDirectionalInput;
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        
        Ray cursorRay = GameInfo.CameraController.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit cursorHit;
        Vector3 direction = Vector3.zero;
        if (Physics.Raycast(cursorRay, out cursorHit, 100f, LayerConstants.GroundCollision))
        {
            direction = (cursorHit.point - PlayerInfo.Capsule.TopSpherePosition()).normalized;
        }
        else
        {
            direction = ((cursorRay.origin + 100f * cursorRay.direction) - PlayerInfo.Capsule.TopSpherePosition()).normalized;
        }
        
        for (int i = 0; i < 5; i++)
        {
            Vector3 iterationRight = Vector3.Cross(direction, Vector3.up);
            Vector3 iterationUp = Vector3.Cross(iterationRight, direction);
            Vector3 iterationDirection = Matho.Rotate(direction, iterationUp, 5f * (i - 2));
            Vector3 velocity = 50 * iterationDirection; // 50

            GameInfo.ProjectilePool.Create<HomingFireboltProjectile>(
                Resources.Load<GameObject>(ResourceConstants.Player.Projectiles.HomingFireball), 
                PlayerInfo.Capsule.TopSpherePosition(),
                velocity * (1f - 0.5f * (i / 4f)),
                2,
                TagConstants.EnemyHitbox,
                OnHit,
                ProjectileArgs.Empty);
        }

        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
    }

    public override bool OnHit(GameObject character)
    {
        if (character != null)
        {
            EnemyManager enemy = character.GetComponent<EnemyManager>();
            enemy.ChangeHealth(-damage);
        }
        return true;
    }

    public override void ShortCircuitLogic()
    {
        
    }
}