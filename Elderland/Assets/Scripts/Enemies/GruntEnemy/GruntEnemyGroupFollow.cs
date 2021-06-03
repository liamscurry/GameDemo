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
    private float nearbySensorScale;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        manager.BehaviourLock = this;

        checkTimer = checkDuration;
        exiting = false;

        distanceToPlayer = manager.DistanceToPlayer();
        manager.InGroupState = true;
        manager.Agent.updateRotation = true;
        manager.PingedToAttack = false;
    }

    private void OnStateExitImmediate()
    {
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.Agent.ResetPath();
        manager.InGroupState = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager.BehaviourLock != this && !exiting)
        {
            OnStateExitImmediate();
            exiting = true;
        }

        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();
            
            if (manager.Group == null)
            {
                if (checkTimer > checkDuration)
                {
                    if (distanceToPlayer > manager.GroupFollowRadius + manager.GroupFollowRadiusMargin ||
                        EnemyGroup.AttackingEnemies.Count == 0)
                        manager.UpdateAgentPath();
                }
            }
            else
            {
                if (!manager.Group.IsStopped)
                {
                    // Stop condition 1
                    if (EnemyGroup.AttackingEnemies.Count == EnemyGroup.MaxAttackingEnemies)
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
                    3.3f * Time.deltaTime,
                    0.5f * Time.deltaTime,
                    GruntEnemyManager.ExpandSpeed * Time.deltaTime,
                    manager.NearbySensor.Radius,
                    GruntEnemyManager.ShrinkSpeed * Time.deltaTime,
                    manager.ShrinkRadius);

                manager.Agent.Move(manager.Velocity);
            }

            manager.RotateTowardsPlayer();

            manager.ClampToGround();

            if (!exiting)
                PingTransition();
            if (!exiting)
                FarFollowTransition();
            if (!exiting)
                OverrideAttackTransition();
            if (!exiting)
                AttackTransition();
            if (exiting)
            {
                OnStateExitImmediate();
            }

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
            Matho.StdProj2D(PlayerInfo.Player.transform.position - manager.transform.position);
        if (horizontalOffset.magnitude < manager.AttackFollowRadius &&
            EnemyGroup.AttackingEnemies.Count < EnemyGroup.MaxAttackingEnemies)
        {
            AttackExit();
        }
    }

    private void PingTransition()
    {
        if (manager.PingedToAttack)
        {
            if (EnemyGroup.AttackingEnemies.Count < EnemyGroup.MaxAttackingEnemies)
            {
                PingExit();
            }
            else
            {
                manager.PingedToAttack = false;
            }
        }
    }

    private void OverrideAttackTransition()
    {
        if (EnemyGroup.AttackingEnemies.Count == EnemyGroup.MaxAttackingEnemies)
        {
            Vector2 offset = 
                Matho.StdProj2D(manager.Position - PlayerInfo.Player.transform.position);
            foreach (IEnemyGroup enemy in EnemyGroup.AttackingEnemies)
            {
                Vector2 enemyOffset = 
                    Matho.StdProj2D(enemy.Position - PlayerInfo.Player.transform.position);
                if (offset.magnitude < enemyOffset.magnitude)
                {
                    // Override logic.
                    OverrideAttackExit(enemy);
                    break;
                }
            }
        }
    }

    private void OverrideAttackExit(IEnemyGroup enemy)
    {
        GruntEnemyManager enemyManager = 
            (GruntEnemyManager) enemy;

        enemyManager.PingedToGroup = true;

        EnemyGroup.AttackingEnemies.Remove(enemyManager);
        EnemyGroup.RemoveAttacking(enemyManager);

        EnemyGroup.AttackingEnemies.Add(manager);
        if (manager.Group != null)
        {
            manager.Group.Stop();
        }
        
        manager.Animator.SetTrigger("toAttackFollow");
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.Agent.ResetPath();
        manager.InGroupState = false;
        exiting = true;
    }

    private void FarFollowExit()
    {
        manager.Animator.SetTrigger("toFarFollow");
        exiting = true;
    }

    private void AttackExit()
    {
        EnemyGroup.AttackingEnemies.Add(manager);
        if (manager.Group != null)
        {
            PingNearbyEnemies();
            manager.Group.Stop();
        }
        
        manager.Animator.SetTrigger("toAttackFollow");
        exiting = true;
    }

    private void PingExit()
    {
        EnemyGroup.AttackingEnemies.Add(manager);
        manager.Animator.SetTrigger("toAttackFollow");
        exiting = true;
    }

    private void PingNearbyEnemies()
    {
        Collider[] nearbyEnemies =
            Physics.OverlapSphere(
                manager.transform.position,
                manager.AttackPingRadius,
                LayerConstants.Enemy);
        
        int availableSpots =
            EnemyGroup.MaxAttackingEnemies - EnemyGroup.AttackingEnemies.Count;

        int count = 0;
        while (availableSpots > 0 && count < nearbyEnemies.Length)
        {
            GruntEnemyManager gruntEnemy = 
                nearbyEnemies[count].GetComponent<GruntEnemyManager>();

            if (gruntEnemy != null &&
                gruntEnemy != manager &&
                EnemyGroup.Contains(manager.Group, gruntEnemy))
            {
                gruntEnemy.PingedToAttack = true;
                availableSpots--;
            }
            count++;
        }
    }
}