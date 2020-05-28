using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHoldEventBehaviour : StateMachineBehaviour
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
            float percentage = (stateInfo.normalizedTime < 1) ? stateInfo.normalizedTime : 1;
            PlayerInfo.Manager.Interaction.HoldNormalizedTime = percentage;
            PlayerInfo.Manager.Interaction.HoldEvent();

            if (!Input.GetKey(GameInfo.Settings.UseKey) ||
                (PlayerInfo.Manager.Interaction.InteractionType == StandardInteraction.Type.holdUntilReleaseOrComplete &&
                stateInfo.normalizedTime >= PlayerInfo.Manager.Interaction.HoldDuration))
            {
                animator.SetTrigger("exitInteraction");
                GameInfo.CameraController.AllowZoom = true;
                PlayerInfo.Manager.Interaction.EndEvent();
                exiting = true;
                if (PlayerInfo.Manager.Interaction.Reusable)
                {
                    PlayerInfo.Manager.Interaction.Reset();
                }
            }
        }
    }
}
