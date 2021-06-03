using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbHorizontalBehaviour : StateMachineBehaviour 
{
	private Ladder Ladder { get{ return PlayerInfo.InteractionManager.Ladder; } }
	private bool inTransition;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		inTransition = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (!inTransition)
		{
			Vector2 input = GameInfo.Settings.LeftDirectionalInput;
			float angle = Matho.AngleBetween(Vector2.up, input);
			if (input.magnitude < 0.75f)
			{
				input = Vector2.zero;
				angle = 0;
			}
			float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - Ladder.transform.position, Ladder.RightDirection);

			//Left clamp
			if (horizontalProjectionScalar < -Ladder.Width / 2)
			{
				float verticalPosition = PlayerInfo.Player.transform.position.y;
				Vector3 horizontalPosition = Matho.StandardProjection3D(Ladder.transform.position);
				horizontalPosition += Ladder.RightDirection * (-Ladder.Width / 2);
				horizontalPosition += Ladder.Normal * (PlayerInfo.Capsule.radius + Ladder.Depth / 2);
				PlayerInfo.Player.transform.position = new Vector3(horizontalPosition.x, verticalPosition, horizontalPosition.z);
			}

			//Right Clamp
			if (horizontalProjectionScalar > Ladder.Width / 2)
			{
				float verticalPosition = PlayerInfo.Player.transform.position.y;
				Vector3 horizontalPosition = Matho.StandardProjection3D(Ladder.transform.position);
				horizontalPosition += Ladder.RightDirection * (Ladder.Width / 2);
				horizontalPosition += Ladder.Normal * (PlayerInfo.Capsule.radius + Ladder.Depth / 2);
				PlayerInfo.Player.transform.position = new Vector3(horizontalPosition.x, verticalPosition, horizontalPosition.z);
			}

			if (angle > 45 && angle < 135)
			{
				// - PlayerInfo.Capsule.radius
				if (Mathf.Abs(horizontalProjectionScalar) < Ladder.Width / 2 ||
					horizontalProjectionScalar > 0 && input.x < 0 ||
					horizontalProjectionScalar < 0 && input.x > 0)
				{
					Vector2 invertedNormalDirection = Matho.Rotate(Matho.StdProj2D(Ladder.Normal), 180);

					Vector3 direction = Mathf.Sin(angle * Mathf.Deg2Rad) * Ladder.RightDirection * Matho.Sign(input.x);
					PlayerInfo.PhysicsSystem.AnimationVelocity += 3 * direction.normalized;
				}
			}
			else if (!animator.IsInTransition(0))
			{
				animator.SetFloat("climbSpeedHorizontal", 0);
				inTransition = true;
			}
		}
	}
}
