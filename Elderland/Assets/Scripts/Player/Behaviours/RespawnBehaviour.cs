using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Behaviour that runs during the player's respawn animation.
public class RespawnBehaviour : StateMachineBehaviour 
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		GameInfo.Manager.GameplayUI.SetActive(true);
        GameInfo.Manager.ReceivingInput.TryReleaseLock(GameInfo.Manager, GameInput.Full);
	}
}
