using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

// Needed to drive gameplay cutscene's wait timer and play wait animation.
public class GameplayCutsceneWaitBehaviour : StateMachineBehaviour
{
    private bool exiting;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        exiting = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
		    exiting =
                GameInfo.CameraController.GameplayCutscene.UpdateWait();
            
            if (exiting)
            {
                animator.SetTrigger(AnimationConstants.Player.Proceed);
            }
        }
	}
}
