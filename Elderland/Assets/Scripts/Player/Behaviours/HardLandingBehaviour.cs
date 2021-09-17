using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateMachineBehaviour incorporated.
public class HardLandingBehaviour : PlayerStateMachineBehaviour
{
	public void Awake()
	{
		transitionless = true;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
		PlayerInfo.MovementManager.SnapSpeed();
		PlayerInfo.MovementManager.ResetSprint();
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		GameInfo.Manager.ReceivingInput.TryReleaseLock(PlayerInfo.MovementManager, GameInput.Full);
		PlayerInfo.CharMoveSystem.HorizontalOnExit.TryReleaseLock(PlayerInfo.MovementManager, false);
	}
}
