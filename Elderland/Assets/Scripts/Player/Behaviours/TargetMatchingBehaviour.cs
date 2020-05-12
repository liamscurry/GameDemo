using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMatchingBehaviour : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.StartTarget();
	}
}