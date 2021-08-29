using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
Using EnemyGroup in enemy state machines:
Has three required states, double linked in far-group-attack fashion:
-Far Follow
-Group Follow
-Attack Follow

In far follow, the enemy simply moves towards the player using its agent path.

In grouped follow, will move using agent path if alone, but when in a group will move with group.
Group/single will move towards player if out of range out there are not max enemies attacking. Enemies
can only be in a group in this state.

Attack follow moves towards the player directly and only uses the unique attacking enemy group
to distance each enemy from each other. Enemies are in the attacking group in this state and when
using abilities, unless overriden or out of range, in which a transition will occur in attack follow 
to go back to group follow.

States are safely exited internally or externally using an immediate method that is called when
externally exiting from another source such as when getting hit (flinch state). Some of this logic may
be used with internal transitions, and thus may be called in interal transitions.

The following structure below was the initially structure update. Now the structure is the following:
Current Structure:
Use the generic AttackFollow, GroupFollow, etc behaviours. If slightly different functionality is
needed, override the class.

Old Structure (currently used in FarFollow, GroupFollow and AttackFollow):
Group Follow:
EnemyGroup.OnGroupFollowEnter(manager);
EnemyGroup.FarFollowTransition(manager, ref exiting);
EnemyGroup.OverrideAttackTransition(manager, ref exiting);
EnemyGroup.AttackTransition(manager, ref exiting);
EnemyGroup.UpdateGroupFollowMovement(manager, checkTimer > checkDuration);
EnemyGroup.UpdateGroupFollowRotation(manager);
EnemyGroup.OnGroupFollowImmediateExit(manager); also call this on all transitions from the state

Attack Follow:
EnemyGroup.OnAttackFollowEnter(manager);
EnemyGroup.AttackFollowToGroupFollowTransition(manager, ref exiting, ref exitingFromAttack);
EnemyGroup.AttackFollowToAttackTransition(manager, ref exiting, ref exitingFromAttack);
EnemyGroup.UpdateAttackFollowMovement(manager);
EnemyGroup.UpdateAttackFollowRotation(manager);
EnemyGroup.OnAttackFollowImmediateExit(manager); also call this on transitions that leave the attack cycle.

In attack shortcut logic methods:
EnemyGroup.OnAttackFollowImmediateExit(manager);

Notes:
When removing an enemy from a group and adding to another group, do that in the remove add order
to eliminate multi group bugs.
*/

public class EnemyGroup : IComparable<EnemyGroup>
{
    private List<IEnemyGroup> enemies;
    private const float offsetThreshold = 0.005f;
    private bool adjustAvailable;
    private bool isStopped;

    public static readonly int MaxAttackingEnemies = 2;
    public static EnemyGroup AttackingGroup { get; }

    public bool IsStopped { get { return isStopped; } }

    public int EnemyCount 
    { 
        get 
        {
            if (enemies == null)
            {
                return 0;
            } 
            else
            {
                return enemies.Count;
            }
        }
    }

    static EnemyGroup()
    {
        AttackingGroup = new EnemyGroup();
    }

    private EnemyGroup()
    {
        enemies = new List<IEnemyGroup>();
        adjustAvailable = true;
    }

