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
        manager.Agent.radius = manager.FightingAgentRadius;
        manager.Agent.stoppingDistance = manager.NextAttack.AttackDistance * 0.8f;
        if (!manager.PingedToGroup)
            EnemyGroup.AddAttacking(manager);
        
        manager.NearbySensor.transform.localScale =
            3 * manager.NearbySensor.BaseRadius * Vector3.one;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            
            MoveTowardsPlayer();
            manager.RotateLocallyTowardsPlayer();

            manager.ClampToGround();
            
            
            if (!exiting)
                PingedToGroupTransition();
            if (!exiting)
                GroupTransition();
            if (!exiting)
                AttackTransition();

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
        manager.Agent.radius = manager.FollowAgentRadius;
        manager.Agent.stoppingDistance = 0;
        manager.Agent.ResetPath();
        EnemyGroup.RemoveAttacking(manager);
        manager.NearbySensor.transform.localScale = manager.NearbySensor.BaseRadius * Vector3.one;
        exiting = true;
    }

    private void AttackTransition()
    {
        if (manager.IsInNextAttackMax())
        {
            Vector3 playerEnemyDirection = (PlayerInfo.Player.transform.position - manager.transform.position).normalized;
            float playerEnemyAngle =
                Matho.AngleBetween(
                    Matho.StandardProjection2D(manager.transform.forward),
                    Matho.StandardProjection2D(playerEnemyDirection));

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
        manager.Agent.radius = manager.FollowAgentRadius;
        manager.Agent.stoppingDistance = 0;
        manager.Agent.ResetPath();
        manager.NearbySensor.transform.localScale = manager.NearbySensor.BaseRadius * Vector3.one;
        exiting = true;
    }
}