using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbUpBehaviour : StateMachineBehaviour 
{
	private bool exiting;
	private Ladder Ladder { get { return PlayerInfo.InteractionManager.Ladder; } }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		exiting = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		Vector2 input = GameInfo.Settings.LeftDirectionalInput;

		//Idle transition
		float angle = Matho.AngleBetween(Vector2.up, input);

		if (PlayerInfo.Player.transform.position.y > (Ladder.transform.position.y + Ladder.Height / 2))
		{
			float verticalPosition = (Ladder.transform.position.y + Ladder.Height / 2);
			PlayerInfo.Player.transform.position = new Vector3(PlayerInfo.Player.transform.position.x, verticalPosition, PlayerInfo.Player.transform.position.z);
		}

		if (angle < 45)
		{
			Vector3 direction = Mathf.Cos(angle * Mathf.Deg2Rad) * Ladder.UpDirection; 
			PlayerInfo.PhysicsSystem.AnimationVelocity += 5 * direction.normalized;
		}
		else if (!animator.IsInTransition(0))
		{
			animator.SetFloat("climbSpeedVertical", 0);
			exiting = true;
		}

		//Transition
		if (PlayerInfo.Sensor.LadderTop != null && !exiting)
		{
			float playerMiddle = PlayerInfo.Player.transform.position.y;
			float ladderTop = Ladder.transform.position.y + (Ladder.Height / 2);

			Vector2 invertedNormal = Matho.Rotate(Matho.StandardProjection2D(Ladder.Normal), 180);

			if (Matho.IsInRange(playerMiddle, ladderTop, 0.1f) && angle < 45)
			{
				//Horizontal positioning
            	float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - Ladder.transform.position, Ladder.RightDirection);

				//Generate target positions
				Vector3 climbTargetPosition = Ladder.transform.position;
                climbTargetPosition += Ladder.Normal * (PlayerInfo.Capsule.radius + Ladder.Depth / 2);
                climbTargetPosition += Ladder.RightDirection * horizontalProjectionScalar;
                climbTargetPosition.y = Ladder.transform.position.y + (Ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Vector3 walkTargetPosition = Ladder.transform.position;
                walkTargetPosition -= Ladder.Normal * 1f;
                walkTargetPosition += Ladder.RightDirection * horizontalProjectionScalar;
                walkTargetPosition.y = Ladder.transform.position.y + (Ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

				Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, -Ladder.Normal);

				var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1); 
                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1);
                PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);

				PlayerInfo.MovementManager.TargetDirection = -Ladder.Normal;
				PlayerInfo.MovementManager.SnapDirection();

				animator.SetTrigger("climbExitTop");
				exiting = true;
			}
		}
	}
}
