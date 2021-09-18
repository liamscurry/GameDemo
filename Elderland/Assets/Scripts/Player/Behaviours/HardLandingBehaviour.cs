using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardLandingBehaviour : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
		PlayerInfo.MovementManager.SnapSpeed();
		PlayerInfo.MovementManager.ResetSprint();
	}

	
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.MovementManager, GameInput.Full);
		PlayerInfo.CharMoveSystem.HorizontalOnExit.TryReleaseLock(PlayerInfo.MovementManager, false);
	}
}
