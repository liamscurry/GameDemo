using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionEndEventBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.CameraController.AllowZoom = true;
        PlayerInfo.Manager.Interaction.EndEvent();
        if (PlayerInfo.Manager.Interaction.Reusable)
        {
            PlayerInfo.Manager.Interaction.Reset();
        }
        animator.SetTrigger("exitInteraction");
    }
}
