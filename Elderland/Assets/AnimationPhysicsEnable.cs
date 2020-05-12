using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPhysicsEnable : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.AnimationPhysicsBehaviour = this;

		PlayerInfo.PhysicsSystem.Animating = true;
	}
}
