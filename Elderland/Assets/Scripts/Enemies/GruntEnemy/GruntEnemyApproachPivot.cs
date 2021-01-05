using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemyApproachPivot : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        //manager.ChooseNextAbility();
        DecideTransition();
    }

    // TEMP
    private void DecideTransition()
    {
        FarFollowExit();

        /*
        if (!manager.IsInNextAttackMax() || manager.IsInNextAttackMin())
        {
            FollowExit();
        }
        else
        {
            StationaryExit();
        }
        */
    }

    private void FarFollowExit()
    {
        manager.Animator.SetTrigger("toFarFollow");
    }

    private void GroupFollowExit()
    {
        manager.Animator.SetTrigger("toGroupFollow");
    }
}
