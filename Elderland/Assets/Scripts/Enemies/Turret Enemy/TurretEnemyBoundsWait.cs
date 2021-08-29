using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Non agent turret variant of generic BoundsWait behaviour
public sealed class TurretEnemyBoundsWait : GruntEnemyBoundsWait
{
    // Overrides to not consider agent component.
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<EnemyManager>();
        }

        manager.BehaviourLock = this;

        checkTimer = checkDuration;
        exiting = false;

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        animator.SetBool(AnimationConstants.Enemy.InBoundsReturn, true);

        recycleTimer = 0;
    }

    // Overrides to not consider agent component.
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager.BehaviourLock != this && !exiting)
        {
            OnStateExitImmediate(animator);
            exiting = true;
        }

        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();

            if (!exiting)
                CheckForRecycle();
            if (!exiting)
                ApproachTransition(animator);

            if (!exiting)
            {
                ((TurretEnemyManager) manager).RotateTowardsDefault();
            }
            
            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    // Overrides to make recycle duration longer.
    protected override void CheckForRecycle()
    {
        recycleTimer += Time.deltaTime;
        if (recycleTimer > Encounter.RecycleDuration * 3)
        {
            manager.Recycle();
            exiting = true;
        }
    }

    // Overrides for animation property change.
    protected override void ApproachExit(Animator animator)
    {
        manager.Animator.SetTrigger(AnimationConstants.Enemy.ToSearch);
        exiting = true;
        animator.SetBool(AnimationConstants.Enemy.InBoundsReturn, false);
    }

}