using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnCameraDirectionOnExit : StateMachineBehaviour 
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        GameInfo.CameraController.TargetDirection = Vector3.zero;
	}
}