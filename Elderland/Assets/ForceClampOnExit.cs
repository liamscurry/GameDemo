using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceClampOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerInfo.PhysicsSystem.ForceTouchingFloor();
    }
}
