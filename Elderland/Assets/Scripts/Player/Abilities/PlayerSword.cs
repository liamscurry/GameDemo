using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Weapon ability with a light melee attack.

public sealed class PlayerSword : PlayerAbility 
{
    //Fields
    private AnimationClip holdClip;
    private AnimationClip chargeFarDashClip;
    private AnimationClip actFarDashClip;
    private AnimationClip chargeFarRunClip;
    private AnimationClip actFarRunClip;
    private AnimationClip chargeCloseClip;
    private AnimationClip actCloseClip;

    private AnimationClip chargeNoTargetClip;
    private AnimationClip actNoTargetClip;
    private AnimationClip holdNoTargetClip;
    private AnimationClip chargeNoTargetClipMirror;
    private AnimationClip actNoTargetClipMirror;
    private AnimationClip holdNoTargetClipMirror;

    private AnimationClip chargeNoTarget2Clip;
    private AnimationClip actNoTarget2Clip;
    private AnimationClip holdNoTarget2Clip;
    private AnimationClip chargeNoTarget2ClipMirror;
    private AnimationClip actNoTarget2ClipMirror;
    private AnimationClip holdNoTarget2ClipMirror;

    private AnimationClip chargeNoTarget3Clip;
    private AnimationClip actNoTarget3Clip;
    private AnimationClip holdNoTarget3Clip;
    private AnimationClip chargeNoTarget3ClipMirror;
    private AnimationClip actNoTarget3ClipMirror;
    private AnimationClip holdNoTarget3ClipMirror;

    private float damage = 0.5f;//0.5
    private float strength = 18;
    private float knockbackStrength = 4.5f;
    private PlayerMultiDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(3f, 2, 2);
    private ParticleSystem hitboxParticles;
    private Gradient hitboxParticlesNormalGradient;
    private ParticleSystem.ColorOverLifetimeModule hitboxParticlesColors;

    private AbilityProcess holdProcess;
    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilitySegment hold;
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
    private float baseSpeed = 1f;
    private float maxSpeed = 1.5f;
    private float hitTime;
    private const float resetHitTime = 3.5f;

    public bool IsAbilitySpeedReset { get { return Time.time - hitTime > resetHitTime; } }

    private PlayerAbilityHold holdSegmentHold;
    private KeyCode keyUsed;

    // Swing Direction
    private int flipSign;
    private float rotationSign;
    private int verticalSign;

    private float hitboxTimer;
    private const float hitboxDelay = 0.05f;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        //Animation assignment
        chargeFarDashClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeFarDashLightAttack");
        actFarDashClip = Resources.Load<AnimationClip>("Player/Abilities/ActFarDashLightAttack");

        chargeFarRunClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeFarRunLightAttack");
        actFarRunClip = Resources.Load<AnimationClip>("Player/Abilities/ActFarRunLightAttack");

        chargeCloseClip = Resources.Load<AnimationClip>("Player/Abilities/ChargeCloseLightAttack");
        actCloseClip = Resources.Load<AnimationClip>("Player/Abilities/ActCloseLightAttack");

        chargeNoTargetClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Charge);
        actNoTargetClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Act);
        holdNoTargetClip = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Hold);
        chargeNoTargetClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1MirrorCharge);
        actNoTargetClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1MirrorAct);
        holdNoTargetClipMirror = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Hold_M);

        chargeNoTarget2Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Charge);
        actNoTarget2Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Act);
        holdNoTarget2Clip = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Hold);
        chargeNoTarget2ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2MirrorCharge);
        actNoTarget2ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2MirrorAct);
        holdNoTarget2ClipMirror = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Hold_M);

        chargeNoTarget3Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Charge);
        actNoTarget3Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Act);
        holdNoTarget3Clip = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Hold);
        chargeNoTarget3ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3MirrorCharge);
        actNoTarget3ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3MirrorAct);
        holdNoTarget3ClipMirror = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Hold_M);


        holdProcess = new AbilityProcess(HoldBegin, DuringHold, HoldEnd, 1, true);
        chargeProcess = new AbilityProcess(ChargeBegin, DuringCharge, ChargeEnd, 1);
        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1f);
        hold = new AbilitySegment(null, holdProcess);
        charge = new AbilitySegment(null, chargeProcess);
        charge.Type = AbilitySegmentType.RootMotion;
        act = new AbilitySegment(null, actProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        //segments.AddSegment(hold);
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;
        continous = true;

        //Hitbox initializations
        GameObject hitboxObject =
            Instantiate(Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularMultiHitbox), transform.position, Quaternion.identity);
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

        holdSegmentHold =
            new PlayerAbilityHold(
                abilityManager.HoldBar,
                holdProcess,
                0.1f,
                0.25f,
                HeldPredicate,
                false
            );

        scanRotation = Quaternion.identity;   

        abilitySpeed = baseSpeed;     
    }

    public override bool Wait(bool firstTimeCalling)
    {
        if (Input.GetKey(GameInfo.Settings.MeleeAbilityKey))
        {
            keyUsed = GameInfo.Settings.MeleeAbilityKey;
        }
        else
        {
            keyUsed = GameInfo.Settings.AlternateMeleeAbilityKey;
        }

        bool success = base.Wait(firstTimeCalling);

        return success;
    }

    private bool HeldPredicate()
    {
        return !Input.GetKey(keyUsed);
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
        Vector3 interuptDirection = (dashPosition - PlayerInfo.Player.transform.position).normalized;

        Collider[] overlapColliders = UnityEngine.Physics.OverlapSphere(
            PlayerInfo.Player.transform.position + PlayerInfo.Capsule.BottomSphereOffset() + Vector3.up * 0.2f + interuptDirection * 0.3f,
            PlayerInfo.Capsule.radius,
            LayerConstants.GroundCollision | LayerConstants.Destructable);
        
        if (overlapColliders.Length == 0)
        {
            return false;
        }

        return true;
    }

    protected override void GlobalStart()
    {
        GenRanDirection();

        interuptedTarget = false;
          
        playerDirection = (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.5f) ?
            GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput) :
            Matho.StandardProjection2D(GameInfo.CameraController.Direction);

        playerPlanarDirection = Matho.PlanarDirectionalDerivative(playerDirection, PlayerInfo.PhysicsSystem.Normal).normalized;

        //Scan for enemies
        Vector3 center = 2f * playerPlanarDirection;
        Vector3 size = new Vector3(2.25f, 2, 2);
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
                /*if (distance > (hitboxScale.z / 2) && distance < (2.5f * hitboxScale.z / 2))
                {
                    charge.Clip = chargeFarDashClip;
                    act.Clip = actFarDashClip;
                }
                else
                {
                    charge.Clip = chargeFarRunClip;
                    act.Clip = actFarRunClip;
                }*/

                type = Type.FarTarget;
            }
            else
            {
                //Close target
                //PlayerInfo.Animator.SetBool("targetMatch", true);
                //charge.Clip = chargeCloseClip;
                //act.Clip = actCloseClip;

                type = Type.CloseTarget;
            }
        }
        else
        {
            //No target
            //PlayerInfo.Animator.SetBool("targetMatch", true);
            //charge.Clip = chargeNoTargetClip;
            //act.Clip = actNoTargetClip;

            type = Type.NoTarget;
        }

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

        /*
        if (PlayerInfo.StatsManager.AttackSpeedMultiplier.ModifierCount != 0)
        {
            abilitySpeed = PlayerInfo.StatsManager.AttackSpeedMultiplier.Value;
        }*/
    }

    public void HoldBegin()
    {
        holdSegmentHold.Start();
    }

    public void DuringHold()
    {
        holdSegmentHold.Update();
    }

    public void HoldEnd()
    {
        holdSegmentHold.End();
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
            Debug.DrawLine(transform.position, targetPosition, Color.red, 3f);

            dashPosition = targetPosition;

            if (CheckForGround())
                matchTarget.positionWeight = Vector3.zero;
                
            PlayerInfo.AnimationManager.StartDirectTarget(matchTarget); // not match targeting on normal calls (non continous) as there is a 
            // transition in place.
        }
        else if (type == Type.CloseTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Matho.StandardProjection3D(targetDisplacement).normalized, Vector3.up);
            matchTarget = new PlayerAnimationManager.MatchTarget(Vector3.zero, targetRotation, AvatarTarget.Root, Vector3.zero, 1);//1 / 0.25f
            charge.LoopFactor = 1;

            //PlayerInfo.AnimationManager.AnimationPhysicsEnable();
            PlayerInfo.AnimationManager.StartDirectTarget(matchTarget);

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
            PlayerInfo.AnimationManager.StartDirectTarget(matchTarget);

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
        hitboxParticles.transform.position =
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 0.5f +
            PlayerInfo.Player.transform.up * 0.125f;

        //hitboxParticles.transform.localScale = new Vector3(flipSign, 1, 1);
        UseRanDirection();

        hitboxParticles.Play();

        hitboxTimer = 0;
    }

    /*
    * Helper function to randomly generate swing direction, assigning rotation to sword particles
    * and animation to animator
    */ 
    private void GenRanDirection()
    {
        Quaternion horizontalRotation = Quaternion.identity;

        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, PlayerInfo.PhysicsSystem.Normal);

        float verticalRandom = Random.value;
        verticalSign = (verticalRandom > 0.5) ? 1 : -1;

        float rotationRandom = Random.value;
        rotationSign = (rotationRandom > 0.5) ? 0.5f : 1;

        //float flipRandom = Random.value;
        //flipSign = (flipRandom > 0.5) ? 1 : 0;
        //flipSign = (flipSign == 1) ? 0 : 1;
        if (flipSign == 1)
        {
            flipSign = 0;
        }
        else
        {
            flipSign = 1;
        }

        //flipSign = 1;
        //rotationSign = 1f;
        //verticalSign = 1;

        // Animation Sync
        if (flipSign == 1)
        {
            // Left Side
            if (rotationSign > 0.75f)
            {
                if (verticalSign == -1)
                {
                    // Top to bottom swing
                    charge.Clip = chargeNoTargetClip;
                    act.Clip = actNoTargetClip;
                    hold.Clip = holdNoTargetClip;
                }
                else
                {
                    charge.Clip = chargeNoTarget3Clip;
                    act.Clip = actNoTarget3Clip;
                    hold.Clip = holdNoTarget3Clip;
                }
            }
            else
            {
                // Top to bottom swing
                charge.Clip = chargeNoTarget2Clip;
                act.Clip = actNoTarget2Clip;
                hold.Clip = holdNoTarget2Clip;
            }
        }
        else
        {
            // Right Side
            if (rotationSign > 0.75f)
            {
                if (verticalSign * -1 == -1)
                {
                    // Top to bottom swing
                    charge.Clip = chargeNoTargetClipMirror;
                    act.Clip = actNoTargetClipMirror;
                    hold.Clip = holdNoTargetClipMirror;
                }
                else
                {
                    charge.Clip = chargeNoTarget3ClipMirror;
                    act.Clip = actNoTarget3ClipMirror;
                    hold.Clip = holdNoTarget3ClipMirror;
                }
            }
            else
            {
                charge.Clip = chargeNoTarget2ClipMirror;
                act.Clip = actNoTarget2ClipMirror;
                hold.Clip = holdNoTarget2ClipMirror;
            }
        }

        if (rotationSign < 0.75f)
            verticalSign = (flipSign == 0) ? 1 : -1;
    }

    /*
    * Need a separate method for using the random direction generated as the player may have match targeted.
    */
    private void UseRanDirection()
    {
        Quaternion horizontalRotation = Quaternion.identity;

        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, PlayerInfo.PhysicsSystem.Normal);
        hitbox.transform.rotation = normalRotation * horizontalRotation;

        float tiltRot = (rotationSign < 0.75f) ? 0.125f : 1f;
        Quaternion tiltRotation =
            Quaternion.Euler(
                180 * flipSign,
                180 * flipSign,
                45 * verticalSign * tiltRot);
        hitboxParticles.transform.rotation = normalRotation * horizontalRotation * tiltRotation;
        hitboxParticles.transform.localScale = Vector3.one;
        hitboxParticlesColors.color = hitboxParticlesNormalGradient;
    }

    public void DuringAct()
    {
        hitbox.gameObject.transform.position =
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 0.5f;
    
        hitboxTimer += Time.deltaTime;

        if (hitboxTimer > hitboxDelay && !hitbox.gameObject.activeInHierarchy)
        {
            hitbox.gameObject.SetActive(true);
            if (type == Type.CloseTarget || type == Type.FarTarget)
            {
                hitbox.Invoke(this, target);
            }
            else
            {
                hitbox.Invoke(this);
            }
            hitbox.gameObject.transform.position =
                PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 0.5f;
        }
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
        Gizmos.DrawLine(dashPosition, PlayerInfo.Player.transform.position);
        //scan
        Matrix4x4 customMatrix = Matrix4x4.TRS(scanCenter, scanRotation, Vector3.one);
        Gizmos.matrix = customMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(Vector3.zero, scanSize);

        Gizmos.matrix = Matrix4x4.identity;

        //target
        //Gizmos.color = Color.green;
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

        //if (holdSegmentHold.Held)
        //{
            enemy.TryFlinch();
        //}

        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value * directionalDamageModifier,
            holdSegmentHold.Held);

        PlayerInfo.AbilityManager.ChangeStamina(
            0.5f * PlayerInfo.StatsManager.StaminaYieldMultiplier.Value);

        enemy.Push((enemy.transform.position - PlayerInfo.Player.transform.position).normalized * knockbackStrength);

        if (castDirection == enemy.WeakDirection)
        {
            if (enemy.CheckResolve())
            {
                enemy.Push((enemy.transform.position - PlayerInfo.Player.transform.position).normalized * knockbackStrength);
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
                if (directionalDamageModifier == 1)
                    enemy.IncreaseResolve(0.5f);
            }

            abilitySpeed += 0.25f;
            if (abilitySpeed > maxSpeed)
                abilitySpeed = maxSpeed;

            enemy.ScrambleWeakDirection();
        }
        else
        {
            abilitySpeed = baseSpeed;
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