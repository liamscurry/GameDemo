using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffInputBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.Manager.ReceivingInput.ClaimLock(animator, GameInput.None);
    }
}
