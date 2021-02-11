using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemyFlinch : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        manager.BehaviourLock = this;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
