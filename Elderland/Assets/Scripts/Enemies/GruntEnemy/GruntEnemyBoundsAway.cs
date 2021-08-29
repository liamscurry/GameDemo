using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Generic behaviour needed in order to cause enemy to return to spawn location when reaching too far away 
// from encounter. Can be overriden.
public class GruntEnemyBoundsAway : StateMachineBehaviour
{
    protected GruntEnemyManager manager;

    protected float checkTimer;
    protected const float checkDuration = 0.5f;

    protected bool exiting;
    protected float lastDistanceToPlayer;
    protected float distanceToPlayer;

    protected Vector3 startPosition; // position of enemy on enter
    protected float slowTurnTimer; // timer used to track when to face player or not.
    protected const float slowTurnDuration = 3.5f; 

    protected float startSpeed; // storage holding default agent speed.
    protected bool slowTurnComplete; // indicator that the enemy is facing the player or not.

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        manager.BehaviourLock = this;

        checkTimer = checkDuration;
        exiting = false;

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        OnBoundsAwayEnter();
    }

    /*
    Follows enemy state immediate pattern. See EnemyManager.cs for more details.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void OnStateExitImmediate()
    {
        manager.Agent.ResetPath();

        manager.Agent.updateRotation = true;
        manager.Agent.speed = startSpeed;
        manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.33f);
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

            if (!exiting)
                ApproachTransition();
            if (!exiting)
                BoundsWaitTransition();

            if (!exiting)
            {
                UpdateBoundsAway();
            }

            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    // Transitions //
    protected virtual void ApproachTransition()
    {
        float distanceFromStart = 
            Matho.StdProj2D(startPosition - manager.transform.position).magnitude; 
        if (distanceToPlayer < Encounter.EngageEnemyDistance &&
            distanceFromStart > Encounter.EngageStartDistance)
        {
            ApproachExit();
        }
    }

    protected virtual void ApproachExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToFarFollow);
        exiting = true;

        manager.Agent.updateRotation = true;
        manager.Agent.speed = startSpeed;
        manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.33f);
    }

    protected virtual void BoundsWaitTransition()
    {
        float distanceFromSpawn = 
            Matho.StdProj2D(manager.EncounterSpawn.spawnPosition - manager.transform.position).magnitude; 
        if (distanceFromSpawn < Encounter.BoundsWaitDistance)
        {
            BoundsWaitExit();
        }
    }

    protected virtual void BoundsWaitExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.BoundsWait);
        exiting = true;
    }
    
    /*
    Sets enemy group fields on enter.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void OnBoundsAwayEnter()
    {
        startPosition = manager.transform.position;
        EnemyGroup.RemoveAttacking(manager);
        slowTurnTimer = 0;
        slowTurnComplete = false;
        startSpeed = manager.Agent.speed;
        manager.Agent.speed = startSpeed * 0.33f;
        manager.Agent.updateRotation = false;
        manager.StatsManager.MovespeedMultiplier.AddModifier(0.33f);
    }

    /*
    Updates enemy group movement and fields.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void UpdateBoundsAway()
    {
        if (checkTimer > checkDuration)
        {
            manager.UpdateSpawnPath();
            manager.AgentPath = manager.Agent.path.corners;
        }
        if (!slowTurnComplete)
            UpdateSlowTurn();

        manager.ClampToGround();
    }

    /*
    Rotation function that initially rotates the enemy towards the player, walking backwards. After
    a duration, the enemy then rotates towards its path goal.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void UpdateSlowTurn()
    {
        slowTurnTimer += Time.deltaTime;
        if (slowTurnTimer < slowTurnDuration)
        {
            EnemyGroup.RotateTowardsPlayer(manager, Encounter.BoundsWaitRotSpeed);
        }
        else
        {
            slowTurnComplete = true;
            manager.Agent.updateRotation = true;
            manager.Agent.speed = startSpeed;
            manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.33f);
        }
    }
}