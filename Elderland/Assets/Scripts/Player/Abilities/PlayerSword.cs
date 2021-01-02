using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Weapon ability with a light melee attack.

public sealed class PlayerSword : PlayerAbility 
{
    //Fields
    private AnimationClip chargeFarDashClip;
    private AnimationClip actFarDashClip;
    private AnimationClip chargeFarRunClip;
    private AnimationClip actFarRunClip;
    private AnimationClip chargeCloseClip;
    private AnimationClip actCloseClip;
    private AnimationClip chargeNoTargetClip;
    private AnimationClip actNoTargetClip;

    private float damage = 0.5f;//0.5
    private float strength = 18;
    private PlayerMultiDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(3f, 2, 2);
    private ParticleSystem hitboxParticles;
    private Gradient hitboxParticlesNormalGradient;
    private ParticleSystem.ColorOverLifetimeModule hitboxParticlesColors;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilitySegment charge;
    private AbilitySegment act;

    private enum Type { NoTarget, FarTarget, CloseTarget }
    private Type type;

    private Vector2 playerDirection;
    private Vector3 playerPlanarDirection;
    private Collider target;
    private Vector3 targetDisplacement;
    private Vector3 targetPlanarDirection;
    private float targetWidth;
    private float targetHorizontalDistance;

    private PlayerAnimationManager.MatchTarget matchTarget;
    private bool interuptedTarget;

    private int castDirection;
    private float baseSpeed = 0.75f;
    private float maxSpeed = 1.5f;
    private float hitTime;
    private const float resetHitTime = 3.5f;

    public bool IsAbilitySpeedReset { get { return Time.time - hitTime > resetHitTime; } }

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        //Animation assignment
        chargeFarDashClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeFarDashLightAttack");
        actFarDashClip = Resources.Load<AnimationClip>("Player/Abilities/ActFarDashLightAttack");

        chargeFarRunClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeFarRunLightAttack");
        actFarRunClip = Resources.Load<AnimationClip>("Player/Abilities/ActFarRunLightAttack");

        chargeCloseClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeCloseLightAttack");
        actCloseClip = Resources.Load<AnimationClip>("Player/Abilities/ActCloseLightAttack");

        chargeNoTargetClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeNoTargetLightAttack");
        actNoTargetClip = Resources.Load<AnimationClip>("Player/Abilities/ActNoTargetLightAttack");

