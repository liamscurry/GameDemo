using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingStartBehaviour : StateMachineBehaviour 
{
	private bool exiting;
	private float fastestFallingSpeed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		exiting = false;

		animator.SetBool(AnimationConstants.Player.Jump, false);
		animator.SetBool(AnimationConstants.Player.Falling, true);
		PlayerInfo.MovementManager.LockDirection();
		PlayerInfo.MovementManager.LockSpeed();
		fastestFallingSpeed = PlayerInfo.CharMoveSystem.GravityVelocity.y;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (!exiting)
		{
			if (PlayerInfo.CharMoveSystem.GravityVelocity.y < fastestFallingSpeed)
				fastestFallingSpeed = PlayerInfo.CharMoveSystem.GravityVelocity.y;

			if (PlayerInfo.CharMoveSystem.Grounded)
			{
				if (fastestFallingSpeed < -15)
				{
					//Going fast, transition to landing animation
					animator.SetInteger(AnimationConstants.Player.FallSpeed, 1);
				}
				else
				{
					animator.SetInteger(AnimationConstants.Player.FallSpeed, 0);
				}

				animator.SetBool(AnimationConstants.Player.Falling, false);
				exiting = true;
			}	
		}
	}
}
