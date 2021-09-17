using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateMachineBehaviour incorporated.
public class SoftLandingBehaviour : PlayerStateMachineBehaviour
{
	public void Awake()
	{
		transitionless = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!Exiting)
		{
			PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
		}
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.MovementManager, GameInput.Full);
		PlayerInfo.CharMoveSystem.HorizontalOnExit.TryReleaseLock(PlayerInfo.MovementManager, false);
	}
}
