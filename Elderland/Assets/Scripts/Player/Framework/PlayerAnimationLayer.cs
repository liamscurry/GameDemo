using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// A helper structure on the animation manager that handles masked body animation actions.
// Tested initial implementation, passed 5/15/21.
// Only supports one animation at a time, cannot chain animations unless there is a gap of time
// in between them.
public class PlayerAnimationLayer 
{
    // Fields
    private AnimationLoop animLoop;
    private int layerIndex;
    private string layerName;

    public StateMachineBehaviour CurrentBehaviour { get; set; }
    public Action OnEnd { get; private set; }
    public Action OnShortCircuit { get; private set; }

    public float GetLayerWeight { get { return PlayerInfo.Animator.GetLayerWeight(layerIndex); } }

    public PlayerAnimationLayer(string layerName)
    {
        this.layerName = layerName;
        layerIndex = PlayerInfo.Animator.GetLayerIndex(layerName);
        animLoop = 
            new AnimationLoop(
                PlayerInfo.Controller,
                PlayerInfo.Animator,
                AnimationConstants.Player.GenericAction);
    }

    /*
    * Handles an action request for animations.
    */
    public bool RequestAction(AnimationClip actionClip, Action onEnd, Action onShortCircuit)
    {
        animLoop.SetNextSegmentClip(actionClip);
        PlayerInfo.Animator.SetInteger(layerName + "ChoiceSeparator", animLoop.CurrentSegmentIndex + 1);
        PlayerInfo.Animator.SetTrigger(layerName + "Proceed");
        PlayerInfo.Animator.SetBool(layerName + "Exit", false);
        OnEnd = onEnd;
        OnEnd += OnInteractionFinish;
        this.OnShortCircuit = onShortCircuit;
        return true;
    }

    /*
    Short circuits the currently requested action and calls short circuit logic as appropriate (if
    an action is taking place)

    Inputs:
    None

    Outputs:
    None
    */
    public void TryShortCircuit()
    {
        if (CurrentBehaviour != null)
        {
            if (OnShortCircuit != null)
                OnShortCircuit();
            
            PlayerInfo.Animator.SetLayerWeight(layerIndex, 0);
            OnInteractionFinish();
        }
    }

    private void OnInteractionFinish()
    {
        PlayerInfo.Animator.SetBool(layerName + "Exit", true);
        PlayerInfo.Animator.SetTrigger(layerName + "Proceed");
        CurrentBehaviour = null;
    }
}