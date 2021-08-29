using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Behaviour that makes the enemy idle and wait to see if the player re-enters the encounter. If 
// the player does not, the enemy despawns.
public class GruntEnemyBoundsWait : StateMachineBehaviour
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
    private Vector3 startForward;

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
        if (manager.AgentPath.Length >= 2)
        {
            startForward =
                manager.AgentPath[manager.AgentPath.Length - 1] - manager.AgentPath[manager.AgentPath.Length - 2];
            startForward = Matho.StdProj3D(startForward).normalized;
            if (startForward.magnitude == 0)
                startForward = manager.transform.forward;
        }
        else
        {
            startForward = manager.transform.forward;
        }
    
        manager.Agent.ResetPath();

        manager.AttackingPlayer = false;
    }

    private void OnStateExitImmediate()
    {

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
                CheckForRecycle();
            if (!exiting)
                ApproachTransition();

            if (!exiting)
            {
                RotateOpposite();
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

    private void CheckForRecycle()
    {
        recycleTimer += Time.deltaTime;
        if (recycleTimer > Encounter.RecycleDuration)
        {
            manager.Recycle();
            exiting = true;
        }
    }

    private void ApproachTransition()
    {
        float distanceFromStart = 
            Matho.StdProj2D(startPosition - manager.transform.position).magnitude; 
        if (distanceToPlayer < Encounter.EngageEnemyDistance)
        {
            ApproachExit();
        }
    }

    private void ApproachExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToFarFollow);
        exiting = true;
    }

    /*
    Rotates the enemy opposite of the direction of the startinf forward vector when entering this state.
    This gives a more realistic behaviour an enemy reaching its idle spawn state and guarding it.

    Inputs:
    None

    Outputs:
    None
    */
    private void RotateOpposite()
    {
        Vector3 targetForward = -startForward;
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