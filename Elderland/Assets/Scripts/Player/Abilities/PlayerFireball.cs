using UnityEngine;

//Weapon ability that casts Fireball projectiles.

public sealed class PlayerFireball : PlayerAbility
{
    private AbilitySegment act;
    private AbilityProcess waitProcess;
    private AbilityProcess shootProcess;

    private const float damage = 0.75f;
    private const float staminaCost = 1f;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        AnimationClip actClip = Resources.Load<AnimationClip>("Player/Abilities/Fireball/FireballAct");

        waitProcess = new AbilityProcess(null, null, null, 0.25f);
        shootProcess = new AbilityProcess(ActBegin, null, null, 0.75f);
        act = new AbilitySegment(actClip, waitProcess, shootProcess);
        act.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Durations
        continous = true;
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCost;
    }

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

    /*
    public void WaitEnd()
    {
        Debug.Log("called");
        //Slow player movement speed
        PlayerInfo.StatsManager.MovespeedModifier = .75f;
        PlayerInfo.StatsManager.MovespeedEditor = gameObject;
    }
    */

	public void ActBegin()
    {
        // PlayerInfo.Capsule.TopSpherePosition()
        Vector3 startPosition = 
            PlayerInfo.Capsule.TopSpherePosition() +
            Vector3.up * 0.75f +
            GameInfo.CameraController.transform.right * -1 * 0.5f;
        Vector3 direction = CalculateProjectileDirection(startPosition);
        SpawnProjectiles(direction, startPosition);
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
    }

    private Vector3 CalculateProjectileDirection(Vector3 startPosition)
    {
        Vector2 analog = GameInfo.Settings.RightDirectionalInput;
        Vector2 projectedCameraDirection = Matho.StandardProjection2D(GameInfo.CameraController.Direction).normalized;
        
        Ray cursorRay = GameInfo.CameraController.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit cursorHit;
        Vector3 direction = Vector3.zero;
        if (Physics.Raycast(cursorRay, out cursorHit, 100f, LayerConstants.GroundCollision | LayerConstants.Enemy))
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
        Vector3 velocity = 50 * direction;

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
}