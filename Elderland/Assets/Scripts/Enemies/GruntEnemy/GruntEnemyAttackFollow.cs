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
        manager.Agent.radius = manager.FightingAgentRadius;
        manager.Agent.stoppingDistance = manager.NextAttack.AttackDistance * 0.8f;
        if (!manager.PingedToGroup)
            EnemyGroup.AddAttacking(manager);
        
        manager.NearbySensor.transform.localScale =
            3 * manager.NearbySensor.BaseRadius * Vector3.one;
    }

    private void OnStateExitImmediate()
    {
        if (!exitingFromAttack)
        {
            EnemyGroup.AttackingEnemies.Remove(manager);
            manager.Agent.radius = manager.FollowAgentRadius;
            manager.Agent.stoppingDistance = 0;
            manager.Agent.ResetPath();
            EnemyGroup.RemoveAttacking(manager);
            manager.NearbySensor.transform.localScale = manager.NearbySensor.BaseRadius * Vector3.one;
        }
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
                PingedToGroupTransition();
            if (!exiting)
                GroupTransition();
            if (!exiting)
                AttackTransition();
            if (exiting)
            {
                OnStateExitImmediate();
            }

            if (!exiting)
            {
                MoveTowardsPlayer();
                manager.RotateLocallyTowardsPlayer();

                manager.ClampToGround();
            }

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!manager.IsInNextAttackMax())
        {
            EnemyGroup.AttackingGroup.Adjust(
                PlayerInfo.Player.transform.position,
                0,
                0,
                GruntEnemyManager.ExpandSpeed * 0.5f * Time.deltaTime,
                manager.NearbySensor.Radius,
                0,
                0,
                true);
            manager.Agent.Move(manager.Velocity);

            Vector3 moveDirection = 
                PlayerInfo.Player.transform.position - manager.transform.position;
            moveDirection.Normalize();
            manager.Agent.Move(moveDirection * manager.AttackFollowSpeed * Time.deltaTime);
        }
    }

    private void GroupTransition()
    {
        Vector2 horizontalOffset = 
            Matho.StdProj2D(PlayerInfo.Player.transform.position - manager.transform.position);
        if (horizontalOffset.magnitude > manager.AttackFollowRadius + manager.AttackFollowRadiusMargin)
        {
            GroupExit();
        }
    }

    private void GroupExit()
    {
        manager.Animator.SetTrigger("toGroupFollow");
        exiting = true;
    }

    private void AttackTransition()
    {
        if (manager.IsInNextAttackMax())
        {
            Vector3 playerEnemyDirection = (PlayerInfo.Player.transform.position - manager.transform.position).normalized;
            float playerEnemyAngle =
                Matho.AngleBetween(
                    Matho.StdProj2D(manager.transform.forward),
                    Matho.StdProj2D(playerEnemyDirection));

            if (playerEnemyAngle < manager.NextAttack.AttackAngleMargin)
            {
                AttackExit();
            }
        }
    }

    private void AttackExit()
    {
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        manager.Agent.ResetPath();
        exiting = true;
        exitingFromAttack = true;
    }

    private void PingedToGroupTransition()
    {
        if (manager.PingedToGroup)
        {
            PingedToGroupExit();
        }
    }

    private void PingedToGroupExit()
    {
        manager.PingedToGroup = false;
        manager.Animator.SetTrigger("toGroupFollow");
        exiting = true;
    }
}