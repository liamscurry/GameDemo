using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Behaviour that runs during the player's respawn animation.
// PlayerStateMachineBehaviour incorporated.
public class RespawnBehaviour : PlayerStateMachineBehaviour 
{
	public void Awake()
	{
		transitionless = true;
		unoverrideable = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        GameInfo.Manager.ReceivingInput.TryReleaseLock(GameInfo.Manager, GameInput.Full);
	}
}
