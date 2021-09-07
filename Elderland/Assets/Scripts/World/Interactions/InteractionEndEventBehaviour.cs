﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionEndEventBehaviour : StateMachineBehaviour
{
    /*
    private float timer;
    private const float duration = 0.4f;
    
    private bool exiting;*/

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger("exitInteraction");
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        PlayerInfo.MovementManager.SnapSpeed();
        PlayerInfo.MovementManager.TargetDirection =
            Matho.StdProj2D(PlayerInfo.Player.transform.forward);
        PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.AnimationManager.UpdateFreeWalkProperties();
        PlayerInfo.AnimationManager.UpdateFreeRotationProperties();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerInfo.AnimationManager.UpdateFreeWalkProperties();
        PlayerInfo.AnimationManager.UpdateFreeRotationProperties();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameInfo.CameraController.AllowZoom = true;
        PlayerInfo.Manager.Interaction.ReleaseInteraction();
        PlayerInfo.Manager.Interaction.EndEvent();
        if (PlayerInfo.Manager.Interaction.Reusable)
        {
            PlayerInfo.Manager.Interaction.Reset();
        }
        GameInfo.CameraController.TargetDirection = Vector3.zero;
        PlayerInfo.AnimationManager.CurrentInteraction = null;
    }
}
