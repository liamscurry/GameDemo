using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// In the attack follow state, enemies are apart of the attacking enemies enemy group.
// When they leave the state, they are not apart of the group (unless they are in attacking ability). 
// When they enter the state they are
// a part of the group. This way there are only a max of maximum enemies looking to attack the player at
// a given time.
public class GruntEnemyAttackFollow : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private bool exitingFromAttack;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        manager.BehaviourLock = this;

        checkTimer = checkDuration;

        exiting = false;
        exitingFromAttack = false;

        EnemyGroup.OnAttackFollowEnter(manager);
    }

    private void OnStateExitImmediate()
    {
        EnemyGroup.OnAttackFollowImmediateExit(manager);
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
            
            if (!exiting)
                EnemyGroup.AttackFollowToGroupFollowTransition(manager, ref exiting, ref exitingFromAttack);
            if (!exiting)
                EnemyGroup.AttackFollowToAttackTransition(manager, ref exiting, ref exitingFromAttack);

            if (!exiting)
            {
                EnemyGroup.UpdateAttackFollowMovement(manager);
                EnemyGroup.UpdateAttackFollowRotation(manager);
                manager.ClampToGround();
            }

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }
}