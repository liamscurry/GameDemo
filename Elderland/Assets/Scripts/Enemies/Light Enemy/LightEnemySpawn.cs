using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemySpawn : StateMachineBehaviour
{
    private bool exiting;
    private LightEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponent<LightEnemyManager>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            if (manager.PhysicsSystem.TouchingFloor)
            {
                DelegateToGroup();
                exiting = true;
            }
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
