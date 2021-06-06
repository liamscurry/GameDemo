using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemyDeath : StateMachineBehaviour
{
    private GruntEnemyManager manager;

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
