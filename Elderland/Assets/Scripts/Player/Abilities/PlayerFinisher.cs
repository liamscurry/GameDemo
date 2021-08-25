using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Weapon ability with a light melee attack.

public sealed class PlayerFinisher : PlayerAbility 
{
    private enum SwingType { DiagonalLeft, DiagonalRight, ForwardRight }

    private float damage = 0.5f;
    private float strength = 18;
    private const float knockbackStrength = 3f;
    private const float maxSpeedOnExit = 5f;

    private AbilityProcess chargeProcess;
    private AbilityProcess actProcess;
    private AbilityProcess actLeaveProcess;
    private AbilitySegment charge;
    private AbilitySegment act;

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
    private const float rotateSpeed = 40f;

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

    private const float noTargetDistance = 1.5f;
    private const float noTargetSpeed = 10f;

    // Animation Clips
    private AnimationClip chargeClip1;
    private AnimationClip actClip1;

    private AnimationClip chargeClip2;
    private AnimationClip actClip2;

    private AnimationClip chargeClip3;
    private AnimationClip actClip3;

    private SwingType swingType;

    private const float finisherHealthMargin = 0.1f;
    private const float zeroHealthMargin = 0.001f;

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
        actProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.25f);
        actLeaveProcess = new AbilityProcess(ActLeaveBegin, null, ActLeaveEnd, 0.75f);
        charge = new AbilitySegment(chargeClip1, chargeProcess);
        act = new AbilitySegment(actClip1, actProcess, actLeaveProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(charge);
        segments.AddSegment(act);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;
        continous = true;

        scanRotation = Quaternion.identity;   
    }

    protected override bool WaitCondition()
    {
        return FinisherEnemyNearby();
    }

    private bool FinisherEnemyNearby()
    {
        playerDirection = 
            Matho.StdProj2D(GameInfo.CameraController.Direction);

        playerPlanarDirection =
            Matho.PlanarDirectionalDerivative(playerDirection, PlayerInfo.CharMoveSystem.GroundNormal).normalized;

        //Scan for enemies
        Vector3 center = 2f * playerPlanarDirection;
        Vector3 size = new Vector3(2.25f, 2, 2);
        Quaternion rotation =
            Quaternion.FromToRotation(Vector3.right, playerPlanarDirection);
        Collider[] hitboxColliders =
            Physics.OverlapBox(PlayerInfo.Player.transform.position + center, size / 2, rotation, LayerConstants.Hitbox);

        //Draw parameters   
        scanCenter = PlayerInfo.Player.transform.position + center;
        scanSize = size;
        scanRotation = rotation;

        List<Collider> enemyColliders = new List<Collider>();
        foreach (Collider collider in hitboxColliders)
        {
            if (collider.tag == TagConstants.EnemyHitbox &&
                IsEnemyInFinisherState(collider) &&
                !EnemyInfo.IsEnemyObstructed(collider))
            {
                enemyColliders.Add(collider);
            }
        }

        if (enemyColliders.Count > 0)
        {
            //Locate closest enemy
            float minDistance =
                Vector3.Distance(enemyColliders[0].transform.position, PlayerInfo.Player.transform.position);
            Collider minCollider = enemyColliders[0];
            for (int i = 1; i < enemyColliders.Count; i++)
            {
                float currentDistance =
                    Vector3.Distance(enemyColliders[i].transform.position, PlayerInfo.Player.transform.position);
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

            calculatedTargetInfo = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsEnemyInFinisherState(Collider other)
    {
        EnemyManager enemyManager = 
            other.GetComponentInParent<EnemyManager>();
        
        return (enemyManager.Health < enemyManager.FinisherHealth ||
               Matho.IsInRange(enemyManager.Health, enemyManager.FinisherHealth, finisherHealthMargin)) &&
               !Matho.IsInRange(enemyManager.Health, 0, zeroHealthMargin);
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
    
    protected override void GlobalStart()
    {
        if (target != null)
        {
            Vector3 enemyDirection = 
                -(target.transform.position - transform.position);
            enemyDirection = Matho.Rotate(enemyDirection, Vector3.up, -30f);
            targetDisplacement = enemyDirection;
            GameInfo.CameraController.TargetDirection = enemyDirection;
            GameInfo.CameraController.TargetZoom = 1;
            GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -10, 0.8f, 1f));
        }

        SelectRandomSwing();
        leftGround = false;
    }

    public override void GlobalConstantUpdate()
    {
        if (!PlayerInfo.CharMoveSystem.Grounded)
            leftGround = true;

        if (target != null && calculatedTargetInfo && !leftGround)
        {
            MoveTowardsGoal();
            RotateTowardsGoal();
        }
    }

    public void ChargeBegin()
    {
        FarTargetDirectTarget();

        calculatedTargetInfo = true;

        startRotation = PlayerInfo.Player.transform.rotation;

        PlayerInfo.CharMoveSystem.MaxConstantOnExit.ClaimLock(this, maxSpeedOnExit);
        PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
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
        if (target != null)
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
        }
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
        if (!(Physics.OverlapSphere(
            PlayerInfo.Player.transform.position,
            PlayerInfo.Capsule.radius * 1.75f,
            LayerConstants.Enemy).Length != 0))
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
            Matho.StdProj3D(target.transform.position - PlayerInfo.Player.transform.position).normalized;
        
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
        GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0, 0, 0));
    }

    public void ActLeaveEnd()
    {
        GameInfo.CameraController.TargetDirection = Vector3.zero;

        target = null;

        PlayerInfo.MovementManager.TargetDirection =
            Matho.StdProj2D(PlayerInfo.Player.transform.forward).normalized;
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.ZeroSpeed();

        PlayerInfo.CharMoveSystem.MaxConstantOnExit.TryReleaseLock(this, float.MaxValue);
        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(this, false);
    }

    public override bool OnHit(GameObject character)
    {
        throw new System.Exception("The player finisher ability does not use a hitbox");
    }

    public override void ShortCircuitLogic()
    {
        ActLeaveEnd();
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
}