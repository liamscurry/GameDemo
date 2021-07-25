using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Weapon ability with a light melee attack.

public sealed class PlayerSword : PlayerAbility 
{
    //Fields
    private AnimationClip chargeNoTargetClip;
    private AnimationClip actNoTargetClip;
    private AnimationClip chargeNoTargetClipMirror;
    private AnimationClip actNoTargetClipMirror;

    private AnimationClip chargeNoTarget2Clip;
    private AnimationClip actNoTarget2Clip;
    private AnimationClip chargeNoTarget2ClipMirror;
    private AnimationClip actNoTarget2ClipMirror;

    private AnimationClip chargeNoTarget3Clip;
    private AnimationClip actNoTarget3Clip;
    private AnimationClip chargeNoTarget3ClipMirror;
    private AnimationClip actNoTarget3ClipMirror;

    private float damage = 0.5f;
    private float strength = 18;
    private float knockbackStrength = 3.5f;
    private PlayerMultiDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(3f, 2, 2);
    private const float maxSpeedOnExit = 5f;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilitySegment charge;
    private AbilitySegment act;

    private enum Type { NoTarget, FarTarget, CloseTarget, NotCalculated }
    private Type type;

    private Vector2 playerDirection;
    private Vector3 playerPlanarDirection;
    private Collider target;
    private Vector3 targetDisplacement;
    private Vector3 targetPlanarDirection;
    private float targetWidth;
    private float targetHorizontalDistance;

    // Movement info
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector2 projStartPos;
    private Vector2 projTargetPos;
    private bool calculatedTargetInfo;

    private const float targetSlowRatio = 1.2f;
    private const float targetSlowRadius = 1f;
    private const float targetNearRadius = 0.2f;

    // Rotate info
    private Quaternion startRotation;
    private const float rotateSpeed = 30f;

    private bool leftGround;

    private PlayerAnimationManager.MatchTarget matchTarget;

    private int castDirection;
    private float baseSpeed = 1f;
    private float maxSpeed = 1.5f;
    private float hitTime;
    private const float resetHitTime = 3.5f;

    // Swing Direction
    private int flipSign;
    private float rotationSign;
    private int verticalSign;

    private float hitboxTimer;
    private const float hitboxDelay = 0.05f;

