using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowdownBehaviour : StateMachineBehaviour 
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.EndSlowdown();
	}
}