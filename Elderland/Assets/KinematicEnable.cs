using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicEnable : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.KinematicBehaviour = this;

		PlayerInfo.Body.isKinematic = true;
		animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		PlayerInfo.Animator.applyRootMotion = true;
	}
}
