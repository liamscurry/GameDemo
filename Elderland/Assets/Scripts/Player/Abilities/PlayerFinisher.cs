using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Weapon ability with a light melee attack.

public sealed class PlayerFinisher : PlayerAbility 
{
    private enum SwingType { DiagonalRight, DiagonalLeft, ForwardRight }

    //Fields
    private AnimationClip chargeClip1;
    private AnimationClip actClip1;

    private AnimationClip chargeClip2;
    private AnimationClip actClip2;

    private AnimationClip chargeClip3;
    private AnimationClip actClip3;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilityProcess actPauseProcess;
    private AbilityProcess actLeaveProcess; // used for resetting camera settings
    private AbilitySegment charge;
    private AbilitySegment act;

    private Vector2 playerDirection;
    private Vector3 playerPlanarDirection;
    private Collider target;
    private Vector3 targetDisplacement;
    private Vector3 targetPlanarDirection;
    private float targetWidth;
    private float targetHorizontalDistance;
    private Vector3 dashPosition;

    private float obstructionCheckMargin = 0.25f;
    private float endPositionOffset = 0.5f / 3;
    private const float finisherHealthMargin = 0.1f;

    private PlayerAnimationManager.MatchTarget matchTarget;
    private bool interuptedTarget;

    private const float knockbackStrength = 7f;

