using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// A helper structure on the animation manager that handles upper body animation actions.
// Tested initial implementation, passed 5/15/21.
// Only supports one animation at a time, cannot chain animations unless there is a gap of time
// in between them.
public class PlayerAnimationUpper 
{
    // Fields
    private AnimationLoop animLoop;
    private int layerIndex;

    public StateMachineBehaviour CurrentBehaviour { get; set; }
    public Action OnEnd { get; private set; }
    public Action OnShortCircuit { get; private set; }

    public PlayerAnimationUpper()
    {
        layerIndex = PlayerInfo.Animator.GetLayerIndex("UpperAction");
        animLoop = 
            new AnimationLoop(
                PlayerInfo.Controller,
                PlayerInfo.Animator,
                AnimationConstants.Player.GenericUpperAction);
    }

    /*
    * Handles an action request for animations.
    */
    public bool RequestAction(AnimationClip actionClip, Action onEnd, Action onShortCircuit)
    {
        //PlayerInfo.AnimationManager.SetAnim(actionClip, AnimationConstants.Player.GenericUpperAction);
        animLoop.SetNextSegmentClip(actionClip);
        PlayerInfo.Animator.SetInteger("upperActionChoiceSeparator", animLoop.CurrentSegmentIndex + 1);
        PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.ProceedUpperAction);
        PlayerInfo.Animator.SetBool(AnimationConstants.Player.ExitUpperAction, false);
        OnEnd = onEnd;
        OnEnd += OnInteractionFinish;
        this.OnShortCircuit = onShortCircuit;
        return true;
    }

    private void OnInteractionFinish()
    {
        PlayerInfo.Animator.SetBool(AnimationConstants.Player.ExitUpperAction, true);
        PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.ProceedUpperAction);
        CurrentBehaviour = null;
    }
}