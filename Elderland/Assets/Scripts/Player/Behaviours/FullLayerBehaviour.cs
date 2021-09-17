using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
* Behaviour that fades the layer's weights in and out.
*/
// Tested after initial implementation, passed
public class FullLayerBehaviour : StateMachineBehaviour 
{
    // Fields
    private Action onShortCircuit;
    private readonly float duration = 0.20f;

    private bool Exiting { get; set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        Exiting = false;
        PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
        PlayerInfo.AnimationManager.FullLayer.CurrentBehaviour = this;
        onShortCircuit = PlayerInfo.AnimationManager.FullLayer.OnShortCircuit;
	}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!Exiting)
        {
            if (PlayerInfo.AnimationManager.UpperLayer.CurrentBehaviour == null)
            {
                Exiting = true;
                return;
            }
            else if (PlayerInfo.AnimationManager.FullLayer.CurrentBehaviour != this)
            {
                Exiting = true;
                if (onShortCircuit != null)
                    onShortCircuit();

                return;
            }
            else if (stateInfo.normalizedTime >= 1)
            {
                Exiting = true;
                PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
                if (PlayerInfo.AnimationManager.FullLayer.OnEnd != null)
                    PlayerInfo.AnimationManager.FullLayer.OnEnd();
                
                return;
            }

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
        }
	}
}