    private const float noTargetDistance = 1.5f;
    private const float noTargetSpeed = 10f;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        //Animation assignment
        chargeNoTargetClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Charge);
        actNoTargetClip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1Act);
        chargeNoTargetClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1MirrorCharge);
        actNoTargetClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword1MirrorAct);

        chargeNoTarget2Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Charge);
        actNoTarget2Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2Act);
        chargeNoTarget2ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2MirrorCharge);
        actNoTarget2ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword2MirrorAct);

        chargeNoTarget3Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Charge);
        actNoTarget3Clip =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3Act);
        chargeNoTarget3ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3MirrorCharge);
        actNoTarget3ClipMirror =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.LightSword3MirrorAct);

        chargeProcess = new AbilityProcess(ChargeBegin, DuringCharge, ChargeEnd, 1);
        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1f);
        charge = new AbilitySegment(null, chargeProcess);
        act = new AbilitySegment(null, actProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;
        continous = true;

        //Hitbox initializations
        GameObject hitboxObject =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularMultiHitbox),
                transform.position,
                Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxObject.SetActive(false);

        hitbox = hitboxObject.GetComponent<PlayerMultiDamageHitbox>();
        hitbox.gameObject.transform.localScale = hitboxScale;

        scanRotation = Quaternion.identity;   
    }

    public override bool Wait(bool firstTimeCalling)
    {
        bool success = base.Wait(firstTimeCalling);
        return success;
    }

    protected override void GlobalStart()
    {
        GenRanDirection();
          
        playerDirection = (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.5f) ?
            GameInfo.CameraController.StdToCameraDir(GameInfo.Settings.LeftDirectionalInput) :
            Matho.StdProj2D(GameInfo.CameraController.Direction);

        playerPlanarDirection =
            Matho.PlanarDirectionalDerivative(playerDirection, PlayerInfo.CharMoveSystem.GroundNormal).normalized;

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
                float currentDistance = Vector3.Distance(enemyColliders[i].transform.position, PlayerInfo.Player.transform.position);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    minCollider = enemyColliders[i];
                }
            }

            //Target info
            target = minCollider;
            Vector3 targetDirection = 
                Matho.StdProj3D(target.transform.position - PlayerInfo.Player.transform.position);
            float targetWidth = target.bounds.extents.x;
            float distance = targetDirection.magnitude - PlayerInfo.Capsule.radius - targetWidth;
            if (distance > hitboxScale.z / 2)
            {
                type = Type.FarTarget;
                Debug.Log("far target");
            }
            else
            {
                type = Type.CloseTarget;
                Debug.Log("close target");
            }
        }
        else
        {
            type = Type.NoTarget;
            Debug.Log("no target");
        }

        calculatedTargetInfo = false;
        leftGround = false;
    }

    public override void GlobalConstantUpdate()
    {
        if (!PlayerInfo.CharMoveSystem.Grounded)
            leftGround = true;

        if (calculatedTargetInfo && !leftGround)
        {
            MoveTowardsGoal();
            RotateTowardsGoal();
        }
    }

    public void ChargeBegin()
    {
        if (type == Type.NoTarget)
        {
            NoTargetDirectTarget();
        }
        else if (type == Type.CloseTarget)
        {
            CloseTargetDirectTarget();
        }
        else if (type == Type.FarTarget)
        {
            FarTargetDirectTarget();
        }

        calculatedTargetInfo = true;

        startRotation = PlayerInfo.Player.transform.rotation;

        PlayerInfo.CharMoveSystem.MaxConstantOnExit.ClaimLock(this, maxSpeedOnExit);
    }

    /*
    Generates target position and rotation for when there are no nearby enemies.

    Inputs:
    None

    Outputs:
    None
    */
    private void NoTargetDirectTarget()
    {
        Vector2 worldAnalogInput2D = 
            GameInfo.CameraController.StdToCameraDir(GameInfo.Settings.LeftDirectionalInput);
        Vector3 targetDirection = 
            (GameInfo.Settings.LeftDirectionalInput.magnitude < PlayerAbilityManager.deadzone) ?
            GameInfo.CameraController.Direction :
            new Vector3(worldAnalogInput2D.x, 0, worldAnalogInput2D.y);

        targetPosition =
            PlayerInfo.Player.transform.position +
            Matho.StdProj3D(targetDirection) * noTargetDistance;

        startPosition = PlayerInfo.Player.transform.position;

        projStartPos = Matho.StdProj2D(startPosition);
        projTargetPos = Matho.StdProj2D(targetPosition);
        
        charge.LoopFactor = 1;

        //dashPosition = targetPosition;
    }

    /*
    Generates target rotation for when there is a close enemy found. Does not have a target position
    as the player is already close enough to the enemy.

    Inputs:
    None

    Outputs:
    None
    */
    private void CloseTargetDirectTarget()
    {
        Vector3 targetDirection = 
            Matho.StdProj3D(target.transform.position - PlayerInfo.Player.transform.position).normalized;

        charge.LoopFactor = 1;

        //dashPosition = PlayerInfo.Player.transform.position;
        targetPosition =
            PlayerInfo.Player.transform.position +
            Matho.StdProj3D(targetDirection) * noTargetDistance;

        startPosition = PlayerInfo.Player.transform.position;

        projStartPos = Matho.StdProj2D(startPosition);
        projTargetPos = Matho.StdProj2D(targetPosition); 
    }

    /*
    Generates target position and rotation for when there is a far enemy found.

    Assumptions:
    Enemy colliders are cylindrical.

    Inputs:
    None

    Outputs:
    None
    */
    private void FarTargetDirectTarget()
    {
        Vector3 targetDirection = 
            Matho.StdProj3D(target.transform.position - PlayerInfo.Player.transform.position);

        float targetWidth = target.bounds.extents.x;
        float distance = targetDirection.magnitude - PlayerInfo.Capsule.radius - targetWidth;

        targetDirection.Normalize();

        targetPosition =
            PlayerInfo.Player.transform.position +
            targetDirection * distance;

        startPosition = PlayerInfo.Player.transform.position;

        projStartPos = Matho.StdProj2D(startPosition);
        projTargetPos = Matho.StdProj2D(targetPosition); 
        
        // Needed to account for variable distance to enemy, may increase number of times going
        // through state timer.
        float loopFactor = 0;
        if (distance > (hitboxScale.z / 2) && distance < (2.5f * hitboxScale.z / 2))
        {
            loopFactor = 1;
        }
        else
        {
            loopFactor = distance / (2.5f * (hitboxScale.z / 2));
        }

        charge.LoopFactor = loopFactor;
    }

    public void DuringCharge()
    {

    }

    public void ChargeEnd()
    {

    }

    public void ActBegin()
    {
        Quaternion horizontalRotation;
        Quaternion normalRotation;
        PlayerInfo.AbilityManager.GenerateHitboxRotations(
            out horizontalRotation,
            out normalRotation);
            
        hitbox.transform.rotation = normalRotation * horizontalRotation;

        float tiltRot = (rotationSign < 0.75f) ? 0.125f : 1f;
        Quaternion tiltRotation =
            Quaternion.Euler(
                180 * flipSign,
                180 * flipSign,
                45 * verticalSign * tiltRot);

        PlayerInfo.AbilityManager.AlignSwordParticles(
            normalRotation,
            horizontalRotation,
            tiltRotation);

        PlayerInfo.AbilityManager.SwordParticles.Play();

        hitboxTimer = 0;
    }

    /*
    Moves the player using the char move system to the target goal found earlier. Does not overshoot.

    Inputs:
    None

    Outputs:
    None

    Bugs fixed: 
    - Player stops after charging. Solution: lowered duration of transition to abilities. It
      was hanging as it was waiting for transition to end.
    */
    private void MoveTowardsGoal()
    {
        if (type == Type.NoTarget || type == Type.FarTarget)
        {   
            Vector2 direction = 
                Matho.StdProj2D(targetPosition - PlayerInfo.Player.transform.position);

            float slowPercent = (targetSlowRadius - direction.magnitude) / targetSlowRadius;
            float dampedSpeed =
                (direction.magnitude > targetSlowRadius) ?
                noTargetSpeed :
                Mathf.Clamp(noTargetSpeed - slowPercent * (noTargetSpeed * targetSlowRatio), 0, float.MaxValue);

            direction.Normalize();

            PlayerInfo.CharMoveSystem.GroundMove(direction * dampedSpeed);
        }

        if (Physics.OverlapSphere(
            PlayerInfo.Player.transform.position,
            PlayerInfo.Capsule.radius * 1.75f,
            LayerConstants.Enemy).Length != 0)
        {
            ForceAdvanceSegment();
        }
    }
    
    /*
    Rotates the player during charge in order to align the sword direction towards targets

    Inputs:
    None

    Outputs:
    None
    */
    private void RotateTowardsGoal()
    {
        Vector3 targetLook =
            Matho.StdProj3D(targetPosition - PlayerInfo.Player.transform.position).normalized;
        
        if (targetLook.magnitude > targetNearRadius)
        {
            Vector3 interLook = 
                Vector3.RotateTowards(
                    PlayerInfo.Player.transform.forward,
                    targetLook,
                    rotateSpeed * Time.deltaTime,
                    float.MaxValue);
            PlayerInfo.Player.transform.rotation =
                Quaternion.LookRotation(
                    interLook,
                    Vector3.up);
        }
    }

    /*
    * Helper function to randomly generate swing direction, assigning rotation to sword particles
    * and animation to animator
    */ 
    private void GenRanDirection()
    {
        Quaternion horizontalRotation = Quaternion.identity;

        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);

        Quaternion normalRotation =
            Quaternion.FromToRotation(Vector3.up, PlayerInfo.CharMoveSystem.GroundNormal);

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
                }
                else
                {
                    charge.Clip = chargeNoTarget3Clip;
                    act.Clip = actNoTarget3Clip;
                }
            }
            else
            {
                // Top to bottom swing
                charge.Clip = chargeNoTarget2Clip;
                act.Clip = actNoTarget2Clip;
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
                }
                else
                {
                    charge.Clip = chargeNoTarget3ClipMirror;
                    act.Clip = actNoTarget3ClipMirror;
                }
            }
            else
            {
                charge.Clip = chargeNoTarget2ClipMirror;
                act.Clip = actNoTarget2ClipMirror;
            }
        }

        if (rotationSign < 0.75f)
            verticalSign = (flipSign == 0) ? 1 : -1;
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
        PlayerInfo.MovementManager.TargetDirection = Matho.StdProj2D(PlayerInfo.Player.transform.forward).normalized;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.ZeroSpeed();

        /*
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
        */

        PlayerInfo.AbilityManager.LastDirFocus = Time.time;
        PlayerInfo.AbilityManager.DirFocus = PlayerInfo.Player.transform.forward;

        PlayerInfo.CharMoveSystem.MaxConstantOnExit.TryReleaseLock(this, float.MaxValue);
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
        
        if (enemy.Health > enemy.ZeroHealth)
        {
            int shiftedSign = (flipSign == 1) ? -1 : 1;
            enemy.TryFlinch(shiftedSign);
        }

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
            }
            else
            {
                if (directionalDamageModifier == 1)
                    enemy.IncreaseResolve(0.5f);
            }

            enemy.ScrambleWeakDirection();
        }

        hitTime = Time.time;

        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
        PlayerInfo.Animator.InterruptMatchTarget(false);
    }

    Vector3 scanCenter;
    Vector3 scanSize;
    Quaternion scanRotation;
    Vector3 dashPosition;
    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(dashPosition, PlayerInfo.Player.transform.position);
        //scan
        Matrix4x4 customMatrix = Matrix4x4.TRS(scanCenter, scanRotation, Vector3.one);
        Gizmos.matrix = customMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(Vector3.zero, scanSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
    */

    /*
    // Inside of OnHit. Used to be used for changing swing particles intensity.
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
    */
}