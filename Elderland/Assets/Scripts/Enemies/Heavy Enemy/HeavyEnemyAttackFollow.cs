using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HeavyEnemyAttackFollow : StateMachineBehaviour
{
    private HeavyEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    private const float rotateDistance = 3;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<HeavyEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
        lastDistanceToPlayer = DistanceToPlayer();
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
            distanceToPlayer = DistanceToPlayer();
            remainingDistance = manager.Agent.remainingDistance;

            if (manager.ArrangementNode != -1)
            {
                if (EnemyInfo.MeleeArranger.OverrideNode(manager))
                {
                    manager.CalculateAgentPath();
                }
                else if (checkTimer >= checkDuration)
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

            manager.ClampToGround();

            if (manager.ArrangementNode != -1)
                StopTransition();
            
            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void RotateTowardsPlayer()
    {
        if (manager.Agent.hasPath)
        {
            if (manager.towardsPlayer)
            {
                if (remainingDistance >= rotateDistance) //lastRemainingDistance < rotateDistance && 
                {
                    if (!manager.Agent.updateRotation)
                        manager.Agent.updateRotation = true;
                }
                else if (remainingDistance < rotateDistance) // && lastRemainingDistance >= rotateDistance
                {
                    if (manager.Agent.updateRotation)
                        manager.Agent.updateRotation = false;
                }

                if (remainingDistance < rotateDistance)
                {
                    Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                    Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 2f * Time.deltaTime, 0f);
                    manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                }
            }
            else
            {
                manager.Agent.updateRotation = false;
                Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 2f * Time.deltaTime, 0f);
                manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }

    private void StopTransition()
    {
        if (manager.completedWaypoints && manager.IsInNextAttackMax())
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
        manager.AbilityManager.StartQueue();
        manager.Animator.SetTrigger("runAbility");
        exiting = true;
    }

    private void StationaryExit()
    {
        manager.TurnOffAgent();
        manager.Animator.SetTrigger("toStationary");
        exiting = true;
    }

    private float DistanceToPlayer()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(manager.transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);
        return horizontalDistanceToPlayer;
    }
}
