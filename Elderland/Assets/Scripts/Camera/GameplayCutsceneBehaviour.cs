using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Needed to drive gameplay cutscene and sync match targets with current
// cutscene waypoint.
public class GameplayCutsceneBehaviour : StateMachineBehaviour
{
    private bool exiting;
    private bool hasStartedMatching;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        float distanceToTarget = 
            Vector3.Distance(
                PlayerInfo.Player.transform.position,
                GameInfo.CameraController.GameplayCutscene.TargetPosition);

        var matchTarget =
            new PlayerAnimationManager.MatchTarget(
                GameInfo.CameraController.GameplayCutscene.TargetPosition,
                GameInfo.CameraController.GameplayCutscene.TargetRotation,
                AvatarTarget.Root,
                Vector3.one,
                1,
                0,
                distanceToTarget *
                GameInfo.CameraController.GameplayCutscene.CurrentWaypointNode.Value.clipsPerDistance
            );

        PlayerInfo.AnimationManager.StartTarget(matchTarget);
        exiting = false;
        hasStartedMatching = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            bool inMatchTarget = animator.isMatchingTarget;
            if (inMatchTarget && !hasStartedMatching)
            {
                hasStartedMatching = true;
            }

		    exiting =
                GameInfo.CameraController.GameplayCutscene.Update(
                    hasStartedMatching && !inMatchTarget);
            
            if (exiting)
            {
                animator.SetTrigger(AnimationConstants.Player.Proceed);
            }
        }
	}
}