    private SwingType swingType;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        //Animation assignment
        chargeClip1 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherCharge1);
        actClip1 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherAct1);

        chargeClip2 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherCharge2);
        actClip2 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherAct2);

        chargeClip3 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherCharge3);
        actClip3 =
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.FinisherAct3);

        chargeProcess = new AbilityProcess(ChargeBegin, null, null, 1);
        actProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.15f);
        actPauseProcess = new AbilityProcess(ActLeaveBegin, null, ActLeaveEnd, 0.3f);
        actLeaveProcess = new AbilityProcess(ActLeaveBegin, null, ActLeaveEnd, 1 - (0.15f + 0.3f));
        charge = new AbilitySegment(null, chargeProcess);
        charge.Type = AbilitySegmentType.RootMotion;
        act = new AbilitySegment(null, actProcess, actPauseProcess, actLeaveProcess);
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
                Resources.Load<GameObject>(
                    ResourceConstants.Player.Hitboxes.RectangularMultiHitbox),
                    transform.position,
                    Quaternion.identity);
        hitboxObject.transform.parent = PlayerInfo.MeleeObjects.transform;
        hitboxObject.SetActive(false);
    }

    protected override bool WaitCondition()
    {
        if (!PlayerInfo.AbilityManager.InCombatStance)
        {
            return false;
        }

        //Scan for enemies
        Collider[] hitboxColliders =
            Physics.OverlapSphere(PlayerInfo.Player.transform.position, 6, LayerConstants.Hitbox);

        List<Collider> enemyColliders = new List<Collider>();
        foreach (Collider collider in hitboxColliders)
        {
            if (collider.tag == TagConstants.EnemyHitbox && collider.gameObject.activeSelf)
                enemyColliders.Add(collider);
        }

        if (enemyColliders.Count == 0)
        {
            return false;
        }
        else
        {
            //Locate closest enemy
            float minDistance = 0;
            Collider minCollider = null;
            for (int i = 0; i < enemyColliders.Count; i++)
            {
                float distance =
                    Vector3.Distance(enemyColliders[i].transform.position, PlayerInfo.Player.transform.position);
                Vector3 enemyOffset = 
                    enemyColliders[i].transform.position - PlayerInfo.Player.transform.position;
                enemyOffset = Matho.StdProj3D(enemyOffset);
                Vector3 projectedCameraForward = 
                    Matho.StdProj3D(GameInfo.CameraController.transform.forward);
                float cameraForwardEnemyAngle =
                    Matho.AngleBetween(enemyOffset, projectedCameraForward);
                
                if (((minCollider != null && distance < minDistance) ||
                    (minCollider == null)) &&
                    cameraForwardEnemyAngle < 90f && 
                    IsEnemyInFinisherState(enemyColliders[i]) &&
                    !IsEnemyObstructed(enemyColliders[i]))
                {
                    minDistance = distance;
                    minCollider = enemyColliders[i];
                }
            }

            if (minCollider != null)
            {
                CalculateEnemyPositionalInfo(minCollider);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private bool IsEnemyInFinisherState(Collider other)
    {
        EnemyManager enemyManager = 
            other.GetComponentInParent<EnemyManager>();
        
        return enemyManager.Health < enemyManager.FinisherHealth ||
               Matho.IsInRange(enemyManager.Health, enemyManager.FinisherHealth, finisherHealthMargin);
    }

    private bool IsEnemyObstructed(Collider other)
    {
        NavMeshHit navMeshHit;
        Vector3 endPosition = 
            Vector3.MoveTowards(
                PlayerInfo.Player.transform.position,
                other.transform.parent.position,
                obstructionCheckMargin);

        return other.GetComponentInParent<NavMeshAgent>().Raycast(
                endPosition,
                out navMeshHit);
    }

    private void CalculateEnemyPositionalInfo(Collider minCollider)
    {
        //Target info
        target = minCollider;
        targetDisplacement =
            (minCollider.transform.parent.position - PlayerInfo.Player.transform.position);
        targetPlanarDirection =
            Matho.PlanarDirectionalDerivative(
                Matho.StdProj2D(targetDisplacement).normalized,
                PlayerInfo.PhysicsSystem.Normal).normalized;

        float theta = Matho.AngleBetween(targetDisplacement, targetPlanarDirection);
        targetHorizontalDistance = targetDisplacement.magnitude * Mathf.Cos(theta * Mathf.Deg2Rad);

        Ray enemyRay = new Ray(PlayerInfo.Player.transform.position, targetPlanarDirection);
        RaycastHit enemyHit;
        target.Raycast(enemyRay, out enemyHit, targetHorizontalDistance);
        targetWidth = targetDisplacement.magnitude - enemyHit.distance;
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
                float height =
                    PlayerInfo.Player.transform.position.y -
                    (raycast.distance) +
                    (PlayerInfo.Capsule.height / 2 -
                    PlayerInfo.Capsule.radius);
                
                PlayerInfo.Player.transform.position =
                    new Vector3(PlayerInfo.Player.transform.position.x, height, PlayerInfo.Player.transform.position.z);
            }

            if (!interuptedTarget && PlayerInfo.Animator.isMatchingTarget)
            {
                if (CheckForGround())
                {
                    interuptedTarget = true;
                    PlayerInfo.Animator.InterruptMatchTarget(false);
                    matchTarget.positionWeight = Vector3.zero;
                    PlayerInfo.AnimationManager.StartDirectTarget(matchTarget, true);
                }
            }   
        }
    }

    private bool CheckForGround()
    {
        Vector3 interuptDirection = (dashPosition - PlayerInfo.Player.transform.position).normalized;

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
        if (target != null)
        {
            Vector3 enemyDirection = 
                -(target.transform.position - transform.position);
            enemyDirection = Matho.Rotate(enemyDirection, Vector3.up, -30f);
            GameInfo.CameraController.TargetDirection = enemyDirection;
            GameInfo.CameraController.TargetZoom = 1;
            GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -10, 0.8f));
        }

        SelectRandomSwing();
    }

    private void SelectRandomSwing()
    {
        int swingRandom = (new System.Random().Next() % 2) + 1;
        switch (swingRandom)
        {
            case 1:
                charge.Clip = chargeClip1;
                act.Clip = actClip1;
                swingType = SwingType.DiagonalLeft;
                break;
            case 2:
                charge.Clip = chargeClip2;
                act.Clip = actClip2;
                swingType = SwingType.DiagonalRight;
                break;
            case 3:
                charge.Clip = chargeClip3;
                act.Clip = actClip3;
                swingType = SwingType.ForwardRight;
                break;
        }
    }

    public void ChargeBegin()
    {
        RaycastHit directionHit;
        float offset = PlayerInfo.Capsule.radius + targetWidth + endPositionOffset;
        bool hit =
            Physics.CapsuleCast(
                PlayerInfo.Capsule.TopSpherePosition(),
                PlayerInfo.Capsule.BottomSpherePosition(),
                PlayerInfo.Capsule.radius - 0.05f,
                targetPlanarDirection,
                out directionHit,
                targetHorizontalDistance - offset,
                LayerConstants.GroundCollision | LayerConstants.Destructable);

        float targetDistance =
            (hit) ?
            directionHit.distance :
            targetHorizontalDistance - offset;

        Vector3 targetPosition = PlayerInfo.Player.transform.position + targetDistance * targetPlanarDirection;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                Matho.StdProj3D(targetDisplacement).normalized,
                Vector3.up);

        matchTarget =
            new PlayerAnimationManager.MatchTarget(
                targetPosition,
                targetRotation,
                AvatarTarget.Root,
                Vector3.one,
                1);

        charge.LoopFactor = 1;
        PlayerInfo.AnimationManager.StartDirectTarget(matchTarget, true);

        dashPosition = targetPosition;
    }

    public void ActBegin()
    {
        Quaternion horizontalRotation;
        Quaternion normalRotation;
        PlayerInfo.AbilityManager.GenerateHitboxRotations(
            out horizontalRotation,
            out normalRotation);

        Quaternion tiltRotation = TiltFromSwing();

        PlayerInfo.AbilityManager.AlignSwordParticles(
            normalRotation,
            horizontalRotation,
            tiltRotation);

        PlayerInfo.AbilityManager.SwordParticles.Play();
    }

    private Quaternion TiltFromSwing()
    {
        switch (swingType)
        {
            case SwingType.DiagonalLeft:
                return Quaternion.Euler(0, 0, -55 - 180);
            case SwingType.DiagonalRight:
                return Quaternion.Euler(0, 0, 55);
            case SwingType.ForwardRight:
                return Quaternion.Euler(0, 0, -187);
        }   
        
        throw new System.Exception("Swing type on finisher not implemented");
    }

    public void ActEnd()
    {
        //hitbox.gameObject.SetActive(false);
        PlayerInfo.MovementManager.TargetDirection = Matho.StdProj2D(PlayerInfo.Player.transform.forward).normalized;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.ZeroSpeed();

        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        PlayerInfo.Animator.SetFloat("speed", 0);

        if (target != null && target.gameObject.activeSelf)
        {
            EnemyManager enemy =
                target.GetComponentInParent<EnemyManager>();

            enemy.ChangeHealth(
                -Mathf.Infinity);

            Pickup.SpawnPickups<HealthPickup>(
                Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
                enemy.transform.position,
                4,
                2f,
                90f);

            Vector3 enemyPushDir =
                (enemy.transform.position - PlayerInfo.Player.transform.position).normalized;
            enemy.Push(enemyPushDir * knockbackStrength);
        }
    }

    public void ActLeaveBegin()
    {
        GameInfo.CameraController.TargetDirection = -targetDisplacement;
    }

    public void ActLeaveEnd()
    {
        GameInfo.CameraController.TargetDirection = Vector3.zero;
        GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0, 0));
    }

    public override bool OnHit(GameObject character)
    {
        throw new System.Exception("The player finisher ability does not use a hitbox");
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
        PlayerInfo.Animator.InterruptMatchTarget(false);
    }
}