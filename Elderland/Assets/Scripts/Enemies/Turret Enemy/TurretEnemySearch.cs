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

    private bool hasLOS;
    private bool passiveSearch;
    private int passiveSearchSign;

    private const float passiveAngleThreshold = 1;
    private const float activeAngleThreshold = 3;

    private float attackWaitTimer;
    private const float attackWaitDuration = 1f;

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
        attackWaitTimer = 0;

        manager.ChooseNextAbility();

        lastDistanceToPlayer = manager.DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = manager.DistanceToPlayer();

            attackWaitTimer += Time.deltaTime;

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
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void CheckLOS()
    {
        hasLOS =
            !Physics.Linecast(
                manager.CannonGameObject.transform.position,
                PlayerInfo.Player.transform.position,
                LayerConstants.GroundCollision);
    }

    private void PassiveRotate()
    {
        Vector3 targetForward =
            manager.WallRight * passiveSearchSign;
        targetForward =
            Vector3.RotateTowards(targetForward, manager.WallForward, 5f * Mathf.Deg2Rad, Mathf.Infinity);

        Vector3 incrementedForward =
            Vector3.RotateTowards(manager.CannonParentForward, targetForward, manager.PassiveSearchSpeed * Time.deltaTime, 0);

        manager.CannonParentRotation = 
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
            PlayerInfo.Player.transform.position - manager.CannonGameObject.transform.position;
        playerDisplacement = 
            Matho.StdProj3D(playerDisplacement).normalized;
        float playerAngle = 
            Matho.AngleBetween(manager.CannonParentForward, playerDisplacement);
        if (hasLOS && playerAngle < manager.PassiveSearchConeAngle / 2f)
        {
            passiveSearch = false;
        }
    }

    private void ActiveRotate()
    {
        Vector3 targetForward =
            PlayerInfo.Player.transform.position - manager.CannonGameObject.transform.position;
        targetForward = 
            Matho.StdProj3D(targetForward).normalized;
        
        Vector3 incrementedForward =
            Vector3.RotateTowards(manager.CannonParentForward, targetForward, manager.ActiveSearchSpeed * Time.deltaTime, 0);

        manager.CannonParentRotation = 
            Quaternion.LookRotation(incrementedForward, Vector3.up);
        
        float targetAngle = 
            Matho.AngleBetween(incrementedForward, targetForward);
        if (targetAngle < activeAngleThreshold && attackWaitTimer > attackWaitDuration)
        {
            AttackExit();
        }
    }

    private void IsPlayerObscured()
    {
        if (!hasLOS)
        {
            passiveSearch = true;

            // Need to check to see if rotation is towards wall from active search
            // Without this, the turret may rotate passively towards the wall instead of away from it.
            if (Matho.AngleBetween(manager.CannonParentForward, manager.WallForward) > 90f)
            {
                float currentAngleDir =
                    Matho.AngleBetween(manager.CannonParentForward, manager.WallRight);
                if (currentAngleDir < 90f && passiveSearchSign == -1)
                {
                    passiveSearchSign = 1;
                }
                else if (currentAngleDir < 90f && passiveSearchSign == 1)
                {
                    passiveSearchSign = -1;
                }
            }
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