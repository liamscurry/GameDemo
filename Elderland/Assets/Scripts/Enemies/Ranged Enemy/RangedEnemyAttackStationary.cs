using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAttackStationary : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private float lastDistanceToPlayer;
    private float distanceToPlayer;

    private bool exiting;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<RangedEnemyManager>();
        }

        lastDistanceToPlayer = DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        checkTimer = checkDuration;
        exiting = false;

        manager.TurnOnAgent();
        CalculateClosePath();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = DistanceToPlayer();

            RotateTowardsPlayer();
            manager.ClampToGround();
            
            if (!exiting)   
                DefensiveTransition();
            if (!exiting)     
                AttackTransition();

            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void CalculateClosePath()
    {
        Vector2 currentPosition = Matho.StandardProjection2D(manager.transform.position);
        Vector2 playerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        Vector2 closePosition = playerPosition + (currentPosition - playerPosition).normalized * (EnemyInfo.RangedArranger.radius - 1.5f);
        Vector3 closePositionNav = GameInfo.CurrentLevel.NavCast(closePosition);
        manager.Agent.SetDestination(closePositionNav);
        manager.Agent.updateRotation = false;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1.1f * Time.deltaTime, 0f);
        manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void DefensiveTransition()
    {
        if (manager.IsInDefensiveRange())
        {
            DefensiveExit();
        }
    }

    private void DefensiveExit()
    {
        manager.TurnOffAgent();
        manager.AbilityManager.CancelQueue();
        
        manager.NextAttack = manager.Slow;
        manager.Slow.Queue(EnemyAbilityType.First);
        //manager.Slow.Queue(EnemyAbilityType.Middle);
        manager.Slow.Queue(EnemyAbilityType.Last);
        manager.AbilityManager.StartQueue();

        manager.Animator.SetTrigger("defensiveStart");
        manager.Animator.SetBool("defensive", true);
        manager.Animator.ResetTrigger("runAbility");        
        exiting = true;
    }

    private void AttackTransition()
    {
        if (manager.Agent.remainingDistance < 0.5f + 0.05f)
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
        manager.TurnOffAgent();
        manager.AbilityManager.StartQueue();
        exiting = true;
    }

    private float DistanceToPlayer()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(manager.transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);
        return horizontalDistanceToPlayer;
    }
}