    public int CompareTo(EnemyGroup other)
    {
        if (this == other)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    /*
    Helper method that updates an enemy's group follow movement. There are two scenarios: in a group
    and when not in a group. In either case, the agent moves when too far away from the group radius
    or when there are open spots to attack the player. Moves the player via the NavMeshAgent component.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void UpdateGroupFollowMovement(GruntEnemyManager manager, bool updateAlonePath)
    {
        if (manager.Group == null)
        {
            if (updateAlonePath)
            {
                float distanceToPlayer = manager.DistanceToPlayer();
                if (distanceToPlayer > manager.GroupFollowRadius + manager.GroupFollowRadiusMargin ||
                    AttackingGroup.enemies.Count < EnemyGroup.MaxAttackingEnemies)
                {
                    manager.UpdateAgentPath();
                }
                else
                {
                    manager.Agent.ResetPath();
                }
            }
        }
        else
        {
            if (!manager.Group.IsStopped)
            {
                // Stop condition 1
                if (AttackingGroup.enemies.Count == EnemyGroup.MaxAttackingEnemies)
                {
                    Vector3 groupOffset =
                        manager.Group.CalculateCenter() - PlayerInfo.Player.transform.position;
                    groupOffset = Matho.StdProj3D(groupOffset);

                    if (groupOffset.magnitude <= manager.CentralStopRadius)
                    {
                        manager.Group.Stop();
                    }
                }
            }
            else
            {
                // Start condition 1
                if (AttackingGroup.enemies.Count < EnemyGroup.MaxAttackingEnemies)
                {
                    manager.Group.Resume();
                }
                else
                {
                    // Start condition 2
                    Vector3 groupOffset =
                        manager.Group.CalculateCenter() - PlayerInfo.Player.transform.position;
                    groupOffset = Matho.StdProj3D(groupOffset);

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
    }

    /*

    Inputs:
    Helper function that rotates the player during group follow movement. Rotates differently depending
    whether the enemy is in a group or not.

    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void UpdateGroupFollowRotation(GruntEnemyManager manager)
    {
        if (manager.Group == null)
        {
            if (manager.Agent.hasPath)
            {
                manager.Agent.updateRotation = true;
            }
            else
            {
                manager.Agent.updateRotation = false;
                RotateTowardsPlayer(manager, 1f);
            }

            manager.UpdatingRotation = true;
        }
        else
        {
            if (manager.Agent.updateRotation)
            {
                manager.Agent.updateRotation = false;
            }
            else
            {
                manager.UpdatingRotation = false;
                RotateTowardsPlayer(manager, 1f);
            }
        }
    }

    /*
    Helper function that rotates the enemy towards the player. Must have Agent.updateRotation disabled
    for this method to have effect.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void RotateTowardsPlayer(GruntEnemyManager manager, float speed)
    {
        Vector3 targetForward =
            Matho.StdProj3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        Vector3 forward =
            Vector3.RotateTowards(manager.transform.forward, targetForward, speed * Time.deltaTime, 0f);
        manager.transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);
    }

    /*
    Helper method used to move the enemy to the player when in attack state.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void UpdateAttackFollowMovement(GruntEnemyManager manager)
    {
        if (!manager.IsInNextAttackMax())
        {
            AttackingGroup.Adjust(
                PlayerInfo.Player.transform.position,
                0,
                0,
                GruntEnemyManager.ExpandSpeed * 0.5f * Time.deltaTime,
                manager.NearbySensor.Radius,
                0,
                0,
                true);
            manager.Agent.Move(manager.Velocity);

            Vector3 moveDirection = 
                PlayerInfo.Player.transform.position - manager.transform.position;
            moveDirection.Normalize();
            manager.Agent.Move(moveDirection * manager.AttackFollowSpeed * Time.deltaTime);
        }
    }

    /*
    Helper method used to rotate the enemy to the player when in attack state.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void UpdateAttackFollowRotation(GruntEnemyManager manager)
    {
        Vector3 targetForward =
            Matho.StdProj3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        Vector3 forward =
            Vector3.RotateTowards(manager.transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
    
    // Transitions //
    /*
    Helper method to transition from Group Follow to Attack Follow.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    public static void AttackTransition(GruntEnemyManager manager, ref bool exiting)
    {
        Vector2 horizontalOffset = 
            Matho.StdProj2D(PlayerInfo.Player.transform.position - manager.transform.position);

        if (horizontalOffset.magnitude < manager.AttackFollowRadius &&
            AttackingGroup.enemies.Count < EnemyGroup.MaxAttackingEnemies)
        {
            AttackExit(manager, ref exiting);
        }
    }

    /*
    Helper method called when AttackTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    private static void AttackExit(GruntEnemyManager manager, ref bool exiting)
    {
        EnemyGroup.Remove((IEnemyGroup) manager);
        EnemyGroup.AddAttacking(manager);
        manager.Animator.SetTrigger("toAttackFollow");
        manager.Agent.ResetPath();
        manager.InGroupState = false;
        exiting = true;
    }

    /*
    Helper method to transition from Group Follow to Attack Follow (when overriding a fighting enemy).

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    public static void OverrideAttackTransition(GruntEnemyManager manager, ref bool exiting)
    {
        if (AttackingGroup.enemies.Count == EnemyGroup.MaxAttackingEnemies)
        {
            Vector2 offset = 
                Matho.StdProj2D(manager.Position - PlayerInfo.Player.transform.position);
            foreach (IEnemyGroup enemy in AttackingGroup.enemies)
            {
                Vector2 enemyOffset = 
                    Matho.StdProj2D(enemy.Position - PlayerInfo.Player.transform.position);
                if (offset.magnitude < enemyOffset.magnitude)
                {
                    // Override logic.
                    OverrideAttackExit(enemy, manager, ref exiting);
                    break;
                }
            }
        }
    }

    /*
    Helper method called when OverrideAttackTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    private static void OverrideAttackExit(
        IEnemyGroup other,
        GruntEnemyManager manager,
        ref bool exiting)
    {
        GruntEnemyManager enemyManager = 
            (GruntEnemyManager) other;

        EnemyGroup.RemoveAttacking(enemyManager);

        EnemyGroup.Remove((IEnemyGroup) manager);
        EnemyGroup.AddAttacking(manager);

        manager.Animator.SetTrigger("toAttackFollow");
        manager.Agent.ResetPath();
        manager.InGroupState = false;
        
        exiting = true;
    }

    /*
    Helper method to transition from Group Follow to Far Follow.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    public static void FarFollowTransition(GruntEnemyManager manager, ref bool exiting)
    {
        Vector3 enemyDirection =
            manager.transform.position - PlayerInfo.Player.transform.position;
        enemyDirection.Normalize();

        NavMeshHit navMeshHit;
        if (manager.DistanceToPlayer() > manager.GroupFollowRadius + manager.GroupFollowRadiusMargin ||
            manager.Agent.Raycast(manager.PlayerNavMeshPosition(enemyDirection), out navMeshHit))
        {
            FarFollowExit(manager, ref exiting);
            OnGroupFollowImmediateExit(manager);
        }
    }

    /*
    Helper method called when FarFollowTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    private static void FarFollowExit(GruntEnemyManager manager, ref bool exiting)
    {
        manager.Animator.SetTrigger("toFarFollow");
        exiting = true;
    }

    /*
    Helper method to transition from Attack Follow to Group Follow.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting
    ref bool : exitingFromAttack : state machine behaviour boolean on whether the state is exiting
    particularly to attack state.

    Outputs:
    None
    */
    public static void AttackFollowToGroupFollowTransition(
        GruntEnemyManager manager,
        ref bool exiting,
        ref bool exitingFromAttack)
    {
        Vector3 enemyDirection =
            manager.transform.position - PlayerInfo.Player.transform.position;
        enemyDirection.Normalize();
        NavMeshHit navMeshHit;

        Vector2 horizontalOffset = 
            Matho.StdProj2D(PlayerInfo.Player.transform.position - manager.transform.position);
        if (horizontalOffset.magnitude > manager.AttackFollowRadius + manager.AttackFollowRadiusMargin || 
            !AttackingGroup.enemies.Contains(manager) ||
            manager.Agent.Raycast(manager.PlayerNavMeshPosition(enemyDirection), out navMeshHit))
        {
            AttackFollowToGroupFollowExit(manager, ref exiting);
            OnAttackFollowImmediateExit(manager);
        }
    }

    /*
    Helper method called when AttackFollowToGroupFollowTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    private static void AttackFollowToGroupFollowExit(GruntEnemyManager manager, ref bool exiting)
    {
        manager.Animator.SetTrigger("toGroupFollow");
        EnemyGroup.RemoveAttacking(manager);
        exiting = true;
    }

    /*
    Helper method to transition from Attack Follow to Attack.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting
    ref bool : exitingFromAttack : state machine behaviour boolean on whether the state is exiting
    particularly to attack state.

    Outputs:
    None
    */
    public static void AttackFollowToAttackTransition(
        GruntEnemyManager manager,
        ref bool exiting,
        ref bool exitingFromAttack)
    {
        if (manager.IsInNextAttackMax())
        {
            Vector3 playerEnemyDirection =
                (PlayerInfo.Player.transform.position - manager.transform.position).normalized;
            float playerEnemyAngle =
                Matho.AngleBetween(
                    Matho.StdProj2D(manager.transform.forward),
                    Matho.StdProj2D(playerEnemyDirection));

            if (playerEnemyAngle < manager.NextAttack.AttackAngleMargin)
            {
                AttackFollowToAttackExit(manager, ref exiting, ref exitingFromAttack);
            }
        }
    }

    /*
    Helper method called when AttackFollowToAttackTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting
    ref bool : exitingFromAttack : state machine behaviour boolean on whether the state is exiting
    particularly to attack state.

    Outputs:
    None
    */
    private static void AttackFollowToAttackExit(
        GruntEnemyManager manager,
        ref bool exiting,
        ref bool exitingFromAttack)
    {
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        manager.Agent.ResetPath();
        exiting = true;
        exitingFromAttack = true;
    }

    /*
    Helper method to transition from Far Follow to Group Follow.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    public static void GroupFollowTransition(
        GruntEnemyManager manager,
        ref bool exiting)
    {
        Vector3 enemyDirection =
            manager.transform.position - PlayerInfo.Player.transform.position;
        enemyDirection.Normalize();

        NavMeshHit navMeshHit;
        if (manager.DistanceToPlayer() < manager.GroupFollowRadius &&
            !manager.Agent.Raycast(manager.PlayerNavMeshPosition(enemyDirection), out navMeshHit))
        {
            GroupFollowExit(manager, ref exiting);
            OnFarFollowImmediateExit(manager);
        }
    }

    /*
    Helper method called when GroupFollowTransition meets its transition requirements.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.
    ref bool : exiting : state machine behaviour boolean on whether the state is exiting

    Outputs:
    None
    */
    private static void GroupFollowExit(
        GruntEnemyManager manager,
        ref bool exiting)
    {
        manager.Animator.SetTrigger("toGroupFollow");
        exiting = true;
    }

    // Events //
    /*
    Method called when OnStateEnter is invoked in the GroupFollow state machine behaviour.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnGroupFollowEnter(GruntEnemyManager manager)
    {
        manager.InGroupState = true;
        manager.Agent.updateRotation = true;
    }

    /*
    Method called when OnStateExit is immediately invoked from an outside source in the GroupFollow
    state machine behaviour. Called in transition from group follow to far folllow as well
    as it contains the same logic that needs the enemy to not be a part of a group anymore.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnGroupFollowImmediateExit(GruntEnemyManager manager)
    {
        EnemyGroup.Remove((IEnemyGroup) manager);
        manager.Agent.ResetPath();
        manager.InGroupState = false;
    }

    /*
    Method called when OnStateEnter is invoked in the AttackFollow state machine behaviour.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnAttackFollowEnter(GruntEnemyManager manager)
    {
        manager.Agent.radius = manager.FightingAgentRadius;
        manager.Agent.stoppingDistance = manager.NextAttack.AttackDistance * 0.8f;
    }

    /*
    Method called when OnStateExit is immediately invoked from an outside source in the AttackFollow
    state machine behaviour. Assumes right now that after being pulled out of attackfollow/attack
    states that it will return directly afterwards to the attack follow state. If the state it is pulled
    to wants it to not be in attacking state (such as bounds away) then it must manually clear the attacking
    group.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnAttackFollowImmediateExit(GruntEnemyManager manager)
    {
        manager.Agent.radius = manager.FollowAgentRadius;
        manager.Agent.stoppingDistance = 0;
        //EnemyGroup.RemoveAttacking(manager);
    }

    /*
    Method called when OnStateEnter is invoked in the FarFollow state machine behaviour.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnFarFollowEnter(GruntEnemyManager manager)
    {
        manager.Agent.updateRotation = true;
    }

    /*
    Method called when OnStateExit is immediately invoked from an outside source in the FarFollow
    state machine behaviour.

    Inputs:
    GruntEnemyManager : manager : enemy manager that supports using EnemyGroup logic.

    Outputs:
    None
    */
    public static void OnFarFollowImmediateExit(GruntEnemyManager manager)
    {
        manager.GroupSensor.Reset();
        manager.Agent.ResetPath();
    }

    // Calculates weighted center of enemies for movement methods.
    public Vector3 CalculateCenter()
    {
        if (enemies.Count == 0)
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 posSum = Vector3.zero;

            foreach (IEnemyGroup enemy in enemies)
            {
                posSum += enemy.Position;
            }

            return posSum * (1.0f / enemies.Count);
        }
    }

