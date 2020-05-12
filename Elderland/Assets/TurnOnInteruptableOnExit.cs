using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnInteruptableOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerInfo.AnimationManager.Interuptable = true;
    }
}
