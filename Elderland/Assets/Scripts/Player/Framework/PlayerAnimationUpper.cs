using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A helper structure on the animation manager that handles upper body animation actions.
// Tested initial implementation, passed 5/15/21.
public class PlayerAnimationUpper 
{
    // Fields
    private int layerIndex;

    public PlayerAnimationUpper()
    {
        layerIndex = PlayerInfo.Animator.GetLayerIndex("UpperAction");
    }

    /*
    * Handles an action request.
    * If the request is possible, plays the animation clip, fading the weight of the layer appropriately.
    * Returns true.
    * If not, returns false.
    */
    public bool RequestAction(AnimationClip actionClip)
    {
        if (GameInfo.CameraController.CameraState == CameraController.State.Gameplay &&
            PlayerInfo.AbilityManager.CurrentAbility == null && 
            PlayerInfo.AnimationManager.Interuptable)
        {
            PlayerInfo.AnimationManager.SetAnim(actionClip, AnimationConstants.Player.GenericUpperAction);
            PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.UpperAction);
            return true;
        }
        else
        {
            return false;
        }
    }
}