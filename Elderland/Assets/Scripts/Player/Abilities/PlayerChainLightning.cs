using UnityEngine;

//Weapon ability that casts Fireball projectiles.

public sealed class PlayerChainLightning : PlayerAbility
{
    private AbilitySegment charge;
    private AbilityProcess chargeProcess;
    private AbilitySegment hold;
    private AbilityProcess holdProcess;

    private PlayerMultiDamageHitbox hitbox;
    private BoxCollider hitboxTrigger;
    private Vector3 hitboxScale = new Vector3(3f, 2, 3.5f);
    private Vector3 hitboxPosition = new Vector3(0, 0, 2.5f);
    private Vector3 scaledHitboxPosition;

    public const float damage = 0.125f;
    private const float processDuration = 1f;
    private float processTimer;
    private const float damageDuration = 0.5f;
    private float damageTimer;

    public override void Initialize(PlayerAbilityManager abilitySystem)
    {
        //Specifications
        this.system = abilitySystem;

        AnimationClip chargeClip = Resources.Load<AnimationClip>("Player/Abilities/Drain/DrainCharge");
        AnimationClip holdClip = Resources.Load<AnimationClip>("Player/Abilities/Drain/DrainHold");

        chargeProcess = new AbilityProcess(ChargeBegin, DuringCharge, ChargeEnd, 1, true);
        charge = new AbilitySegment(chargeClip, chargeProcess);
        charge.Type = AbilitySegmentType.Physics;

        holdProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1, true);
        hold = new AbilitySegment(holdClip, holdProcess);

        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        segments.AddSegment(hold);
        segments.NormalizeSegments();

        //Hitbox initializations
        GameObject hitboxObject = Instantiate(Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularMultiHitbox), transform.position, Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform; 
        hitboxObject.SetActive(false);

        hitbox = hitboxObject.GetComponent<PlayerMultiDamageHitbox>();
        hitboxTrigger = hitboxObject.GetComponent<BoxCollider>();
        hitbox.transform.localScale = hitboxScale;

        scaledHitboxPosition = new Vector3(
            hitboxPosition.x / hitboxScale.x,
            hitboxPosition.y / hitboxScale.y,
            hitboxPosition.z / hitboxScale.z);

        //Durations
        coolDownDuration = 0.1f;
    }

    public override void GlobalConstantUpdate()
    {
        Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
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
            Vector3 targetRotation = Matho.StdProj3D(GameInfo.CameraController.Direction).normalized;
            
            PlayerInfo.MovementManager.TargetDirection = movementDirection;

            float forwardsAngle = Matho.AngleBetween(Matho.StdProj2D(targetRotation), movementDirection);
            float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
        
            PlayerInfo.MovementManager.TargetPercentileSpeed = GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier * 0.5f;
        }

        PlayerInfo.MovementSystem.Move(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);

        system.Animator.SetFloat("speed", PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.MovespeedMultiplier.Value);
    }

    public void ChargeBegin()
    {
        processTimer = 0;
    }

    public void DuringCharge()
    {
        Vector3 targetRotation = Matho.StdProj3D(GameInfo.CameraController.Direction).normalized;
        Vector3 currentRotation = Matho.StdProj3D(PlayerInfo.Player.transform.forward).normalized;
        Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
        Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
        PlayerInfo.Player.transform.rotation = rotation;

        float deltaAngle = Matho.AngleBetween(incrementedRotation, targetRotation);

        GameInfo.CameraController.OrientationModifier = Mathf.Clamp01(1 - 0.75f * (processTimer / 0.5f));

        processTimer += Time.deltaTime;

        if (deltaAngle < 1f && processTimer > 0.5f)
        {
            ActiveProcess.IndefiniteFinished = true;
        }
    }

    public void ChargeEnd()
    {
        GameInfo.CameraController.OrientationModifier = 0.25f;
    }

    public void ActBegin()
    {
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this, PlayerMultiDamageHitbox.ObstructionType.PlayerOrigin);

        hitbox.gameObject.transform.position = PlayerInfo.Player.transform.position;
        hitboxTrigger.center = scaledHitboxPosition;
        hitbox.Display.transform.localPosition = scaledHitboxPosition;
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, system.Physics.Normal);
        hitbox.transform.rotation = normalRotation * transform.rotation;

        GameInfo.CameraController.AllowZoom = false;    
        processTimer = 0;
        damageTimer = 0;
    }

    public void DuringAct()
    {
        Vector3 targetRotation = Matho.StdProj3D(GameInfo.CameraController.Direction).normalized;
        Vector3 currentRotation = Matho.StdProj3D(PlayerInfo.Player.transform.forward).normalized;
        Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
        Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
        PlayerInfo.Player.transform.rotation = rotation;

        hitbox.gameObject.transform.position = PlayerInfo.Player.transform.position;
        hitboxTrigger.center = scaledHitboxPosition;
        hitbox.Display.transform.localPosition = scaledHitboxPosition;
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, system.Physics.Normal);
        hitbox.transform.rotation = normalRotation * transform.rotation;

        processTimer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        if (damageTimer > damageDuration)
        {
            damageTimer = 0;
        }

        if (!pressedOverloadFrame && processTimer > 1f)
        {
            ActiveProcess.IndefiniteFinished = true;
        }
    }

	public void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
        hitbox.Deactivate();
        GameInfo.CameraController.AllowZoom = true; 
        GameInfo.CameraController.OrientationModifier = 1;
    }

    public override bool OnHit(GameObject character)
    {
        if (character != null)
        {
            EnemyManager enemy = character.GetComponent<EnemyManager>();
            if (!(enemy is LightEnemyManager) || (!enemy.Animator.GetBool("falling")))
            {
                enemy.GetComponent<Animator>().SetTrigger("drainShockStart");
                enemy.Freeze();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public override void OnLeave(GameObject character)
    {
        if (character != null)
        {
            EnemyManager enemy = character.GetComponent<EnemyManager>();
            enemy.GetComponent<Animator>().SetTrigger("drainShockEnd");
        }
    }

    public override void ShortCircuitLogic()
    {
        
    }
}