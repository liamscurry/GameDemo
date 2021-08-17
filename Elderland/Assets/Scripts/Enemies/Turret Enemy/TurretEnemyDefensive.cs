using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TurretEnemyDefensive : StateMachineBehaviour
{
    private TurretEnemyManager manager;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<TurretEnemyManager>();
        }

        exiting = false;

        manager.ChooseNextAbility();

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;
        manager.InDefensive = true;
        //manager.StatsManager.DamageTakenMultiplier.AddModifier(0);
        manager.MainHitbox.SetActive(false);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            distanceToPlayer = manager.DistanceToPlayer();

            RotateTowardsDefault();
            SearchTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
        }
    }

    private void RotateTowardsDefault()
    {
        Vector3 incrementedForward =
            Vector3.RotateTowards(
                manager.CannonGameObject.transform.forward,
                manager.WallForward,
                manager.DefensiveRotateSpeed * Time.deltaTime, 0);

        manager.CannonGameObject.transform.rotation = 
            Quaternion.LookRotation(incrementedForward, Vector3.up);
    }

    private void SearchTransition()
    {
        if (distanceToPlayer >= manager.DefensiveRadius + manager.DefensiveRadiusMargin)
        {
            SearchExit();
        }
    }

    private void SearchExit()
    {
        manager.Animator.SetTrigger("toSearch");
        exiting = true;
        manager.InDefensive = false;
        manager.MainHitbox.SetActive(true);
        //manager.StatsManager.DamageTakenMultiplier.RemoveModifier(0);
    }
}