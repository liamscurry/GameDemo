using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockInputBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.ReceivingInput.TryReleaseLock(this, GameInput.Full);
    }
}
