using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TurretEnemySearch : StateMachineBehaviour
{
    private TurretEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;
    private float lastRemainingDistance;
    private float remainingDistance;

    private bool hasLOS;
    private bool passiveSearch;
    private int passiveSearchSign;

    private const float passiveAngleThreshold = 1;
    private const float activeAngleThreshold = 3;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<TurretEnemyManager>();
            passiveSearchSign = 1;
            hasLOS = false;
            passiveSearch = true;
        }

        checkTimer = checkDuration;
        exiting = false;

        manager.ChooseNextAbility();

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
        lastRemainingDistance = distanceToPlayer;
        remainingDistance = distanceToPlayer;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();
            remainingDistance = manager.Agent.remainingDistance;

            if (checkTimer > checkDuration)
            {
                CheckLOS();
            }

            if (!exiting)
                DefensiveTransition();
            if (!exiting)
            {
                if (passiveSearch)
                {
                    PassiveRotate();
                    IsPlayerSeen();
                }
                else
                {
                    ActiveRotate();
                    IsPlayerObscured();
                }
            }

            lastDistanceToPlayer = distanceToPlayer;
            lastRemainingDistance = remainingDistance;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void CheckLOS()
    {
        hasLOS =
            !Physics.Linecast(
                manager.MeshParent.transform.position,
                PlayerInfo.Player.transform.position,
                LayerConstants.GroundCollision);
    }

    private void PassiveRotate()
    {
        Vector3 targetForward =
            manager.WallRight * passiveSearchSign;
        
        Vector3 incrementedForward =
            Vector3.RotateTowards(manager.MeshParent.transform.forward, targetForward, manager.PassiveSearchSpeed * Time.deltaTime, 0);

        manager.MeshParent.transform.rotation = 
            Quaternion.LookRotation(incrementedForward, Vector3.up);
        
        float targetAngle = 
            Matho.AngleBetween(incrementedForward, targetForward);
        if (targetAngle < passiveAngleThreshold)
        {
            passiveSearchSign *= -1;
        }
    }

    private void IsPlayerSeen()
    {
        Vector3 playerDisplacement =
            PlayerInfo.Player.transform.position - manager.MeshParent.transform.position;
        playerDisplacement = 
            Matho.StandardProjection3D(playerDisplacement).normalized;
        float playerAngle = 
            Matho.AngleBetween(manager.MeshParent.transform.forward, playerDisplacement);
        if (hasLOS && playerAngle < manager.PassiveSearchConeAngle / 2f)
        {
            passiveSearch = false;
        }
    }

    private void ActiveRotate()
    {
        Vector3 targetForward =
            PlayerInfo.Player.transform.position - manager.MeshParent.transform.position;
        targetForward = 
            Matho.StandardProjection3D(targetForward).normalized;
        
        Vector3 incrementedForward =
            Vector3.RotateTowards(manager.MeshParent.transform.forward, targetForward, manager.ActiveSearchSpeed * Time.deltaTime, 0);

        manager.MeshParent.transform.rotation = 
            Quaternion.LookRotation(incrementedForward, Vector3.up);
        
        float targetAngle = 
            Matho.AngleBetween(incrementedForward, targetForward);
        if (targetAngle < activeAngleThreshold)
        {
            AttackExit();
        }
    }

    private void IsPlayerObscured()
    {
        if (!hasLOS)
        {
            passiveSearch = true;
        }
    }

    private void AttackExit()
    {
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        exiting = true;
    }

    private void DefensiveTransition()
    {
        if (distanceToPlayer < manager.DefensiveRadius)
        {
            DefensiveExit();
        }
    }

    private void DefensiveExit()
    {
        manager.Animator.SetTrigger("toDefensive");
        exiting = true;
    }
}