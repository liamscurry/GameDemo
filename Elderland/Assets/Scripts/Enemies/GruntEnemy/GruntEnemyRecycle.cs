using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Generic recycle behaviour for enemies. Can be overriden.
*/
public class GruntEnemyRecycle : StateMachineBehaviour
{
    protected EnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<EnemyManager>();

        foreach (var recycleParticle in manager.RecycleParticles)
        {
            recycleParticle.Play();
        }

        manager.BehaviourLock = this;
    }
}
