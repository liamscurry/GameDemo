using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemyRecycle : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<GruntEnemyManager>();

        foreach (var recycleParticle in manager.RecycleParticles)
        {
            recycleParticle.Play();
        }

        manager.BehaviourLock = this;
    }
}
