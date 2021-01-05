using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GruntEnemyAttackFollow : StateMachineBehaviour
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
                //UpdatePath();
            }

            manager.ClampToGround();
            
            //if (manager.ArrangementNode != -1)
            //    StopTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void UpdatePath()
    {
        Vector2 projectedPlayerPosition = 
            Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        Vector3 targetPosition =
            GameInfo.CurrentLevel.NavCast(projectedPlayerPosition);
        
        manager.Agent.destination = targetPosition;
    }

    private void RotateTowardsPlayer()
    {
        if (manager.Agent.hasPath)
        {
            if (remainingDistance >= manager.GroupFollowRadius)
            {
                if (!manager.Agent.updateRotation)
                    manager.Agent.updateRotation = true;
            }
            else if (remainingDistance < manager.GroupFollowRadius)
            {
                if (manager.Agent.updateRotation)
                    manager.Agent.updateRotation = false;
            }

            if (remainingDistance < manager.GroupFollowRadius)
            {
                Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
                Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1f * Time.deltaTime, 0f);
                manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
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
}