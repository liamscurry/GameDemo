using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLoopBehaviour : StateMachineBehaviour 
{
	private float fastestFallingSpeed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		fastestFallingSpeed = PlayerInfo.CharMoveSystem.GravityVelocity.y;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (animator.GetBool(AnimationConstants.Player.Falling))
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
			}	
		}
	}
}
