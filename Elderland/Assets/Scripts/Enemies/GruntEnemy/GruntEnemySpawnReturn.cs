using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Behaviour needed in order to cause enemy to return to spawn location when reaching too far away 
// from encounter.
public class GruntEnemySpawnReturn : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    private float recycleTimer;

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

        manager.ChooseNextAbility();

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;
        manager.Agent.updateRotation = true;
        recycleTimer = 0;

        startPosition = manager.transform.position;

        manager.AttackingPlayer = false;
    }

    private void OnStateExitImmediate()
    {
        manager.GroupSensor.Reset();
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

            if (checkTimer > checkDuration)
            {
                manager.UpdateSpawnPath();
            }

            manager.ClampToGround();

            if (!exiting)
                CheckForRecycle();
            // Need to make transition to normal approach again if given a set of conditions.
            if (!exiting)
                ApproachTransition();
            if (exiting)
            {
                OnStateExitImmediate();
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

    private void CheckForRecycle()
    {
        if (distanceToPlayer > Encounter.RecycleDistance)
        {
            recycleTimer += Time.deltaTime;
            if (recycleTimer > Encounter.RecycleDuration)
            {
                manager.Recycle();
                exiting = true;
            }
        }
        else
        {
            recycleTimer = 0;
        }
    }

    private void ApproachTransition()
    {
        float distanceFromStart = 
            Matho.StdProj2D(startPosition - manager.transform.position).magnitude; 
        if (distanceToPlayer < Encounter.EngageEnemyDistance && distanceFromStart > Encounter.EngageStartDistance)
        {
            ApproachExit();
        }
    }

    private void ApproachExit()
    {
        manager.Animator.SetTrigger("toAttack");
        exiting = true;
    }
}