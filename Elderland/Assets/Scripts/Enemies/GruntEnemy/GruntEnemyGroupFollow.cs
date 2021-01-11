using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GruntEnemyGroupFollow : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;
        manager.InGroupState = true;
        manager.GroupMovement = false;
        manager.Agent.updateRotation = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();
            remainingDistance = manager.Agent.remainingDistance;

            if (!manager.GroupMovement)
            {
                if (checkTimer > checkDuration)
                {
                    manager.UpdateAgentPath();
                }
            }
            else
            {
                manager.Group.Adjust(
                    PlayerInfo.Player.transform.position,
                    1.2f * Time.deltaTime,
                    0.5f * Time.deltaTime,
                    0.5f * Time.deltaTime,
                    manager.NearbySensor.Radius);
                manager.Agent.Move(manager.Velocity);
            }

            RotateTowardsPlayer();

            manager.ClampToGround();

            FarFollowTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (!manager.GroupMovement)
        {
            if (!manager.Agent.updateRotation)
                manager.Agent.updateRotation = true;
        }
        else
        {
            if (manager.Agent.updateRotation)
            {
                manager.Agent.updateRotation = false;
            }
            else
            {
                Vector3 targetForward =
                    Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                Vector3 forward =
                    Vector3.RotateTowards(manager.transform.forward, targetForward, 1f * Time.deltaTime, 0f);
                manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
        Debug.Log(manager.Agent.updateRotation);
    }

    private void FarFollowTransition()
    {
        Vector3 enemyDirection =
            manager.transform.position - PlayerInfo.Player.transform.position;
        enemyDirection.Normalize();

        NavMeshHit navMeshHit;
        if (distanceToPlayer > manager.GroupFollowRadius + manager.GroupFollowRadiusMargin ||
            manager.Agent.Raycast(manager.PlayerNavMeshPosition(enemyDirection), out navMeshHit))
        {
            FarFollowExit();
        }
    }

    private void FarFollowExit()
    {
        manager.Animator.SetTrigger("toFarFollow");
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.InGroupState = false;
        exiting = true;
    }
}