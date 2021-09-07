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
        PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        PlayerInfo.MovementManager.SnapSpeed();
        PlayerInfo.AnimationManager.UpdateFreeWalkProperties();
        PlayerInfo.AnimationManager.UpdateFreeRotationProperties();
        PlayerInfo.MovementManager.TargetDirection =
            Matho.StdProj2D(PlayerInfo.Player.transform.forward);
        PlayerInfo.MovementManager.SnapDirection();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            PlayerInfo.AnimationManager.UpdateFreeWalkProperties();
            PlayerInfo.AnimationManager.UpdateFreeRotationProperties();

            float percentage =
                Mathf.Clamp01(stateInfo.normalizedTime * stateInfo.length / PlayerInfo.Manager.Interaction.HoldDuration);
            PlayerInfo.Manager.Interaction.HoldNormalizedTime = percentage;
            PlayerInfo.Manager.Interaction.HoldEvent();

            if ((!GameInfo.Settings.CurrentGamepad[GameInfo.Settings.UseKey].isPressed &&
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
                    PlayerInfo.AnimationManager.CurrentInteraction = null;
                }
            }
        }
    }
}
