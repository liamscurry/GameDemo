using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete("use Kinematic.cs for target matching.")]
public class RootMotionStateBehaviour : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.PhysicsSystem.Animating = true;
		PlayerInfo.Body.isKinematic = true;
		PlayerInfo.Animator.applyRootMotion = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.PhysicsSystem.Animating = false;
		PlayerInfo.Body.isKinematic = false;
		PlayerInfo.Animator.applyRootMotion = false;
	}
}