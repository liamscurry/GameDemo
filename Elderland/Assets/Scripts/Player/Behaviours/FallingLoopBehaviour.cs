using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateMachineBehaviour incorporated.
public class FallingLoopBehaviour : PlayerStateMachineBehaviour 
{
	private float fastestFallingSpeed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		fastestFallingSpeed = PlayerInfo.CharMoveSystem.DynamicAirVelocity.y;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!Exiting)
		{
			if (animator.GetBool(AnimationConstants.Player.Falling))
			{
				if (PlayerInfo.CharMoveSystem.DynamicAirVelocity.y < fastestFallingSpeed)
					fastestFallingSpeed = PlayerInfo.CharMoveSystem.DynamicAirVelocity.y;

				if (PlayerInfo.CharMoveSystem.Grounded)
				{
					if (fastestFallingSpeed < -PlayerMovementManager.FastFallSpeed)
					{
						//Going fast, transition to landing animation
						animator.SetInteger(AnimationConstants.Player.FallSpeed, 1);
					}
					else
					{
						animator.SetInteger(AnimationConstants.Player.FallSpeed, 0);
					}

					animator.SetBool(AnimationConstants.Player.Falling, false);
					Exiting = true;
				}	
			}
		}
	}
}
