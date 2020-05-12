using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbIdleBehaviour : StateMachineBehaviour 
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		Vector2 input = GameInfo.Settings.LeftDirectionalInput;

        float verticalAngle = Matho.AngleBetween(Vector2.up, input);

		if (!animator.IsInTransition(0))
		{
			if (verticalAngle < 45)
			{
				animator.SetFloat("climbSpeedVertical", 1);
			}
			else if (verticalAngle > 135)
			{
				animator.SetFloat("climbSpeedVertical", -1);
			}
			else
			{
				float horizontalAngle = Matho.AngleBetween(Vector2.right, input);
				if (horizontalAngle < 45)
				{
					animator.SetFloat("climbSpeedHorizontal", 1);
				}
				else if (horizontalAngle > 135)
				{
					animator.SetFloat("climbSpeedHorizontal", -1);
				}
			}
		}
	}
}
