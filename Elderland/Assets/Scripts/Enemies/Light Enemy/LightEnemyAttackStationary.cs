using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemyAttackStationary : StateMachineBehaviour
{
    private LightEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<LightEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
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
            
            FollowTransition();
            AttackTransition();
            
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void RotateTowardsPlayer()
    {
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - manager.transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(manager.transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        manager.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
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
