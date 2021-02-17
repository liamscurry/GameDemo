using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayCutsceneBehaviour : StateMachineBehaviour
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
                GameInfo.CameraController.GameplayCutscene.Update();
            
            if (exiting)
            {
                animator.SetTrigger(AnimationConstants.Player.Proceed);
            }
        }
	}
}
