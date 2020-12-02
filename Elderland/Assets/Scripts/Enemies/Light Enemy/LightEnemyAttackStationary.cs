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
        Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 2f * Time.deltaTime, 0f);
        manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        float previousAngle = Matho.AngleBetween(previousForward, forward);
        if (!animatingRotation && previousAngle > 0.5f)
        {
            animatingRotation = true;
        }
        else if (animatingRotation && previousAngle < 0.1f)
        {
            previousForward = forward;
            animatingRotation = false;
        }

        int targetRotation = 0;
        int targetSign = 1;
        if (animatingRotation)
        {
            previousForward = Vector3.RotateTowards(previousForward, forward, 2f * Time.deltaTime, 0f);

            float forwardAxisAngle = Matho.Angle(Matho.StandardProjection2D(forward));
            float previousAxisAngle = Matho.Angle(Matho.StandardProjection2D(previousForward));
            if (forwardAxisAngle < previousAxisAngle ||
                (forwardAxisAngle < 90f && previousAxisAngle > 270f))
            {
                targetSign = -1;
            }

            targetRotation = 1;
        } 

        targetRotation *= targetSign * -1;

        animatingRotationSmooth =
            Mathf.SmoothDamp(animatingRotationSmooth, targetRotation, ref animatingRotationSmoothVelocity, 0.3f, 100f);
        manager.Animator.SetFloat("turning", animatingRotationSmooth);
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
