using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbDownBehaviour : StateMachineBehaviour 
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

		//Clamp
		if (PlayerInfo.Player.transform.position.y < (Ladder.transform.position.y - Ladder.Height / 2) + (PlayerInfo.Capsule.height / 2))
		{
			float verticalPosition = (Ladder.transform.position.y - Ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);
			PlayerInfo.Player.transform.position = new Vector3(PlayerInfo.Player.transform.position.x, verticalPosition, PlayerInfo.Player.transform.position.z);
		}

		if (angle > 135)
		{
			Vector3 direction = Mathf.Cos(angle * Mathf.Deg2Rad) * Ladder.UpDirection; 
			PlayerInfo.PhysicsSystem.AnimationVelocity += 5 * direction.normalized;
		}
		else if (!animator.IsInTransition(0))
		{	
			animator.SetFloat("climbSpeedVertical", 0);
			exiting = true;
		}

		//Exit transition
		if (PlayerInfo.Sensor.LadderBottom != null && !exiting)
		{
			float playerBottom = PlayerInfo.Player.transform.position.y - (PlayerInfo.Capsule.height / 2);
			float ladderBottom = Ladder.transform.position.y - (Ladder.Height / 2);

			Vector2 normal = Matho.StdProj2D(Ladder.Normal);

			if (Matho.IsInRange(playerBottom, ladderBottom, 0.1f) && angle > 135)
			{
				//Horizontal positioning
            	float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - Ladder.transform.position, Ladder.RightDirection);

				//Generate target positions
				Vector3 climbTargetPosition = Ladder.transform.position;
                climbTargetPosition += Ladder.Normal * (PlayerInfo.Capsule.radius + Ladder.Depth / 2);
                climbTargetPosition += Ladder.RightDirection * horizontalProjectionScalar;
                climbTargetPosition.y = Ladder.transform.position.y - (Ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Vector3 walkTargetPosition = climbTargetPosition + Ladder.Normal * 1f;

				Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, Ladder.Normal);

				var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1); 
                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1);
				PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);

				PlayerInfo.MovementManager.TargetDirection = Ladder.Normal;
				PlayerInfo.MovementManager.SnapDirection();

				animator.SetTrigger("climbExitBottom");
				exiting = true;
			}
		}
	}
}
