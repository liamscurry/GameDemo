using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBehaviour : StateMachineBehaviour 
{
	private bool exiting;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		exiting = false;
        PlayerInfo.MovementSystem.Move(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);
		PlayerInfo.MovementSystem.Jump(PlayerInfo.StatsManager.Jumpspeed);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (!exiting)
		{
			PlayerInfo.MovementSystem.Move(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);
			
			if ((PlayerInfo.PhysicsSystem.EnteredFloor || PlayerInfo.PhysicsSystem.TouchingFloor) && !animator.IsInTransition(0))
			{
				if (PlayerInfo.PhysicsSystem.LastCalculatedVelocity.y < -15)
				{
					//Going fast, transition to landing animation
					animator.SetFloat("speed", 0);
					PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
					PlayerInfo.MovementManager.SnapSpeed();
				}
				else
				{
					//Going slow enough, transition to walk animation
					animator.SetFloat("speed", 1);

					//Set walk speed based on landing angle. Horizontal starts at 0, straight down is 1.
					//This reduces "speed boost" effect when landing due to dynamic velocity cancelation.
					Vector3 landTrajectory = PlayerInfo.PhysicsSystem.LastDynamicVelocity;
					float landAngle = Matho.AngleBetween(landTrajectory, -PlayerInfo.PhysicsSystem.Normal);
					PlayerInfo.MovementManager.TargetPercentileSpeed = Mathf.Clamp01(-(1/67.5f) * landAngle + 1) * PlayerInfo.MovementManager.TargetPercentileSpeed;
					PlayerInfo.MovementManager.SnapSpeed();
				}

				animator.SetBool("jump", false);
				exiting = true;
			}
		}
	}
}
