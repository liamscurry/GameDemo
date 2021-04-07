using UnityEngine;
using UnityEngine.UI;

//Weapon ability that casts Fireball projectiles.

public sealed class PlayerFireball : PlayerAbility
{
    private AbilitySegment chargeSegment;
    private AbilityProcess chargeProcess;

    private AbilitySegment actSegment;
    private AbilityProcess waitProcess;
    private AbilityProcess shootProcess;

    private const float damage = 2f;
    private const float speed = 50;
    private const float walkSlowRate = 3;

    private float walkSpeedModifier;

    private PlayerAbilityHold chargeSegmentHold;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        AnimationClip chargeClip = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FireballLeftCharge);
        AnimationClip actClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FireballLeftAct);

        chargeProcess = new AbilityProcess(ChargeStart, ChargeUpdate, ChargeEnd, 1, true);
        chargeSegment = new AbilitySegment(chargeClip, chargeProcess);

        waitProcess = new AbilityProcess(null, null, null, 0.25f);
        shootProcess = new AbilityProcess(ActBegin, null, null, 0.75f);
        actSegment = new AbilitySegment(actClip, waitProcess, shootProcess);
        actSegment.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        segments.AddSegment(chargeSegment);
        segments.AddSegment(actSegment);
        segments.NormalizeSegments();

        chargeSegmentHold =
            new PlayerAbilityHold(
                abilitySystem.HoldBar,
                chargeProcess,
                0.25f,
                0.4f,
                () => Mathf.Abs(GameInfo.Settings.FireballTrigger) < GameInfo.Settings.FireballTriggerOffThreshold,
                true
            );

        //Durations
        continous = true;

        staminaCost = 1f;

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
        walkSpeedModifier = 1;
        GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -10, 0.32f));
    }

    public override void GlobalConstantUpdate()
    {
        walkSpeedModifier -= walkSlowRate * Time.deltaTime;
        if (walkSpeedModifier < 0)
            walkSpeedModifier = 0;
        PlayerInfo.AbilityManager.MoveDuringAbility(walkSpeedModifier);
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

    public void ChargeStart()
    {
        chargeSegmentHold.Start();
    }

    public void ChargeUpdate()
    {
        chargeSegmentHold.Update();
    }

    public void ChargeEnd()
    {
        chargeSegmentHold.End();
        GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0f, 0f));
    }

	public void ActBegin()
    {
        if (chargeSegmentHold.Held)
        {
            // PlayerInfo.Capsule.TopSpherePosition()
            Vector3 startPosition = CalculateStartPosition();
            Vector3 direction = CalculateProjectileDirection(startPosition);
        
            SpawnProjectiles(direction, startPosition);
            PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
        }
    }

    private Vector3 CalculateStartPosition()
    {
        return PlayerInfo.Capsule.TopSpherePosition() +
               Vector3.up * 0.35f +
               GameInfo.CameraController.transform.right * -1 * 0.5f +
               GameInfo.CameraController.transform.forward * -1 * 0.5f;
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