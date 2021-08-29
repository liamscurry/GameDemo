using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Behaviour needed in order to cause enemy to return to spawn location when reaching too far away 
// from encounter.
public class GruntEnemyBoundsAway : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    private Vector3 startPosition;
    private float slowTurnTimer;
    private const float slowTurnDuration = 3.5f;

    private float startSpeed;
    private bool slowTurnComplete;

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
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;

        startPosition = manager.transform.position;

        manager.AttackingPlayer = false;
        EnemyGroup.RemoveAttacking(manager);

        slowTurnTimer = 0;
        startSpeed = manager.Agent.speed;
        manager.Agent.speed = startSpeed * 0.33f;
        manager.Agent.updateRotation = false;
        manager.StatsManager.MovespeedMultiplier.AddModifier(0.33f);
        slowTurnComplete = false;
    }

    private void OnStateExitImmediate()
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
            remainingDistance = manager.Agent.remainingDistance;

            if (!exiting)
                ApproachTransition();
            if (!exiting)
                BoundsWaitTransition();

            if (!exiting)
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

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        manager.AttackingPlayer = true;
    }

    /*
    Rotation function that initially rotates the enemy towards the player, walking backwards. After
    a duration, the enemy then rotates towards its path.

    Inputs:
    None

    Outputs:
    None
    */
    private void UpdateSlowTurn()
    {
        slowTurnTimer += Time.deltaTime;
        if (slowTurnTimer < slowTurnDuration)
        {
            RotateTowardsPlayer();
        }
        else
        {
            slowTurnComplete = true;
            manager.Agent.updateRotation = true;
            manager.Agent.speed = startSpeed;
            manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.33f);
        }
    }

    private void ApproachTransition()
    {
        float distanceFromStart = 
            Matho.StdProj2D(startPosition - manager.transform.position).magnitude; 
        if (distanceToPlayer < Encounter.EngageEnemyDistance &&
            distanceFromStart > Encounter.EngageStartDistance)
        {
            ApproachExit();
        }
    }

    private void ApproachExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToFarFollow);
        exiting = true;

        manager.Agent.updateRotation = true;
        manager.Agent.speed = startSpeed;
        manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.33f);
    }

    private void BoundsWaitTransition()
    {
        float distanceFromSpawn = 
            Matho.StdProj2D(manager.EncounterSpawn.spawnPosition - manager.transform.position).magnitude; 
        if (distanceFromSpawn < Encounter.BoundsWaitDistance)
        {
            BoundsWaitExit();
        }
    }

    private void BoundsWaitExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.BoundsWait);
        exiting = true;
    }

    /*
    Rotates the enemy towards the player when initially in the state.

    Inputs:
    None

    Outputs:
    None
    */
    private void RotateTowardsPlayer()
    {
        Vector3 targetForward = PlayerInfo.Player.transform.position - manager.transform.position;
        targetForward = Matho.StdProj3D(targetForward).normalized;
        Vector3 forward =
            Vector3.RotateTowards(
                manager.transform.forward,
                targetForward,
                Encounter.BoundsWaitRotSpeed * Time.deltaTime,
                0f);
        manager.transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);
    }
}