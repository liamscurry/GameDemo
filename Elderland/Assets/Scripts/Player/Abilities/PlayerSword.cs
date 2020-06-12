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

    private float damage = 1;
    private float strength = 18;
    private PlayerSingleDamageHitbox hitbox;
    private Vector3 hitboxScale = new Vector3(1.5f, 2, 2);

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

        chargeProcess = new AbilityProcess(ChargeBegin, null, ChargeEnd, 1);
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
        GameObject hitboxObject = Instantiate(Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.RectangularSingleHitbox), transform.position, Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxObject.SetActive(false);

        hitbox = hitboxObject.GetComponent<PlayerSingleDamageHitbox>();
        hitbox.gameObject.transform.localScale = hitboxScale;

        scanRotation = Quaternion.identity;        
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
                if (!CheckForGround())
                {
                    interuptedTarget = true;
                    PlayerInfo.Animator.InterruptMatchTarget(false);
                    matchTarget.positionWeight = Vector3.zero;
                    PlayerInfo.AnimationManager.StartTargetImmediately(matchTarget);
                }
            }   
        }
    }

    /*
    bool hitLedge = UnityEngine.Physics.SphereCast(
    PlayerInfo.Player.transform.position + interuptDirection * 1.25f,
    PlayerInfo.Capsule.radius,
    Vector3.down,
    out predictLedgeCast,
    (PlayerInfo.Capsule.height / 2) + PlayerInfo.Capsule.radius * 0.5f,
    LayerConstants.GroundCollision);
    */

    private bool CheckForGround()
    {
        RaycastHit predictLedgeCastClose;
        RaycastHit predictLedgeCastFar;
        Vector3 interuptDirection = Matho.StandardProjection3D(dashPosition - PlayerInfo.Player.transform.position).normalized;

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

        if (!hitLedgeClose || !hitLedgeFar)
        {
            Collider[] overlapColliders = UnityEngine.Physics.OverlapSphere(
                PlayerInfo.Player.transform.position + PlayerInfo.Capsule.BottomSphereOffset() + interuptDirection * 0.6f,
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
            matchTarget = new PlayerAnimationManager.MatchTarget(targetPosition, targetRotation, AvatarTarget.Root, new Vector3(1, 0, 1), 1 / 0.25f);
            charge.LoopFactor = 1;
            //PlayerInfo.AnimationManager.AnimationPhysicsEnable();

            dashPosition = targetPosition;

            if (!CheckForGround())
                matchTarget.positionWeight = Vector3.zero;
                
            PlayerInfo.AnimationManager.StartTarget(matchTarget);
        }
        else if (type == Type.CloseTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Matho.StandardProjection3D(targetDisplacement).normalized, Vector3.up);
            matchTarget = new PlayerAnimationManager.MatchTarget(Vector3.zero, targetRotation, AvatarTarget.Root, Vector3.zero, 1 / 0.25f);
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

    public void ChargeEnd()
    {
        //print("charge end called");
        //PlayerInfo.AnimationManager.DisableRootMotion();
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

        Quaternion horizontalRotation = Quaternion.identity;

        horizontalRotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), PlayerInfo.Player.transform.forward);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, PlayerInfo.PhysicsSystem.Normal);
        hitbox.transform.rotation = normalRotation * horizontalRotation;
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

    /*
    protected override void Stop()
    {
        hitbox.gameObject.SetActive(false);
    }
    */

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
        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value);
        PlayerInfo.AbilityManager.ChangeStamina(
            0.5f * PlayerInfo.StatsManager.StaminaYieldMultiplier.Value);
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
        PlayerInfo.Animator.InterruptMatchTarget(false);
    }

    /*
    private class MatchTarget
    {
        public Vector3 position;
        public Quaternion rotation;
        public AvatarTarget part;
        public MatchTargetWeightMask weightMask;
        public float startTime;
        public float endTime;

        public MatchTarget(Vector3 position, Quaternion rotation, AvatarTarget part, MatchTargetWeightMask weightMask, float startTime, float endTime)
        {
            this.position = position;
            this.rotation = rotation;
            this.part = part;
            this.weightMask = weightMask;
            this.startTime = startTime;
            this.endTime = endTime;
        }
    }
    */
}