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
        PlayerInfo.MovementManager.AnimationPercentileSpeed = 0;
        PlayerInfo.AnimationManager.UpdateWalkProperties();
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
    }

    /*
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            timer += Time.deltaTime;
            if (timer > duration)
            {
                exiting = true;
                animator.SetTrigger("exitInteraction");
            }
        }
    }*/
}
