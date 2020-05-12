using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPhysicsDisable : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.AnimationPhysicsBehaviour = this;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (PlayerInfo.AnimationManager.AnimationPhysicsBehaviour == this)
		{
			PlayerInfo.PhysicsSystem.Animating = false;
		}
	}
}
