using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnInputBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.ReceivingInput.TryReleaseLock(animator, GameInput.Full);
    }
}
