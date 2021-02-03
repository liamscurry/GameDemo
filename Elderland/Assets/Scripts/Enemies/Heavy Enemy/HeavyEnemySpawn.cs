using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemySpawn : StateMachineBehaviour
{
    private bool exiting;
    private HeavyEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<HeavyEnemyManager>();

        foreach (var spawnParticle in manager.SpawnParticles)
        {
            spawnParticle.Play();
        }
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