        chargeProcess = new AbilityProcess(ChargeBegin, DuringCharge, ChargeEnd, 1);
        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        charge = new AbilitySegment(null, chargeProcess);
        charge.Type = AbilitySegmentType.RootMotion;
        act = new AbilitySegment(null, actProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;
        continous = true;

        //Hitbox initializations
        GameObject hitboxObject = Instantiate(Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularMultiHitbox), transform.position, Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxObject.SetActive(false);

        hitbox = hitboxObject.GetComponent<PlayerMultiDamageHitbox>();
        hitbox.gameObject.transform.localScale = hitboxScale;

        GameObject hitboxParticlesObject =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.SwordParticles),
                transform.position,
                Quaternion.identity);
        hitboxParticlesObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxParticles = hitboxParticlesObject.GetComponent<ParticleSystem>();
        hitboxParticlesNormalGradient = hitboxParticles.colorOverLifetime.color.gradient;
        hitboxParticlesColors = hitboxParticles.colorOverLifetime;

        scanRotation = Quaternion.identity;   

        abilitySpeed = baseSpeed;     
    }

    public override void GlobalConstantUpdate()
    {
        if (PlayerInfo.PhysicsSystem.TouchingFloor)
        {
            RaycastHit raycast;

            bool hit = UnityEngine.Physics.SphereCast(
                PlayerInfo.Player.transform.position,
                PlayerInfo.Capsule.radius,
                Vector3.down,
                out raycast,
                (PlayerInfo.Capsule.height / 2) + PlayerInfo.Capsule.radius * 0.5f,
                LayerConstants.GroundCollision);

            if (hit)
            {
                float height = PlayerInfo.Player.transform.position.y - (raycast.distance) + (PlayerInfo.Capsule.height / 2 - PlayerInfo.Capsule.radius);
                //PlayerInfo.Manager.test = PlayerInfo.Player.transform.position + (raycast.distance) * Vector3.down;
                PlayerInfo.Player.transform.position = new Vector3(PlayerInfo.Player.transform.position.x, height, PlayerInfo.Player.transform.position.z);
            }

            if (!interuptedTarget && PlayerInfo.Animator.isMatchingTarget)
            {
                if (CheckForGround())
                {
                    interuptedTarget = true;
                    PlayerInfo.Animator.InterruptMatchTarget(false);
                    matchTarget.positionWeight = Vector3.zero;
                    PlayerInfo.AnimationManager.StartTargetImmediately(matchTarget);
                }
            }   
        }
    }

    private bool CheckForGround()
    {
        //RaycastHit predictLedgeCastClose;
        //RaycastHit predictLedgeCastFar;
        Vector3 interuptDirection = (dashPosition - PlayerInfo.Player.transform.position).normalized;
        //Debug.DrawLine(dashPosition, PlayerInfo.Player.transform.position, Color.black, 5);
        /*
        bool hitLedgeClose = UnityEngine.Physics.Raycast(
            PlayerInfo.Player.transform.position + interuptDirection * 0.6f,
            Vector3.down,
            out predictLedgeCastClose,
            (PlayerInfo.Capsule.height / 2) + PlayerInfo.Capsule.radius * 0.5f,
            LayerConstants.GroundCollision | LayerConstants.Destructable);

        bool hitLedgeFar = UnityEngine.Physics.Raycast(
            PlayerInfo.Player.transform.position + interuptDirection * 0.7f,
            Vector3.down,
            out predictLedgeCastFar,
            (PlayerInfo.Capsule.height / 2) + PlayerInfo.Capsule.radius * 0.5f,
            LayerConstants.GroundCollision | LayerConstants.Destructable);
        */
        //if (!hitLedgeClose || !hitLedgeFar)
        {
            Collider[] overlapColliders = UnityEngine.Physics.OverlapSphere(
                PlayerInfo.Player.transform.position + PlayerInfo.Capsule.BottomSphereOffset() + Vector3.up * 0.2f + interuptDirection * 0.3f,
                PlayerInfo.Capsule.radius,
                LayerConstants.GroundCollision | LayerConstants.Destructable);
            
            if (overlapColliders.Length == 0)
            {
                return false;
            }
        }

        return true;
    }

    protected override void GlobalStart()
    {
        interuptedTarget = false;
          
        playerDirection = (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.5f) ?
            GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput) :
            Matho.StandardProjection2D(GameInfo.CameraController.Direction);

        playerPlanarDirection = Matho.PlanarDirectionalDerivative(playerDirection, PlayerInfo.PhysicsSystem.Normal).normalized;

        //Scan for enemies
        Vector3 center = 3f * playerPlanarDirection;
        Vector3 size = new Vector3(6f, 2, 4);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, playerPlanarDirection);
        Collider[] hitboxColliders = Physics.OverlapBox(PlayerInfo.Player.transform.position + center, size / 2, rotation, LayerConstants.Hitbox);

        //Draw parameters   
        scanCenter = PlayerInfo.Player.transform.position + center;
        scanSize = size;
        scanRotation = rotation;

        List<Collider> enemyColliders = new List<Collider>();
        foreach (Collider collider in hitboxColliders)
        {
            if (collider.tag == TagConstants.EnemyHitbox)
                enemyColliders.Add(collider);
        }

        if (enemyColliders.Count > 0)
        {
            //Locate closest enemy
            float minDistance = Vector3.Distance(enemyColliders[0].transform.position, PlayerInfo.Player.transform.position);
            Collider minCollider = enemyColliders[0];
            for (int i = 1; i < enemyColliders.Count; i++)
            {
                float distance = Vector3.Distance(enemyColliders[i].transform.position, PlayerInfo.Player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minCollider = enemyColliders[i];
                }
            }

            //Target info
            //GameInfo.CameraController.SecondaryTarget = minCollider.transform;
            target = minCollider;
            targetDisplacement = (minCollider.transform.parent.position - PlayerInfo.Player.transform.position);
            targetPlanarDirection = Matho.PlanarDirectionalDerivative(Matho.StandardProjection2D(targetDisplacement).normalized, PlayerInfo.PhysicsSystem.Normal).normalized;

            float theta = Matho.AngleBetween(targetDisplacement, targetPlanarDirection);
            targetHorizontalDistance = targetDisplacement.magnitude * Mathf.Cos(theta * Mathf.Deg2Rad);

            Ray enemyRay = new Ray(PlayerInfo.Player.transform.position, targetPlanarDirection);
            RaycastHit enemyHit;
            target.Raycast(enemyRay, out enemyHit, targetHorizontalDistance);
            targetWidth = targetDisplacement.magnitude - enemyHit.distance;

            //Far target
            if (targetHorizontalDistance - PlayerInfo.Capsule.radius - targetWidth > hitboxScale.z / 2)
            {
                //PlayerInfo.Animator.SetBool("targetMatch", true);

                float distance = targetHorizontalDistance - PlayerInfo.Capsule.radius - targetWidth;
                if (distance > (hitboxScale.z / 2) && distance < (2.5f * hitboxScale.z / 2))
                {
                    charge.Clip = chargeFarDashClip;
                    act.Clip = actFarDashClip;
                }
                else
                {
                    charge.Clip = chargeFarRunClip;
                    act.Clip = actFarRunClip;
                }

                type = Type.FarTarget;
            }
            else
            {
                //Close target
                //PlayerInfo.Animator.SetBool("targetMatch", true);
                charge.Clip = chargeCloseClip;
                act.Clip = actCloseClip;

                type = Type.CloseTarget;
            }
        }
        else
        {
            //No target
            //PlayerInfo.Animator.SetBool("targetMatch", true);
            charge.Clip = chargeNoTargetClip;
            act.Clip = actNoTargetClip;

            type = Type.NoTarget;
        }

        //charge.Clip.apparentSpeed

        if (Input.GetKey(GameInfo.Settings.MeleeAbilityKey))
        {
            castDirection = 0;
        }
        else
        {
            castDirection = 1;
        }

        if (Time.time - hitTime > resetHitTime)
        {
            abilitySpeed = baseSpeed;
        }

        if (PlayerInfo.StatsManager.AttackSpeedMultiplier.ModifierCount != 0)
        {
            abilitySpeed = PlayerInfo.StatsManager.AttackSpeedMultiplier.Value;
        }
    }

    public void ChargeBegin()
    {
        if (type == Type.NoTarget)
        {
            RaycastHit distanceHit;
            bool distanceRegistered =
                Physics.SphereCast(
                    PlayerInfo.Player.transform.position,
                    PlayerInfo.Capsule.radius,
                    playerPlanarDirection.normalized,
                    out distanceHit,
                    1,
                    LayerConstants.GroundCollision | LayerConstants.Destructable);

            float distance = (distanceRegistered) ? distanceHit.distance : 1;

            Collider[] overlappingColliders = 
            Physics.OverlapSphere(
                PlayerInfo.Player.transform.position + playerPlanarDirection.normalized * 0.5f,
                PlayerInfo.Capsule.radius,
                LayerConstants.GroundCollision | LayerConstants.Destructable);
            
            if (overlappingColliders.Length > 0)
                distance = 0;

            //need to limit matchtarget here
            Vector3 targetPosition = PlayerInfo.Player.transform.position + playerPlanarDirection.normalized * distance;
            Quaternion targetRotation = Quaternion.LookRotation(Matho.StandardProjection3D(playerPlanarDirection), Vector3.up);
            matchTarget = new PlayerAnimationManager.MatchTarget(targetPosition, targetRotation, AvatarTarget.Root, new Vector3(1, 0, 1), 1);//1 / 0.25f
            charge.LoopFactor = 1;
            //PlayerInfo.AnimationManager.AnimationPhysicsEnable();

            dashPosition = targetPosition;

            if (CheckForGround())
                matchTarget.positionWeight = Vector3.zero;
                
            PlayerInfo.AnimationManager.StartTarget(matchTarget);
        }
        else if (type == Type.CloseTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Matho.StandardProjection3D(targetDisplacement).normalized, Vector3.up);
            matchTarget = new PlayerAnimationManager.MatchTarget(Vector3.zero, targetRotation, AvatarTarget.Root, Vector3.zero, 1);//1 / 0.25f
            charge.LoopFactor = 1;

            //PlayerInfo.AnimationManager.AnimationPhysicsEnable();
            PlayerInfo.AnimationManager.StartTarget(matchTarget);

            dashPosition = PlayerInfo.Player.transform.position;
        }
        else if (type == Type.FarTarget)
        {
            //need to limit matchtarget here
            RaycastHit directionHit;
            float offset = PlayerInfo.Capsule.radius + targetWidth + hitboxScale.z / 3;
            bool hit = Physics.CapsuleCast(
                                PlayerInfo.Capsule.TopSpherePosition(),
                                PlayerInfo.Capsule.BottomSpherePosition(),
                                PlayerInfo.Capsule.radius - 0.05f,
                                targetPlanarDirection,
                                out directionHit,
                                targetHorizontalDistance - offset,
                                LayerConstants.GroundCollision | LayerConstants.Destructable);
            float targetDistance = (hit) ? directionHit.distance : targetHorizontalDistance - offset;
            Vector3 targetPosition = PlayerInfo.Player.transform.position + targetDistance * targetPlanarDirection;
           
            float loopFactor = 0;
            float distance = targetHorizontalDistance - PlayerInfo.Capsule.radius - targetWidth;
            if (distance > (hitboxScale.z / 2) && distance < (2.5f * hitboxScale.z / 2))
            {
                loopFactor = 1;
            }
            else
            {
                loopFactor = (targetHorizontalDistance - PlayerInfo.Capsule.radius - targetWidth) / (2.5f * (hitboxScale.z / 2));
            }

            Quaternion targetRotation = Quaternion.LookRotation(Matho.StandardProjection3D(targetDisplacement).normalized, Vector3.up);
            matchTarget = new PlayerAnimationManager.MatchTarget(targetPosition, targetRotation, AvatarTarget.Root, Vector3.one, loopFactor / 0.25f, 0, loopFactor);
            charge.LoopFactor = loopFactor;
            //PlayerInfo.AnimationManager.AnimationPhysicsEnable();
            PlayerInfo.AnimationManager.StartTarget(matchTarget);

            dashPosition = targetPosition;
        }
    }

    public void DuringCharge()
    {
        //Debug.Log("being called");
        if (Physics.OverlapSphere(
            PlayerInfo.Player.transform.position,
            PlayerInfo.Capsule.radius * 1.75f,
            LayerConstants.Enemy).Length != 0)
        {
            PlayerInfo.Animator.InterruptMatchTarget(false);
            ForceAdvanceSegment();
        }
    }

    public void ChargeEnd()
    {
        
    }

    public void ActBegin()
    {
        //print("act begin called");
        hitbox.gameObject.SetActive(true);
        if (type == Type.CloseTarget || type == Type.FarTarget)
        {
            hitbox.Invoke(this, target);
        }
        else
        {
            hitbox.Invoke(this);
        }
        hitbox.gameObject.transform.position = PlayerInfo.Player.transform.position;
        hitboxParticles.transform.position = hitbox.transform.position;

        Quaternion horizontalRotation = Quaternion.identity;

        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, PlayerInfo.PhysicsSystem.Normal);
        hitbox.transform.rotation = normalRotation * horizontalRotation;

        float verticalSign = Random.value;
        verticalSign = (verticalSign > 0.5) ? 1 : -1;

        float rotationSign = Random.value;
        rotationSign = (rotationSign > 0.5) ? 0.5f : 1;

        float flipSign = Random.value;
        flipSign = (flipSign > 0.5) ? 1 : 0;

        // * Mathf.Clamp((Random.value * 2f - 1), -1, 1)
        Quaternion tiltRotation =
            Quaternion.Euler(
                180 * flipSign,
                180 * flipSign,
                30 * verticalSign * rotationSign);
        hitboxParticles.transform.rotation = normalRotation * horizontalRotation * tiltRotation;
        hitboxParticles.transform.localScale = Vector3.one;
        hitboxParticlesColors.color = hitboxParticlesNormalGradient;

        //hitboxParticles.transform.localScale = new Vector3(flipSign, 1, 1);

        hitboxParticles.Play();
    }

    public void DuringAct()
    {
        hitbox.gameObject.transform.position = PlayerInfo.Player.transform.position;
    }

    public void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
        target = null;
        PlayerInfo.MovementManager.TargetDirection = Matho.StandardProjection2D(PlayerInfo.Player.transform.forward).normalized;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.ZeroSpeed();

        float exitMoveSpeed = 0;
        if (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.2f)
        {
            exitMoveSpeed = 1;
        }
        else
        {
            exitMoveSpeed = 0;
            PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        }
        PlayerInfo.Animator.SetFloat("speed", exitMoveSpeed);
    }

    Vector3 scanCenter;
    Vector3 scanSize;
    Quaternion scanRotation;
    Vector3 dashPosition;
    private void OnDrawGizmosSelected()
    {
        //scan
        Matrix4x4 customMatrix = Matrix4x4.TRS(scanCenter, scanRotation, Vector3.one);
        Gizmos.matrix = customMatrix;

        Gizmos.color = Color.green;
        //Gizmos.DrawCube(Vector3.zero, scanSize);

        Gizmos.matrix = Matrix4x4.identity;

        //target
        Gizmos.color = Color.green;
        //Gizmos.DrawCube(targetPosition, Vector3.one);
    }

    public override bool OnHit(GameObject character)
    {
        EnemyManager enemy = character.GetComponent<EnemyManager>();
        float directionalDamageModifier = 1;
        float playerEnemyAngle =
            Matho.AngleBetween(
                PlayerInfo.Player.transform.forward,
                enemy.transform.position - PlayerInfo.Player.transform.position);
        if (playerEnemyAngle > 25)
            directionalDamageModifier = 0.5f;

        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value * directionalDamageModifier);

        PlayerInfo.AbilityManager.ChangeStamina(
            0.5f * PlayerInfo.StatsManager.StaminaYieldMultiplier.Value);

        if (castDirection == enemy.WeakDirection)
        {
            if (enemy.CheckResolve())
            {
                enemy.Push((enemy.transform.position - PlayerInfo.Player.transform.position).normalized * 5.5f);
                enemy.ChangeHealth(
                    -damage * PlayerInfo.StatsManager.DamageMultiplier.Value * 2);
                enemy.ConsumeResolve();

                hitboxParticles.transform.localScale = Vector3.one * 1.5f;
                
                // Based on Particle System manual API.
                Gradient newGradient = new Gradient();
                newGradient.SetKeys(
                    new GradientColorKey[] 
                        { new GradientColorKey(new Color(1,1,1,1), 0),
                          new GradientColorKey(new Color(1,1,1,1), 1)},
                    new GradientAlphaKey[] 
                        { new GradientAlphaKey(0, 0),
                          new GradientAlphaKey(.5f, 0.1f),
                          new GradientAlphaKey(.5f, 0.9f),
                          new GradientAlphaKey(0, 1) }
                );

                hitboxParticlesColors.color = newGradient;
                hitboxParticles.Play();
            }
            else
            {
                enemy.Push((enemy.transform.position - PlayerInfo.Player.transform.position).normalized * 1.5f);
                if (directionalDamageModifier == 1)
                    enemy.IncreaseResolve(0.5f);
            }

            abilitySpeed += 0.5f;
            if (abilitySpeed > maxSpeed)
                abilitySpeed = maxSpeed;

            enemy.ScrambleWeakDirection();
        }
        else
        {
            abilitySpeed = baseSpeed;
        }

        if (PlayerInfo.StatsManager.AttackSpeedMultiplier.ModifierCount != 0)
        {
            abilitySpeed = PlayerInfo.StatsManager.AttackSpeedMultiplier.Value;
        }

        hitTime = Time.time;

        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
        PlayerInfo.Animator.InterruptMatchTarget(false);
    }
}