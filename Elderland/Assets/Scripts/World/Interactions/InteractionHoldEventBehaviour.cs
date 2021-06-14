using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHoldEventBehaviour : StateMachineBehaviour
{
    private bool exiting;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        exiting = false;
        PlayerInfo.Manager.Interaction.StartEvent();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            float percentage =
                Mathf.Clamp01(stateInfo.normalizedTime * stateInfo.length / PlayerInfo.Manager.Interaction.HoldDuration);
            PlayerInfo.Manager.Interaction.HoldNormalizedTime = percentage;
            PlayerInfo.Manager.Interaction.HoldEvent();

            if ((!Input.GetKey(GameInfo.Settings.UseKey) &&
                stateInfo.normalizedTime * stateInfo.length >= PlayerInfo.Manager.Interaction.MinimumHoldDuration) ||
                stateInfo.normalizedTime * stateInfo.length >= PlayerInfo.Manager.Interaction.HoldDuration)
            {
                //if (stateInfo.normalizedTime * stateInfo.length > 1.25f)
                {
                    animator.SetTrigger("exitInteraction");
                    GameInfo.CameraController.AllowZoom = true;
                    PlayerInfo.Manager.Interaction.ReleaseInteraction();
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
}
