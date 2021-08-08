﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBehaviour : StateMachineBehaviour 
{
	private bool exiting;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		exiting = false;
		PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (!exiting)
		{
			PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
		}
	}
}
