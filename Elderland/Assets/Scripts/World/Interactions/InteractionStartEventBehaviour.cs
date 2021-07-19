using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Behaviour needed to start match target on interactions.
*/
public class InteractionStartEventBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerInfo.AnimationManager.CurrentInteraction.StartDirectTarget();
    }
}
