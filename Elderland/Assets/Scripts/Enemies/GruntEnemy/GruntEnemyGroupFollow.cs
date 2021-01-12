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
    private float distanceToPlayer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;

        distanceToPlayer = manager.DistanceToPlayer();
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

            if (!manager.GroupMovement)
            {
                if (checkTimer > checkDuration)
                {
                    manager.UpdateAgentPath();
                }
            }
            else
            {
                if (!manager.Group.IsStopped)
                {
                    // Stop condition 2
                    if (EnemyGroup.AttackingEnemies.Count != 0)
                    {
                        Vector3 groupOffset =
                            manager.Group.CalculateCenter() - PlayerInfo.Player.transform.position;
                        groupOffset = Matho.StandardProjection3D(groupOffset);

                        if (groupOffset.magnitude <= manager.CentralStopRadius)
                        {
                            manager.Group.Stop();
                        }
                    }
                }
                else
                {
                    // Start condition 1
                    if (EnemyGroup.AttackingEnemies.Count == 0)
                    {
                        manager.Group.Resume();
                    }
                    else
                    {
                        // Start condition 2
                        Vector3 groupOffset =
                            manager.Group.CalculateCenter() - PlayerInfo.Player.transform.position;
                        groupOffset = Matho.StandardProjection3D(groupOffset);

                        if (groupOffset.magnitude > manager.CentralStopRadius + manager.CentralStopRadiusMargin)
                        {
                            manager.Group.Resume();
                        }
                    }
                }

                manager.Group.Adjust(
                    PlayerInfo.Player.transform.position,
                    1.2f * Time.deltaTime,
                    0.5f * Time.deltaTime,
                    0.5f * Time.deltaTime,
                    manager.NearbySensor.Radius);
                manager.Agent.Move(manager.Velocity);
            }

            manager.RotateTowardsPlayer();

            manager.ClampToGround();

            FarFollowTransition();
            if (!exiting)
                AttackTransition();

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
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

    private void AttackTransition()
    {
        Vector2 horizontalOffset = 
            Matho.StandardProjection2D(PlayerInfo.Player.transform.position - manager.transform.position);
        if (horizontalOffset.magnitude < manager.AttackFollowRadius &&
            EnemyGroup.AttackingEnemies.Count < EnemyGroup.MaxAttackingEnemies)
        {
            AttackExit();
        }
    }

    private void FarFollowExit()
    {
        manager.Animator.SetTrigger("toFarFollow");
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.GroupMovement = false;
        manager.InGroupState = false;
        exiting = true;
    }

    private void AttackExit()
    {
        if (manager.Group != null)
            manager.Group.Stop();
        EnemyGroup.AttackingEnemies.Add(manager);
        manager.Animator.SetTrigger("toAttackFollow");
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.Agent.ResetPath();
        manager.GroupMovement = false;
        manager.InGroupState = false;
        exiting = true;
    }
}