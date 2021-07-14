using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftLandingBehaviour : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.MovementManager.UpdateWalkMovement();
		PlayerInfo.AnimationManager.UpdateWalkProperties();
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.MovementManager, GameInput.Full);
		PlayerInfo.CharMoveSystem.HorizontalOnExit.TryReleaseLock(PlayerInfo.MovementManager, false);
	}
}