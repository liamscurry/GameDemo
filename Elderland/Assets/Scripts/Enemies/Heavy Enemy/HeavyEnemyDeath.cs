using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemyDeath : StateMachineBehaviour
{
    private HeavyEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<HeavyEnemyManager>();
        
        foreach (var deathParticle in manager.DeathParticles)
        {
            deathParticle.Play();
        }

        animator.speed = 0;

        //manager.BehaviourLock = this;
    }
}