    public void Stop()
    {
        isStopped = true;
    }

    public void Resume()
    {
        isStopped = false;
    }

    private void Move(Vector3 center, Vector3 target, float speed)
    {
        Vector3 velocity = 
            (target - center);
        velocity = Matho.StdProj3D(velocity);

        if (velocity.magnitude < offsetThreshold)
        {
            return;
        }

        velocity = velocity.normalized * speed;
        
        foreach (IEnemyGroup enemy in enemies)
        {
            enemy.Velocity += velocity;
        }
    }

    private static float AbsoluteAngleBetween(Vector2 v, Vector2 w)
    {
        float percentageBetween = 
            Matho.AngleBetween(v, w) / 180.0f;
        
        if (percentageBetween <= 0.5f)
        {
            percentageBetween *= 2;
        }
        else
        {
            percentageBetween = -2 * (percentageBetween - 1);
        }

        return percentageBetween;
    }

    private float CalculateRotationConstant(Vector3 center, Vector3 target)
    {
        if (enemies.Count == 0)
        {
            return 1;
        }
        else
        {
            float sumAbsAngle = 0;

            Vector3 targetDirection3D = 
                (target - center);
            
            Vector2 targetDirection =
                Matho.StdProj2D(targetDirection3D);

            if (targetDirection.magnitude < offsetThreshold)
            {
                return 1;
            }

            targetDirection.Normalize();

            foreach (IEnemyGroup enemy in enemies)
            {
                Vector3 centerDirection3D =
                    (center - enemy.Position);
                Vector2 centerDirection =
                    Matho.StdProj2D(centerDirection3D).normalized;

                sumAbsAngle += AbsoluteAngleBetween(targetDirection, centerDirection);
            }

            return sumAbsAngle / enemies.Count;
        }
    }

