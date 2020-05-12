using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnZoomOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.CameraController.AllowZoom = true;
    }
}
