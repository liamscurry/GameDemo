using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : StateMachineBehaviour 
{
	private bool exiting;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		exiting = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{ 
		if (!exiting)
		{
			PlayerInfo.MovementSystem.Move(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);
			//PlayerInfo.MovementSystem.Move(new Vector2(1,0), PlayerInfo.StatsManager.Movespeed);

			if (PlayerInfo.PhysicsSystem.ExitedFloor)
			{
				animator.SetBool("falling", true);
				exiting = true;
			}

			if (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.5f && GameInfo.Manager.ReceivingInput && !animator.IsInTransition(0))
			{
				animator.SetFloat("speed", 1);
				exiting = true;
			}
		}
	}
}