using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicDisable : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.KinematicBehaviour = this;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (PlayerInfo.AnimationManager.KinematicBehaviour == this)
		{
			PlayerInfo.Body.isKinematic = false;
			animator.updateMode = AnimatorUpdateMode.Normal;
			PlayerInfo.Animator.applyRootMotion = false;

			Vector2 forwardProjection = Matho.StdProj2D(PlayerInfo.Player.transform.forward).normalized;
			PlayerInfo.MovementManager.TargetDirection = forwardProjection;
			PlayerInfo.MovementManager.SnapDirection();
		}
	}
}
