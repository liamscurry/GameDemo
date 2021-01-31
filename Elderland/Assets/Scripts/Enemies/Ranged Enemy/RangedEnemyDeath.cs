using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyDeath : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<RangedEnemyManager>();

        foreach (var deathParticle in manager.DeathParticles)
        {
            deathParticle.Play();
        }
        
        //manager.BehaviourLock = this;
    }
}