    private void Rotate(Vector3 center, float speed)
    {
        foreach (IEnemyGroup enemy in enemies)
        {
            Vector3 centerDirection = 
                (center - enemy.Position);
            centerDirection =
                Matho.StdProj3D(centerDirection).normalized;
            Vector3 tangentDirection =
                Matho.Rotate(centerDirection, Vector3.up, 90f);
            enemy.Velocity += tangentDirection * speed;
        }
    }

    private void Expand(Vector3 target, float speed, float radius, bool spin = false)
    {
        foreach (IEnemyGroup enemy in enemies)
        {
            if (enemy.NearbyEnemies.Count > 0)
            {
                foreach (IEnemyGroup nearbyEnemy in enemy.NearbyEnemies)
                {
                    if (nearbyEnemy.Group != this)
                        continue;

                    Vector3 nearbyDirection =
                        nearbyEnemy.Position - enemy.Position;
                    nearbyDirection = Matho.StdProj3D(nearbyDirection);

                    if (spin)
                        nearbyDirection = SpinExpand(target, nearbyEnemy, nearbyDirection);

                    if (nearbyDirection.magnitude > offsetThreshold)
                    {
                        float scaledSpeed = speed;

                        //if (!spin)
                        {
                            //scaledSpeed -= Mathf.Pow(nearbyDirection.magnitude / radius, 2) * speed;
                            scaledSpeed -= Mathf.Pow(nearbyDirection.magnitude / radius, 1) * speed;
                            if (scaledSpeed < 0)
                                scaledSpeed = 0;
                        }

                        nearbyDirection.Normalize();
                        nearbyEnemy.Velocity += nearbyDirection * scaledSpeed * 0.5f;
                        enemy.Velocity += -nearbyDirection * scaledSpeed * 0.5f;
                    }
                }
            }
        }
    }

    private void Shrink(Vector3 target, Vector3 center,  float shrinkSpeed, float shrinkRadius)
    {
        bool surroundingTarget = Matho.StdProj2D(target - center).magnitude < shrinkRadius;
        if (surroundingTarget)
        {
            foreach (IEnemyGroup enemy in enemies)
            {
                //Vector2 offset = Matho.StandardProjection2D(target - enemy.Position);
                //if (offset.magnitude < shrinkRadius || surroundingTarget)
                //{
                    enemy.Velocity += (target - enemy.Position).normalized * shrinkSpeed;
                //}
            }
        }
    }

