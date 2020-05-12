using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyAttackPivot : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<RangedEnemyManager>();
        }

        manager.ChooseNextAbility();    
    
        if (manager.IsInNextAttackMax() && manager.HasClearPlacement())
        {
            StationaryExit();
        }
        else
        {
            FollowExit();
        }
    }

    private void StationaryExit()
    {
        manager.Animator.SetTrigger("toStationary");  
    }

    private void FollowExit()
    {
        manager.Animator.SetTrigger("toFollow");
    }
}
