using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockInputBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.FreezeInput(this);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.UnfreezeInput(this);
    }
}
