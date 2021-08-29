using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Generic recycle behaviour for enemies. Can be overriden.
*/
public class GruntEnemyDeath : StateMachineBehaviour
{
    protected GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<GruntEnemyManager>();
        
        foreach (var deathParticle in manager.DeathParticles)
        {
            deathParticle.Play();
        }

        //animator.speed = 0;

        manager.BehaviourLock = this;
    }
}
