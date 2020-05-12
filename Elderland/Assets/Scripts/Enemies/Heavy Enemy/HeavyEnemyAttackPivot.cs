using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemyAttackPivot : StateMachineBehaviour
{
    private HeavyEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<HeavyEnemyManager>();
        }

        manager.ChooseNextAbility();
        DecideTransition();
    }

    private void DecideTransition()
    {
        if (!manager.IsInNextAttackMax() || manager.IsInNextAttackMin())
        {
            FollowExit();
        }
        else
        {
            StationaryExit();
        }
    }

    private void FollowExit()
    {
        manager.Animator.SetTrigger("toFollow");
    }

    private void StationaryExit()
    {
        manager.Animator.SetTrigger("toStationary");
    }
}
