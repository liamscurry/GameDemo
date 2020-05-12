using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbExitBehaviour : StateMachineBehaviour 
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		animator.SetFloat("climbSpeedVertical", 0);
		animator.SetFloat("climbSpeedHorizontal", 0);
		PlayerInfo.InteractionManager.Ladder = null;
		PlayerInfo.AnimationManager.IgnoreFallingAnimation = false;
		GameInfo.CameraController.AllowZoom = true;
	}
}