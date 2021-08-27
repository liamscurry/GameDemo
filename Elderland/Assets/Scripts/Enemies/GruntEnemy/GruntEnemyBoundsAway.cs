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
        manager.Agent.updateRotation = true;

        startPosition = manager.transform.position;

        manager.AttackingPlayer = false;
    }

    private void OnStateExitImmediate()
    {
        manager.Agent.ResetPath();
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
                }

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

    private void ApproachTransition()
    {
        float distanceFromStart = 
            Matho.StdProj2D(startPosition - manager.transform.position).magnitude; 
        if (distanceToPlayer < Encounter.EngageEnemyDistance &&
            distanceFromStart > Encounter.EngageStartDistance)
        {
            ApproachExit();
            OnStateExitImmediate();
        }
    }

    private void ApproachExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToFarFollow);
        exiting = true;
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
}