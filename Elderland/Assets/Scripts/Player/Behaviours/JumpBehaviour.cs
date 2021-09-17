using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateMachineBehaviour incorporated.
public class JumpBehaviour : PlayerStateMachineBehaviour 
{
	public void Awake()
	{
		transitionless = true;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!Exiting)
		{
			PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
		}
	}
}
