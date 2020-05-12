using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBehaviour : StateMachineBehaviour 
{
	private bool exiting;
	private float fastestFallingSpeed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		PlayerInfo.MovementManager.LockDirection();
		PlayerInfo.MovementManager.LockSpeed();
		exiting = false;
		fastestFallingSpeed = PlayerInfo.PhysicsSystem.LastCalculatedVelocity.y;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		// && !animator.IsInTransition(0)
		if (!exiting)
		{
			if (PlayerInfo.PhysicsSystem.LastCalculatedVelocity.y < fastestFallingSpeed)
				fastestFallingSpeed = PlayerInfo.PhysicsSystem.LastCalculatedVelocity.y;

			if ((PlayerInfo.PhysicsSystem.EnteredFloor || PlayerInfo.PhysicsSystem.TouchingFloor))
			{
				if (fastestFallingSpeed < -15)
				{
					//Going fast, transition to landing animation
					animator.SetFloat("speed", 0);
					PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
					PlayerInfo.MovementManager.SnapSpeed();
				}
				else
				{
					animator.SetFloat("speed", 1);
				}

				animator.SetBool("falling", false);
				exiting = true;
			}	
		}
	}
}
