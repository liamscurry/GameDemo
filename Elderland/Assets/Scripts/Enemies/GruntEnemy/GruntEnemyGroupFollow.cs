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

        manager.BehaviourLock = this;

        checkTimer = checkDuration;
        exiting = false;
        distanceToPlayer = manager.DistanceToPlayer();

        EnemyGroup.OnGroupFollowEnter(manager);
    }

    private void OnStateExitImmediate()
    {
        EnemyGroup.OnGroupFollowImmediateExit(manager);
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
            
            EnemyGroup.UpdateGroupFollowMovement(manager, checkTimer > checkDuration);
            EnemyGroup.UpdateGroupFollowRotation(manager);
            manager.ClampToGround();

            //if (!exiting)
            //    PingTransition();
            if (!exiting)
                EnemyGroup.FarFollowTransition(manager, ref exiting);
            if (!exiting)
                EnemyGroup.OverrideAttackTransition(manager, ref exiting);
            if (!exiting)
                EnemyGroup.AttackTransition(manager, ref exiting);
            if (exiting)
            {
                OnStateExitImmediate();
            }

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }
}