using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAttackWaiting : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;

    private bool exiting;

    private Vector3 losPosition;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<RangedEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
        lastDistanceToPlayer = DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        losPosition = PlayerInfo.Player.transform.position;
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
                WakeUpTransition();
            
            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
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

    private void WakeUpTransition()
    {
        if (manager.HasClearPlacement() ||
            (manager.waitingParent == null && distanceToPlayer > 7f + 1.5f) ||
            (manager.waitingParent != null && !manager.waitingParent.GetComponent<Animator>().GetBool("waiting")))
        {
            WakeUpExit();
        }
    }

    private void WakeUpExit()
    {
        manager.Animator.SetBool("waiting", false);
        manager.waitingParent = null;
        exiting = true;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 targetForward = Matho.StandardProjection3D(losPosition - manager.transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1.1f * Time.deltaTime, 0f);
        manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private float DistanceToPlayer()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(manager.transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);
        return horizontalDistanceToPlayer;
    }
}