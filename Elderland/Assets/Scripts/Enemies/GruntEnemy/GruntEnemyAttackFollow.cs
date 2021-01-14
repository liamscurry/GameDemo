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
        EnemyGroup.AddAttacking(manager);
        
        manager.NearbySensor.transform.localScale *= 3;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            
            if (!manager.IsInNextAttackMax())
            {
                //MoveTowardsPlayer(checkTimer > checkDuration);
                EnemyGroup.AttackingGroup.Adjust(
                    PlayerInfo.Player.transform.position,
                    0,
                    0,
                    GruntEnemyManager.ExpandSpeed * 2 * Time.deltaTime,
                    manager.NearbySensor.Radius,
                    true);
                manager.Agent.Move(manager.Velocity);

                Vector3 moveDirection = 
                    PlayerInfo.Player.transform.position - manager.transform.position;
                moveDirection.Normalize();
                manager.Agent.Move(moveDirection * manager.AttackFollowSpeed * Time.deltaTime);
            }

            manager.RotateLocallyTowardsPlayer();

            manager.ClampToGround();
            
            GroupTransition();
            if (!exiting)
                AttackTransition();

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void MoveTowardsPlayer(bool timerReady)
    {
        if (!manager.IsInNextAttackMax())
        {
            
            if (timerReady)
            {
                manager.UpdateAgentPath();
            }
            //MoveAwayFromAttackingEnemies();
            /*
            Vector3 moveDirection = 
                PlayerInfo.Player.transform.position - manager.transform.position;
            moveDirection.Normalize();
            manager.Agent.Move(moveDirection * manager.AttackFollowSpeed * Time.deltaTime);
            */
        }
        else
        {
            if (manager.Agent.hasPath)
                manager.Agent.ResetPath();
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

    private void MoveAwayFromAttackingEnemies()
    {
        Vector3 compositeExpansion = 
            Vector3.zero;
        foreach (EnemyManager enemy in EnemyGroup.AttackingEnemies)
        {
            Vector3 offset = (manager.transform.position - enemy.transform.position);
            offset = Matho.StandardProjection3D(offset);
            if (offset.magnitude < manager.NearbySensor.Radius)
            {
                //offset = Matho.Rotate(offset, Vector3.up, 90f);
                float speed = GruntEnemyManager.ExpandSpeed * 2f;
                speed *= Mathf.Clamp01(1 - (offset.magnitude / manager.NearbySensor.Radius));
                compositeExpansion +=
                    offset.normalized * speed * Time.deltaTime;
            }
        }
        manager.Agent.Move(compositeExpansion);
    }
}