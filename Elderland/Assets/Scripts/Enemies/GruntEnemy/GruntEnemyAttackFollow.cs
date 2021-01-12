using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GruntEnemyAttackFollow : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        checkTimer = checkDuration;

        exiting = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;

            MoveTowardsPlayer();
            manager.RotateLocallyTowardsPlayer();

            manager.ClampToGround();
            
            GroupTransition();

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 moveDirection = 
            PlayerInfo.Player.transform.position - manager.transform.position;
        moveDirection.Normalize();
        manager.Agent.Move(moveDirection * manager.AttackFollowSpeed * Time.deltaTime);
    }

    private void GroupTransition()
    {
        Vector2 horizontalOffset = 
            Matho.StandardProjection2D(PlayerInfo.Player.transform.position - manager.transform.position);
        if (horizontalOffset.magnitude > manager.AttackFollowRadius + manager.AttackFollowRadiusMargin)
        {
            GroupExit();
        }
    }

    private void GroupExit()
    {
        EnemyGroup.AttackingEnemies.Remove(manager);
        manager.Animator.SetTrigger("toGroupFollow");
        exiting = true;
    }

    private void AttackExit()
    {
        manager.TurnOffAgent();
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        exiting = true;
    }
}