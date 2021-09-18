using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// No immediate exit code needed, as it is handled in all cases through exit state method.
public class SlowdownBehaviour : PlayerStateMachineBehaviour 
{
	public void Awake()
	{
		transitionless = true;
	}

	// Same code as in movement behaviour except no transitions. Needed as need one version of walking
	// code (that is, the slowdown behaviour) to be transitionless and the other will have transitions.
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{   
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (!Exiting && GameInfo.Manager.ReceivingInput.Value != GameInput.None)
        {
            PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);
        }
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		PlayerInfo.AnimationManager.EndSlowdown();
	}
}