    private Vector3 SpinExpand(Vector3 target, IEnemyGroup nearbyEnemy, Vector3 nearbyDirection)
    {
        Vector3 targetDirection = 
            target - nearbyEnemy.Position;
        targetDirection = 
            Matho.StdProj3D(targetDirection);

        Vector3 tangentDirection = Vector3.zero;
            
        float tangencyAngle = 
            Matho.AngleBetween(targetDirection, nearbyDirection);

        if (tangencyAngle == 0 ||
            tangencyAngle == 180 ||
            nearbyDirection.magnitude == 0 ||
            targetDirection.magnitude == 0)
        {
            tangentDirection = Matho.Rotate(targetDirection, Vector3.up, 90f);
            tangentDirection.Normalize();
        }
        else
        {
            // Gram-schmidt for tangent direction.
            Vector3 enemyDirection = -nearbyDirection;
            Vector3 projectedEnemyDirection = 
                Matho.Project(enemyDirection, targetDirection);
            tangentDirection = enemyDirection - projectedEnemyDirection;
            tangentDirection.Normalize();
        }

        return -tangentDirection;
    }

    public void Adjust(
        Vector3 target,
        float speed,
        float rotateSpeed,
        float expandSpeed,
        float expandRadius,
        float shrinkSpeed,
        float shrinkRadius,
        bool expandSpin = false)
    {
        if (adjustAvailable)
        {
            Vector3 center = 
                CalculateCenter();

            if (!isStopped && CalculateRotationConstant(center, target) < 0.55f)
                Rotate(center, rotateSpeed);

            Expand(target, expandSpeed, expandRadius, expandSpin);

            if (!isStopped)
                Shrink(target, center, shrinkSpeed, shrinkRadius);

            if (!isStopped)
                Move(center, target, speed);
                
            adjustAvailable = false;
        }
    }

    public void ResetAdjust()
    {
        if (!adjustAvailable)
        {
            adjustAvailable = true;

            foreach (IEnemyGroup enemy in enemies)
            {
                enemy.Velocity = Vector3.zero;
            }
        }
    }

    public static bool Contains(EnemyGroup group, IEnemyGroup enemy)
    {
        return group.enemies.Contains(enemy);
    }

    // Automatically combines groups and creates groups when
    // two enemies try to group up for autonomous grouping.
    public static void Add(IEnemyGroup e1, IEnemyGroup e2)
    {
        if (e1.Group == null && e2.Group != null)
        {
            e1.Group = e2.Group;
            e2.Group.enemies.Add(e1);
        }
        else if (e2.Group == null && e1.Group != null)
        {
            e2.Group = e1.Group;
            e1.Group.enemies.Add(e2);
        }
        else if (e1.Group == null && e2.Group == null)
        {
            // No Groups
            EnemyGroup newGroup = 
                new EnemyGroup();
            e1.Group = newGroup;
            e2.Group = newGroup;
            newGroup.enemies.Add(e1);
            newGroup.enemies.Add(e2);
        }
        else
        {
            if (e1.Group == e2.Group)
                return;

            // Two Groups
            EnemyGroup temp = 
                e1.Group;

            foreach (IEnemyGroup enemy in temp.enemies)
            {
                enemy.Group = e2.Group;
                e2.Group.enemies.Add(enemy);
            }

            temp.enemies.Clear();
        }
    }

    public static void AddAttacking(IEnemyGroup e)
    {
        if (!AttackingGroup.enemies.Contains(e))
        {
            e.Group = AttackingGroup;
            AttackingGroup.enemies.Add(e);
        }
    }

    // Removes an enemy from its group, deleting group if empty afterwards for automation.
    public static void Remove(IEnemyGroup e)
    {
        if (e.Group != null)
        {
            EnemyGroup temp = e.Group;
            e.Group = null;
            temp.enemies.Remove(e);
        }
    }

    public static void RemoveAttacking(IEnemyGroup e)
    {
        if (AttackingGroup.enemies.Contains(e))
        {
            e.Group = null;
            AttackingGroup.enemies.Remove(e);
        }
    }

    // Unit Tests
    public static void AddTest()
    {
        // Both no group
        var e1 = new EnemyGroupUTDummy();
        var e2 = new EnemyGroupUTDummy();

        EnemyGroup.Add(e1, e2);
        UT.CheckEquality<EnemyGroup>(e1.Group, e2.Group);
        UT.CheckDifference<EnemyGroup>(e1.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 2);

        // First null
        var e3 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e3, e1);
        UT.CheckEquality<EnemyGroup>(e3.Group, e1.Group);
        UT.CheckDifference<EnemyGroup>(e3.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 3);

