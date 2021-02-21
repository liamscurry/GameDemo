﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

// Needed to drive gameplay cutscene and sync match targets with current
// cutscene waypoint.
public class GameplayCutsceneBehaviour : StateMachineBehaviour
{
    private bool exiting;
    private bool hasStartedMatching;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        var matchTarget =
            new PlayerAnimationManager.MatchTarget(
                GameInfo.CameraController.GameplayCutscene.TargetPosition,
                GameInfo.CameraController.GameplayCutscene.TargetRotation,
                AvatarTarget.Root,
                Vector3.one,
                GameInfo.CameraController.GameplayCutscene.CurrentWaypointNode.Value.RotationWeight,
                0,
                GameInfo.CameraController.GameplayCutscene.CurrentStateNormDuration
            );

        PlayerInfo.AnimationManager.StartTarget(matchTarget);
        exiting = false;
        hasStartedMatching = false;
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        PlayerInfo.Manager.StartCoroutine(OnStateExitCoroutine());
    }

    private IEnumerator OnStateExitCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (GameInfo.CameraController.GameplayCutscene.OnStateExit != null)
			GameInfo.CameraController.GameplayCutscene.OnStateExit.Invoke();
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

            /*
            var currentClips = 
                animator.GetCurrentAnimatorClipInfo(0);
            Debug.Log("start");
            foreach (var clipInfo in currentClips)
            {
                Debug.Log(clipInfo.clip);
            }
            Debug.Log("end");
            Debug.Log("");*/

		    exiting =
                GameInfo.CameraController.GameplayCutscene.UpdateTravel(
                    hasStartedMatching && !inMatchTarget);
            
            if (exiting)
            {
                animator.SetTrigger(AnimationConstants.Player.Proceed);
            }
        }
	}
}
