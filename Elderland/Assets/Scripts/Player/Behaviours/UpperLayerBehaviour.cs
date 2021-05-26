using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Behaviour that fades the layer's weights in and out.
*/
// Tested after initial implementation, passed
public class UpperLayerBehaviour : StateMachineBehaviour 
{
    // Fields
    private readonly float duration = 0.20f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
	}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (stateInfo.normalizedTime < duration)
        {
            PlayerInfo.Animator.SetLayerWeight(
                layerIndex,
                Mathf.Clamp01(stateInfo.normalizedTime / duration));
        }
        else if (stateInfo.normalizedTime > 1 - duration)
        {
            PlayerInfo.Animator.SetLayerWeight(
                layerIndex,
                Mathf.Clamp01((1 - stateInfo.normalizedTime) / duration));
        }
        else
        {
            PlayerInfo.Animator.SetLayerWeight(layerIndex, 1f);
        }
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
        PlayerInfo.AnimationManager.Interuptable = true;
        if (PlayerInfo.AnimationManager.UpperLayer.OnEnd != null)
            PlayerInfo.AnimationManager.UpperLayer.OnEnd();
	}
}