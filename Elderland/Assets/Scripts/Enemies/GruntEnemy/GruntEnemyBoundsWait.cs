using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Generic behaviour that makes the enemy idle and wait to see if the player re-enters the encounter. If 
// the player does not, the enemy despawns. Can be overriden.
public class GruntEnemyBoundsWait : StateMachineBehaviour
{
    protected GruntEnemyManager manager;

    protected float checkTimer;
    protected const float checkDuration = 0.5f;

    protected bool exiting;
    protected float lastDistanceToPlayer;
    protected float distanceToPlayer;

    protected float recycleTimer;

    protected Vector3 startForward; // Vector that stores the player's forward rotation when entering the state.

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

        OnBoundsWaitEnter();
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
                CheckForRecycle();
            if (!exiting)
                ApproachTransition();

            if (!exiting)
            {
                RotateOpposite();
                manager.ClampToGround();
            }

            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    /*
    Recycles the enemy in an encounter after a duration.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void CheckForRecycle()
    {
        recycleTimer += Time.deltaTime;
        if (recycleTimer > Encounter.RecycleDuration)
        {
            manager.Recycle();
            exiting = true;
        }
    }

    // Transitions //
    protected virtual void ApproachTransition()
    {
        if (distanceToPlayer < Encounter.EngageEnemyDistance)
        {
            ApproachExit();
        }
    }

    protected virtual void ApproachExit()
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToFarFollow);
        exiting = true;
    }

    /*
    Sets enemy group fields on enter.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void OnBoundsWaitEnter()
    {
        manager.Agent.updateRotation = true;
        recycleTimer = 0;

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
    }

    /*
    Rotates the enemy opposite of the direction of the startinf forward vector when entering this state.
    This gives a more realistic behaviour an enemy reaching its idle spawn state and guarding it.

    Inputs:
    None

    Outputs:
    None
    */
    protected virtual void RotateOpposite()
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