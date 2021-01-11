using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GruntEnemyFarFollow : StateMachineBehaviour
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
        manager.Agent.updateRotation = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();
            remainingDistance = manager.Agent.remainingDistance;

            if (checkTimer > checkDuration)
            {
                manager.UpdateAgentPath();
            }

            manager.ClampToGround();
            
            GroupFollowTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void GroupFollowTransition()
    {
        Vector3 enemyDirection =
            manager.transform.position - PlayerInfo.Player.transform.position;
        enemyDirection.Normalize();

        NavMeshHit navMeshHit;
        if (distanceToPlayer < manager.GroupFollowRadius &&
            !manager.Agent.Raycast(manager.PlayerNavMeshPosition(enemyDirection), out navMeshHit))
        {
            GroupFollowExit();
        }
    }

    private void GroupFollowExit()
    {
        manager.Animator.SetTrigger("toGroupFollow");
        manager.GroupSensor.Reset();
        exiting = true;
    }
}