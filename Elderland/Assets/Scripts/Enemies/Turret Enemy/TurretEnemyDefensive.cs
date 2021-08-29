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
        manager.HealthbarLockIndicator.SetActive(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            distanceToPlayer = manager.DistanceToPlayer();

            manager.RotateTowardsDefault();
            SearchTransition();

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
        }
    }

    private void SearchTransition()
    {
        float playerForwardAngle = 
            Matho.AngleBetween(
                Matho.StdProj3D(PlayerInfo.Player.transform.position - manager.transform.position), 
                Matho.StdProj3D(manager.WallForward));
        if (distanceToPlayer >= manager.DefensiveRadius + manager.DefensiveRadiusMargin ||
            playerForwardAngle > TurretEnemyManager.DefensiveWallAngle)
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
        manager.HealthbarLockIndicator.SetActive(false);
    }
}