        // Second null
        var e4 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e1, e4);
        UT.CheckEquality<EnemyGroup>(e1.Group, e4.Group);
        UT.CheckDifference<EnemyGroup>(e4.Group, null);  
        UT.CheckEquality<int>(e1.Group.EnemyCount, 4);

        // Both have group
        var e5 = new EnemyGroupUTDummy();
        var e6 = new EnemyGroupUTDummy();
        EnemyGroup.Add(e5, e6);
        EnemyGroup.Add(e5, e1);
        UT.CheckEquality<EnemyGroup>(e1.Group, e5.Group);
        UT.CheckDifference<EnemyGroup>(e5.Group, null);   
        UT.CheckEquality<EnemyGroup>(e5.Group, e6.Group);
        UT.CheckDifference<EnemyGroup>(e6.Group, null);   
        UT.CheckEquality<int>(e1.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e2.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e5.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e6.Group.EnemyCount, 6);

        EnemyGroup.Add(e5, e6);
        UT.CheckEquality<EnemyGroup>(e1.Group, e5.Group);
        UT.CheckDifference<EnemyGroup>(e5.Group, null);   
        UT.CheckEquality<EnemyGroup>(e5.Group, e6.Group);
        UT.CheckDifference<EnemyGroup>(e6.Group, null);   
        UT.CheckEquality<int>(e1.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e2.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e5.Group.EnemyCount, 6);
        UT.CheckEquality<int>(e6.Group.EnemyCount, 6);
    }

    public static void RemoveTest()
    {
        var e1 = new EnemyGroupUTDummy();
        var e2 = new EnemyGroupUTDummy();
        var e3 = new EnemyGroupUTDummy();

        EnemyGroup.Add(e1, e2);
        EnemyGroup.Add(e1, e3);

        UT.CheckEquality<int>(e1.Group.EnemyCount, 3);
        EnemyGroup.Remove(e2);
        UT.CheckEquality<int>(e1.Group.EnemyCount, 2);
        UT.CheckDifference<IEnemyGroup>(e2, null);
        UT.CheckEquality<EnemyGroup>(e2.Group, null);
        UT.CheckEquality<bool>(EnemyGroup.Contains(e1.Group, e2), false);

        EnemyGroup.Remove(e2);
        UT.CheckEquality<int>(e1.Group.EnemyCount, 2);
        UT.CheckDifference<IEnemyGroup>(e2, null);
        UT.CheckEquality<EnemyGroup>(e2.Group, null);
        UT.CheckEquality<bool>(EnemyGroup.Contains(e1.Group, e2), false);
    }
    
    public static void CalculateCenterTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        UT.CheckEquality<bool>(e1.Group.CalculateCenter() == new Vector3(2, 0, 0), true);

        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, -3));
        var e4 = new EnemyGroupUTDummy(new Vector3(2, 1, 0));
        var e5 = new EnemyGroupUTDummy(new Vector3(3, 2, 0));
        EnemyGroup.Add(e3, e4);
        EnemyGroup.Add(e3, e5);
        UT.CheckEquality<bool>(e3.Group.CalculateCenter() == new Vector3(2, 1, -1), true);

        var e6 = new EnemyGroupUTDummy(new Vector3(2, 3, 3));
        EnemyGroup.Add(e3, e6);
        EnemyGroup.Add(e1, e3);
        UT.CheckEquality<bool>(e3.Group.CalculateCenter() == new Vector3(2, 1, 0), true);
    }

    public static void MoveTest()
    {
        // Positive test
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();

        e1.Group.Move(center1, new Vector3(2, 0, 2), 1);
        UT.CheckEquality<bool>(e1.Velocity == new Vector3(0, 0, 1), true);
        UT.CheckEquality<bool>(e2.Velocity == new Vector3(0, 0, 1), true);
        
        // Negative test
        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e4 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e3, e4);
        Vector3 center2 =
            e3.Group.CalculateCenter();

        e3.Group.Move(center2, new Vector3(2, 0, -2), 1);
        UT.CheckEquality<bool>(e3.Velocity == new Vector3(0, 0, -1), true);
        UT.CheckEquality<bool>(e4.Velocity == new Vector3(0, 0, -1), true);

        // Diagonal Test
        var e5 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e6 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e5, e6);
        Vector3 center3 =
            e5.Group.CalculateCenter();

        e5.Group.Move(center3, new Vector3(0, 0, 2), 1);
        Vector2 diagonalTestDirection = 
            Matho.DirectionVectorFromAngle(135.0f);
        
        UT.CheckEquality<bool>(
            Matho.AngleBetween(e5.Velocity, new Vector3(diagonalTestDirection.x, 0, diagonalTestDirection.y)) < 5f, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(e6.Velocity, new Vector3(diagonalTestDirection.x, 0, diagonalTestDirection.y)) < 5f, true);

        // Vertical Test
        var e7 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e8 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e7, e8);
        Vector3 center4 =
            e7.Group.CalculateCenter();

        e7.Group.Move(center4, new Vector3(2, -2, 0), 1);
        UT.CheckEquality<bool>(e7.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e8.Velocity == new Vector3(0, 0, 0), true);

        // Origin Test
        var e9 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e10 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e9, e10);
        Vector3 center5 =
            e9.Group.CalculateCenter();

        e9.Group.Move(center5, new Vector3(2, 0, 0), 1);
        UT.CheckEquality<bool>(e9.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e10.Velocity == new Vector3(0, 0, 0), true);

        e9.Group.Move(center5, new Vector3(2 + offsetThreshold * 0.5f, 0, 0), 1);
        UT.CheckEquality<bool>(e9.Velocity == new Vector3(0, 0, 0), true);
        UT.CheckEquality<bool>(e10.Velocity == new Vector3(0, 0, 0), true);
    }

    public static void AbsoluteAngleBetweenTest()
    {
        float a1 = AbsoluteAngleBetween(Vector2.down, Vector2.right);
        UT.CheckEquality<bool>(Matho.IsInRange(a1, 1, UT.Threshold), true);

        float a2 = AbsoluteAngleBetween(Vector2.down, -Vector2.right);
        UT.CheckEquality<bool>(Matho.IsInRange(a2, 1, UT.Threshold), true);

        float a3 = AbsoluteAngleBetween(Vector2.down, Vector2.up);
        UT.CheckEquality<bool>(Matho.IsInRange(a3, 0, UT.Threshold), true);

        float a4 = AbsoluteAngleBetween(Vector2.down, (Vector2.right + Vector2.up).normalized);
        float a5 = AbsoluteAngleBetween(Vector2.down, (Vector2.right - Vector2.up).normalized);
        UT.CheckEquality<bool>(Matho.IsInRange(a4, a5, UT.Threshold), true);

        float a6 = AbsoluteAngleBetween(Vector2.down, (-Vector2.right + Vector2.up).normalized);
        float a7 = AbsoluteAngleBetween(Vector2.down, (-Vector2.right - Vector2.up).normalized);
        UT.CheckEquality<bool>(Matho.IsInRange(a6, a7, UT.Threshold), true);
    }

    public static void CalculateRotationConstantTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();
        
        // Cardinal tests
        float r1 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, 0, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r1, 1, UT.Threshold), true);

        float r2 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(5, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r2, 0, UT.Threshold), true);

        float r3 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(-5, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r3, 0, UT.Threshold), true);

        float r4 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, 0, 3));
        UT.CheckEquality<bool>(Matho.IsInRange(r4, 1, UT.Threshold), true);

        // Diagonal tests
        float r5 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(0, 0, -2));
        UT.CheckEquality<bool>(Matho.IsInRange(r5, 0.5f, UT.Threshold), true);

        float r6 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(0, 0, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r6, 0.5f, UT.Threshold), true);

        float r7 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 0, -2));
        UT.CheckEquality<bool>(Matho.IsInRange(r7, 0.5f, UT.Threshold), true);

        float r8 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 0, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r8, 0.5f, UT.Threshold), true);

        // Vertical tests
        float r9 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, 6, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r9, 0.5f, UT.Threshold), true);

        float r10 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(4, -6, 2));
        UT.CheckEquality<bool>(Matho.IsInRange(r10, 0.5f, UT.Threshold), true);

        float r11 = 
            e1.Group.CalculateRotationConstant(center1, new Vector3(2, -7, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r11, 1, UT.Threshold), true);

        // Empty test
        EnemyGroup linkGroup =
            e1.Group;

        EnemyGroup.Remove(e1);
        EnemyGroup.Remove(e2);
        Vector3 center2 =
            linkGroup.CalculateCenter();
        float r12 = 
            linkGroup.CalculateRotationConstant(center2, new Vector3(2, -7, -3));
        UT.CheckEquality<bool>(Matho.IsInRange(r12, 1, UT.Threshold), true);
        
        // In Crowd test
        var e3 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var e4 = new EnemyGroupUTDummy(new Vector3(3, 0, 0));

        EnemyGroup.Add(e3, e4);
        Vector3 center3 =
            e3.Group.CalculateCenter();
        
        // Cardinal tests
        float r13 = 
            e3.Group.CalculateRotationConstant(center3, new Vector3(2, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r13, 1, UT.Threshold), true);

        float r14 = 
            e3.Group.CalculateRotationConstant(center3, new Vector3(2.5f, 0, 0));
        UT.CheckEquality<bool>(Matho.IsInRange(r14, 0, UT.Threshold), true);
    }

    public static void RotateTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));

        EnemyGroup.Add(e1, e2);
        Vector3 center1 =
            e1.Group.CalculateCenter();

        // Cardinal tests
        e1.Group.Rotate(center1, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, -1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e2.Velocity, new Vector3(0, 0, 1), UT.Threshold), true);

        var e3 = new EnemyGroupUTDummy(new Vector3(0, 0, 1));
        var e4 = new EnemyGroupUTDummy(new Vector3(0, 0, -1));

        EnemyGroup.Add(e1, e3);
        EnemyGroup.Add(e1, e4);

        Vector3 center2 =
            e1.Group.CalculateCenter();
        e1.Group.Rotate(center1, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, -2), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e2.Velocity, new Vector3(0, 0, 2), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e3.Velocity, new Vector3(-1, 0, 0), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(e4.Velocity, new Vector3(1, 0, 0), UT.Threshold), true);

        // Diagonal test
        var d1 = new EnemyGroupUTDummy(new Vector3(0.5f, 0, 0.5f));
        var d2 = new EnemyGroupUTDummy(new Vector3(-0.5f, 0, 0.5f));
        var d3 = new EnemyGroupUTDummy(new Vector3(-0.5f, 0, -0.5f));
        var d4 = new EnemyGroupUTDummy(new Vector3(0.5f, 0, -0.5f));
        EnemyGroup.Add(d1, d2);
        EnemyGroup.Add(d2, d3);
        EnemyGroup.Add(d3, d4);
        Vector3 center3 =
            d1.Group.CalculateCenter();
        d1.Group.Rotate(center3, 1);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d1.Velocity, new Vector3(-Matho.Diagonal, 0, Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d2.Velocity, new Vector3(-Matho.Diagonal, 0, -Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d3.Velocity, new Vector3(Matho.Diagonal, 0, -Matho.Diagonal)) < UT.Threshold, true);
        UT.CheckEquality<bool>(
            Matho.AngleBetween(d4.Velocity, new Vector3(Matho.Diagonal, 0, Matho.Diagonal)) < UT.Threshold, true);

        // Center test
        var w1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var w2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));
        var w0 = new EnemyGroupUTDummy(Vector3.zero);
        EnemyGroup.Add(w0, w1);
        EnemyGroup.Add(w0, w2);
        Vector3 center4 =
            w1.Group.CalculateCenter();
        w1.Group.Rotate(center4, 1);
        UT.CheckEquality<bool>(Matho.IsInRange(w1.Velocity, new Vector3(0, 0, -1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(w2.Velocity, new Vector3(0, 0, 1), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.IsInRange(w0.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);
    } 

    public static void ExpandTest()
    {
        // Single bunch
        var e1 = new EnemyGroupUTDummy(new Vector3(0, 0, 0));

        // Duo bunch
        var e2 = new EnemyGroupUTDummy(new Vector3(5, 0, 3));
        var e3 = new EnemyGroupUTDummy(new Vector3(6, 0, 3));
        e2.NearbyEnemies.Add(e3);
        e3.NearbyEnemies.Add(e2);

        // Trio bunch
        var e4 = new EnemyGroupUTDummy(new Vector3(-5 - Matho.Diagonal, 0, -3 - Matho.Diagonal));
        var e5 = new EnemyGroupUTDummy(new Vector3(-5, 0, -3));
        var e6 = new EnemyGroupUTDummy(new Vector3(-5 + Matho.Diagonal, 0, -3 + Matho.Diagonal));
        e4.NearbyEnemies.Add(e5);
        e4.NearbyEnemies.Add(e6);

        e5.NearbyEnemies.Add(e4);
        e5.NearbyEnemies.Add(e6);

        e6.NearbyEnemies.Add(e4);
        e6.NearbyEnemies.Add(e5);

        EnemyGroup.Add(e1, e2);
        EnemyGroup.Add(e2, e3);
        EnemyGroup.Add(e3, e4);
        EnemyGroup.Add(e4, e5);
        EnemyGroup.Add(e5, e6);

        e1.Group.Expand(Vector3.zero, 1, 5);
        UT.CheckEquality<bool>(Matho.IsInRange(e1.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);

        UT.CheckEquality<bool>(Matho.AngleBetween(e2.Velocity, new Vector3(-1f, 0, 0)) < UT.Threshold * 10, true);
        UT.CheckEquality<bool>(Matho.AngleBetween(e3.Velocity, new Vector3(1f, 0, 0)) < UT.Threshold * 10, true);

        UT.CheckEquality<bool>(Matho.AngleBetween(e4.Velocity, new Vector3(-2 * Matho.Diagonal, 0, -2 * Matho.Diagonal)) < UT.Threshold * 10, true);
        UT.CheckEquality<bool>(Matho.IsInRange(e5.Velocity, new Vector3(0, 0, 0), UT.Threshold), true);
        UT.CheckEquality<bool>(Matho.AngleBetween(e6.Velocity, new Vector3(2 * Matho.Diagonal, 0, 2 * Matho.Diagonal)) < UT.Threshold * 10, true);
    }

    public static void AdjustResetAdjustTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));

        EnemyGroup.Add(e1, e2);
        UT.CheckEquality<bool>(e1.Group.adjustAvailable, true);
        UT.CheckEquality<bool>(e1.Velocity.magnitude < UT.Threshold, true);
        e1.Group.Adjust(new Vector3(5, 0, 0), 1, 1, 1, 1, 0, 0);
        UT.CheckEquality<bool>(e1.Group.adjustAvailable, false);
        UT.CheckEquality<bool>(e1.Velocity.magnitude > UT.Threshold, true);
        e1.Group.ResetAdjust();
        UT.CheckEquality<bool>(e1.Group.adjustAvailable, true);
        UT.CheckEquality<bool>(e1.Velocity.magnitude < UT.Threshold, true);

        var e3 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var e4 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));

        EnemyGroup.Add(e3, e4);
        e3.Group.Adjust(new Vector3(0, 0, 5), 0, 1, 0, 1, 0, 0);
        UT.CheckEquality<bool>(e3.Velocity.magnitude < UT.Threshold, true);
        e3.Group.ResetAdjust();
        e3.Group.Adjust(new Vector3(5, 0, 0), 0, 1, 0, 1, 0, 0);
        UT.CheckEquality<bool>(e3.Velocity.magnitude > UT.Threshold, true);
    }

    public static void AttackingGroupTest()
    {
        var e1 = new EnemyGroupUTDummy(new Vector3(-1, 0, 0));
        var e2 = new EnemyGroupUTDummy(new Vector3(1, 0, 0));

        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 0);
        AddAttacking(e1);
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 1);
        AddAttacking(e2);
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 2);
        RemoveAttacking(e1);
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 1);
        RemoveAttacking(e1);    
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 1);
        RemoveAttacking(e2);    
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 0);
        RemoveAttacking(e2);    
        UT.CheckEquality<int>(AttackingGroup.enemies.Count, 0);
        AttackingGroup.Adjust(Vector3.zero, 0, 1, 2, 3, 0, 0);
    }
}