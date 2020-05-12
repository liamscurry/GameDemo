using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionEndEventBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.CameraController.AllowZoom = true;
        PlayerInfo.Manager.Interaction.EndEvent();
    }
}
