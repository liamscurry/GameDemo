using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LightEnemyAttackFollow : StateMachineBehaviour
{
    private LightEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private bool rechoseAbility;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    private const float rotateDistance = 3;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<LightEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
        rechoseAbility = false;
        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;

        manager.ArrangmentRadius = manager.NextAttack.AttackDistance;
        manager.TurnOnAgent();
        manager.PrimeAgentPath();
        if (manager.ArrangementNode != -1)
        {
            manager.CalculateAgentPath();
        }
        else
        {
            manager.CancelAgentPath();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();
            remainingDistance = manager.Agent.remainingDistance;

            if (manager.ArrangementNode != -1)
            {
                if (EnemyInfo.MeleeArranger.OverrideNode(manager))
                {
                    manager.CalculateAgentPath();
                }
                
                if (checkTimer >= checkDuration)
                {
                    EnemyInfo.MeleeArranger.ClearNode(manager.ArrangementNode);
                    EnemyInfo.MeleeArranger.ClaimNode(manager);
                    manager.CalculateAgentPath();
                }

                manager.FollowAgentPath();
                RotateTowardsPlayer();
            }
            else
            {
                if (checkTimer >= checkDuration)
                {
                    EnemyInfo.MeleeArranger.ClaimNode(manager);
                }
            }

            ClampToGround();

            RechooseAbilityCheck();
            
            if (manager.ArrangementNode != -1)
                StopTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void ClampToGround()
    {
        RaycastHit raycast;

        Vector3 agentCenter = manager.Agent.nextPosition + (-manager.Agent.baseOffset + manager.Agent.height / 2) * Vector3.up;

        bool hit = UnityEngine.Physics.SphereCast(
            agentCenter,
            manager.Capsule.radius,
            Vector3.down,
            out raycast,
            (manager.Capsule.height / 2) + manager.Capsule.radius,
            LayerConstants.GroundCollision);

        if (hit)
        {
            float verticalOffset = 1 - (raycast.distance - (manager.Capsule.height / 2 - manager.Capsule.radius));
            manager.Agent.baseOffset = verticalOffset;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (manager.Agent.hasPath)
        {
            if (manager.towardsPlayer)
            {
                if (lastRemainingDistance < rotateDistance && remainingDistance >= rotateDistance)
                {
                    manager.Agent.updateRotation = true;
                }
                else if (remainingDistance < rotateDistance && lastRemainingDistance >= rotateDistance)
                {
                    manager.Agent.updateRotation = false;
                }

                if (remainingDistance < rotateDistance)
                {
                    Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                    Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1f * Time.deltaTime, 0f);
                    manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                }
            }
            else
            {
                manager.Agent.updateRotation = false;
                Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1f * Time.deltaTime, 0f);
                manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }

    private void StopTransition()
    {
        Vector2 castPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        castPosition = Vector2.MoveTowards(castPosition, Matho.StandardProjection2D(manager.transform.position), 1f);
        Vector3 playerNav = GameInfo.CurrentLevel.NavCast(castPosition);
        NavMeshHit groundHit;
        
        Vector3 direction = PlayerInfo.Player.transform.position - manager.transform.position;
        float distance = direction.magnitude;
        RaycastHit enemyHit;

        if ((manager.NextAttack == manager.Sword && manager.completedWaypoints && manager.IsInNextAttackMax() ||
            (manager.NextAttack == manager.Charge && 
            manager.IsInNextAttackMax() &&
            !manager.Agent.Raycast(playerNav, out groundHit) &&
            !Physics.SphereCast(manager.transform.position, manager.Capsule.radius - 0.1f, direction, out enemyHit, distance, LayerConstants.Enemy))))
        {
            Vector3 playerEnemyDirection = (PlayerInfo.Player.transform.position - manager.transform.position).normalized;
            float playerEnemyAngle =
                Matho.AngleBetween(
                    Matho.StandardProjection2D(manager.transform.forward),
                    Matho.StandardProjection2D(playerEnemyDirection));

            if (playerEnemyAngle < manager.NextAttack.AttackAngleMargin)
            {
                AttackExit();
            }
            else
            {
                StationaryExit();
            }
        }
    }

    private void AttackExit()
    {
        manager.TurnOffAgent();
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        exiting = true;
    }

    private void StationaryExit()
    {
        manager.Agent.ResetPath();
        manager.Animator.SetTrigger("toStationary");
        exiting = true;
    }

    private void RechooseAbilityCheck()
    {
        if (!rechoseAbility)
        {
            if (manager.NextAttack == manager.Sword)
            {
                float threshold = manager.Charge.AttackDistance - 0.25f;
                if (lastDistanceToPlayer <= threshold && distanceToPlayer > threshold)
                {
                    manager.ChooseNextAbility();
                    rechoseAbility = true;
                }
            }
        }

        if (lastDistanceToPlayer < 5f)
        {
            manager.NextAttack = manager.Sword;
        }
    }
}