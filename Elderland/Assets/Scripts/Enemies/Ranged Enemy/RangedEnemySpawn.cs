using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemySpawn : StateMachineBehaviour
{
    private bool exiting;
    private RangedEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<RangedEnemyManager>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            DelegateToGroup();
            exiting = true;
            manager.ClampToGround();
        }
	}

    private void DelegateToGroup()
    {
        if (manager.IsSubscribeToAttackValid())
        {
            manager.SubscribeToAttack();
        } 
        else
        {
            EnemyManager otherManager = manager.FindOverrideAttacker();
            if (otherManager != null)
            {
                manager.OverrideAttacker(otherManager);
            }
            else
            {
                manager.SubscribeToWatch();
            }
        }
    }
}
