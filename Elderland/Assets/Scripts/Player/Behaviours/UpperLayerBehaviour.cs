using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
* Behaviour that fades the layer's weights in and out.
*/
// Tested after initial implementation, passed
public class UpperLayerBehaviour : StateMachineBehaviour 
{
    // Fields
    private Action onShortCircuit;
    private bool exiting;
    private readonly float duration = 0.20f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
        exiting = false;
        PlayerInfo.AnimationManager.UpperLayer.CurrentBehaviour = this;
        onShortCircuit = PlayerInfo.AnimationManager.UpperLayer.OnShortCircuit;
	}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            float currentWeight = 
                PlayerInfo.Animator.GetLayerWeight(layerIndex);
            if (stateInfo.normalizedTime < duration)
            {
                float newWeight = Mathf.Clamp01(stateInfo.normalizedTime / duration);

                if (newWeight > currentWeight)
                    PlayerInfo.Animator.SetLayerWeight(layerIndex, newWeight);
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

            if (PlayerInfo.AnimationManager.UpperLayer.CurrentBehaviour != this)
            {
                exiting = true;
                if (onShortCircuit != null)
                    onShortCircuit();
            }
            else if (stateInfo.normalizedTime >= 1)
            {
                exiting = true;
                PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
                if (PlayerInfo.AnimationManager.UpperLayer.OnEnd != null)
                    PlayerInfo.AnimationManager.UpperLayer.OnEnd();
            }
        }
	}
}