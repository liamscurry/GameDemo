using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemyAttackStationary : StateMachineBehaviour
{
    private LightEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;

    private Vector3 previousForward;
    private bool animatingRotation;
    private float animatingRotationSmooth;
    private float animatingRotationSmoothVelocity;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<LightEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
        previousForward = manager.transform.forward;
        animatingRotation = false;
        animatingRotationSmooth = 0;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            checkTimer += Time.deltaTime;

            if (manager.ArrangementNode != -1)
            {
                EnemyInfo.MeleeArranger.OverrideNode(manager);
            }

            RotateTowardsPlayer();
            manager.ClampToGround();
            
            //FollowTransition();
            //AttackTransition();
            
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void RotateTowardsPlayer()
    {
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        
        float rotationAngle = Matho.AngleBetween(manager.transform.forward, targetForward);
        if (rotationAngle > 10)
        {
            //rotate
            float currentRightAxisAngle =
                Matho.AngleBetween(manager.transform.right, targetForward);

            Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 1.5f * Time.deltaTime, 0f);
            manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            int rotationSign = 1;
            if (currentRightAxisAngle < 90f) // condition on 360 mark is not working, rest
                //of sign is working and the general turning works great, no hiccups from there.
            {
                rotationSign = -1;
            }

            animatingRotationSmooth =
            Mathf.SmoothDamp(animatingRotationSmooth,
                             -1 * rotationSign,
                             ref animatingRotationSmoothVelocity,
                             0.15f,
                             100f);

            manager.Animator.SetFloat("turning", animatingRotationSmooth);
        }
        else
        {
            //slow rotate
            Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 0.5f * Time.deltaTime, 0f);
            manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            animatingRotationSmooth =
            Mathf.SmoothDamp(animatingRotationSmooth,
                             0,
                             ref animatingRotationSmoothVelocity,
                             0.15f,
                             100f);

            manager.Animator.SetFloat("turning", animatingRotationSmooth);
        }

        /*
        int targetRotation = 0;
        int targetSign = 1;
        float dampSpeedModifier = 1f;
        if (animatingRotation)
        {
            float previousAxisAngle = Matho.Angle(Matho.StandardProjection2D(previousForward));
            previousForward = Vector3.RotateTowards(previousForward, manager.transform.forward, 2f * Time.deltaTime, 0f);
            float forwardAxisAngle = Matho.Angle(Matho.StandardProjection2D(previousForward));
            
            if (forwardAxisAngle < previousAxisAngle ||
                (previousAxisAngle < 90f && forwardAxisAngle > 270f))
            {
                targetSign = -1;
                Debug.Log("flipped: " + forwardAxisAngle + ", " + previousAxisAngle);
            }

            targetRotation = 1;
            dampSpeedModifier = 1;
        } */
    }

    private void FollowTransition()
    {
        if (manager.ArrangementNode != -1)
        {
            if ((manager.NextAttack == manager.Sword && (!manager.IsInNextAttackMax() || manager.IsInNextAttackMin())) ||
                (manager.NextAttack == manager.Charge && !manager.IsInNextAttackMax()))
            {
                FollowExit();
            }
        }
        else
        {
            FollowExit();
        }
    }

    private void FollowExit()
    {
        manager.Animator.SetTrigger("toFollow");
        exiting = true;
    }

    private void AttackTransition()
    {
        Vector3 playerEnemyDirection = (PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        float playerEnemyAngle = Matho.AngleBetween(Matho.StandardProjection2D(manager.transform.forward), Matho.StandardProjection2D(playerEnemyDirection));

        if (playerEnemyAngle < manager.NextAttack.AttackAngleMargin && manager.IsInNextAttackMax())
        {
            AttackExit();
        }
    }

    private void AttackExit()
    {
        manager.TurnOffAgent();
        manager.NextAttack.TryRun();
        manager.Animator.SetTrigger("runAbility");
        exiting = true;
    }
}
