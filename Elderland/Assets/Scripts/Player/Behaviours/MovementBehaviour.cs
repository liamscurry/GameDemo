﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// PlayerStateMachineBehaviour incorporated.
public class MovementBehaviour : PlayerStateMachineBehaviour 
{
    private bool keepSprintOnExit;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        base.OnStateEnter(animator, stateInfo, layerIndex);
        keepSprintOnExit = false;
	}

    protected override void OnStateExitImmediate()
    {
        Debug.Log("called immediate exit");
    }

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{   
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (!Exiting && GameInfo.Manager.ReceivingInput.Value != GameInput.None)
        {
            PlayerInfo.MovementManager.UpdateFreeWalkMovement(true);

            //Transitions//
            //if (!animator.IsInTransition(0))
            {
                if (!Exiting)
                    JumpTransition(animator, stateInfo, layerIndex);
                if (!Exiting)
                    FallingTransition(animator, stateInfo, layerIndex);
                if (!Exiting)
                    SlowdownTransition(animator, stateInfo, layerIndex);
                /*
                if (!exiting)
                    LadderTransition(animator, stateInfo, layerIndex);
                if (!exiting)
                    MantleTransition(animator, stateInfo, layerIndex);
                */
            }
        }
	}

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        if (!keepSprintOnExit)
            PlayerInfo.MovementManager.ResetSprint();
        PlayerInfo.MovementManager.PercSpeedObstructedModifier = 0;
	}

    private void JumpTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (GameInfo.Settings.CurrentGamepad[GameInfo.Settings.JumpKey].wasPressedThisFrame)
        {
            bool jumped = PlayerInfo.MovementManager.TryJump();
            if (jumped)
            {
                Exiting = true;
                keepSprintOnExit = true;
            }
        }
    }

    private void FallingTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!PlayerInfo.CharMoveSystem.Grounded && !PlayerInfo.CharMoveSystem.Kinematic.Value)
        {
            GameInfo.Manager.ReceivingInput.ClaimLock(PlayerInfo.MovementManager, GameInput.GameplayUnoverride);
            PlayerInfo.CharMoveSystem.HorizontalOnExit.ClaimLock(PlayerInfo.MovementManager, true);
            animator.SetBool(AnimationConstants.Player.Falling, true);
            Exiting = true;
            keepSprintOnExit = true;
        }
    }

    private void SlowdownTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerInfo.AnimationManager.TriggeredSlowdown)
        {
            PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.Slowdown);
            PlayerInfo.AnimationManager.TriggeredSlowdown = false;
            Exiting = true;
        }
    }

	private void LadderTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = forwardDirection + sidewaysDirection;

        if (PlayerInfo.Sensor.LadderBottom != null)
        {
            //Potential ladder to attach to
            Ladder contactLadder = PlayerInfo.Sensor.LadderBottom.transform.parent.GetComponent<Ladder>();

            //Normal distance
            float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.Normal);

            //Horizontal positioning
            float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.RightDirection);

            //Direction
            Vector2 invertedNormal = Matho.Rotate(Matho.StdProj2D(contactLadder.Normal), 180);

            if (normalProjectionScalar > 0 &&
                Mathf.Abs(horizontalProjectionScalar) < contactLadder.Width / 2 - PlayerInfo.Capsule.radius &&
                Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < (contactLadder.Depth / 2) + 1f &&
                Matho.AngleBetween(invertedNormal, movementDirection) < 45)
            {
                //Specific
                Ladder ladder = PlayerInfo.Sensor.LadderBottom.transform.parent.GetComponent<Ladder>();
                PlayerInfo.InteractionManager.Ladder = ladder;

                //Generate target positions
                Vector3 walkTargetPosition = ladder.transform.position;
                walkTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                walkTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                walkTargetPosition.y = ladder.transform.position.y - (ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Vector3 climbTargetPosition = walkTargetPosition + Vector3.up * 0.5f;

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, -ladder.Normal);

                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1);
                var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, targetRotation, AvatarTarget.Root, Vector3.one, 1); 
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);

                animator.SetTrigger("interacting");
                animator.SetTrigger("climbing");
                animator.SetTrigger("climbEnterBottom");

                Exiting = true;
                GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
                PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
                GameInfo.CameraController.AllowZoom = false;
            }
        }

        if (PlayerInfo.Sensor.LadderTop != null)
        {
            //Potential ladder to attach to
            Ladder contactLadder = PlayerInfo.Sensor.LadderTop.transform.parent.GetComponent<Ladder>();

            //Normal distance
            //float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.Normal);
            float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, -contactLadder.Normal);

            //Horizontal positioning
            float horizontalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - contactLadder.transform.position, contactLadder.RightDirection);

            //Direction
            Vector2 normal = Matho.StdProj2D(contactLadder.Normal);

            if (normalProjectionScalar > 0 &&
                Mathf.Abs(horizontalProjectionScalar) < contactLadder.Width / 2 - PlayerInfo.Capsule.radius &&
                Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < (contactLadder.Depth / 2) + 1f &&
                Matho.AngleBetween(normal, movementDirection) < 45)
            {
                //Specific
                Ladder ladder = PlayerInfo.Sensor.LadderTop.transform.parent.GetComponent<Ladder>();
                PlayerInfo.InteractionManager.Ladder = ladder;
                
                //Generate target positions
                Vector3 walkTargetPosition = ladder.transform.position;
                walkTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                walkTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                walkTargetPosition.y = ladder.transform.position.y + (ladder.Height / 2) + (PlayerInfo.Capsule.height / 2);

                Quaternion walkTargetRotation = Quaternion.FromToRotation(Vector3.forward, ladder.Normal);

                Vector3 climbTargetPosition = ladder.transform.position;
                climbTargetPosition += ladder.Normal * (PlayerInfo.Capsule.radius + ladder.Depth / 2);
                climbTargetPosition += ladder.RightDirection * horizontalProjectionScalar;
                climbTargetPosition.y = ladder.transform.position.y + (ladder.Height / 2);

                Quaternion climbTargetRotation = Quaternion.FromToRotation(Vector3.forward, -ladder.Normal);

                var walkTarget = new PlayerAnimationManager.MatchTarget(walkTargetPosition, walkTargetRotation, AvatarTarget.Root, Vector3.one, 1);
                var climbTarget = new PlayerAnimationManager.MatchTarget(climbTargetPosition, climbTargetRotation, AvatarTarget.Root, Vector3.one, 1); 
                PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(climbTarget);

                animator.SetTrigger("interacting");
                animator.SetTrigger("climbing");
                animator.SetTrigger("climbEnterTop");

                Exiting = true;
                GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
                PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
                GameInfo.CameraController.AllowZoom = false;
            }
        }
	}

    private void MantleTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerInfo.Sensor.MantleTop != null)
        {
            MantleDown(animator, stateInfo, layerIndex);
        }

        if (PlayerInfo.Sensor.MantleBottom != null)
        {
            MantleUp(animator, stateInfo, layerIndex);
        }
    }

    private void MantleDown(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = Matho.StdProj2D(PlayerInfo.Player.transform.forward);

        //Potential mantle to attach to
        Mantle mantle = PlayerInfo.Sensor.MantleTop;

        //Normal distance
        float normalProjectionScalar = Matho.ProjectScalar(PlayerInfo.Player.transform.position - mantle.transform.position, -mantle.Normal);

        if (normalProjectionScalar > 0 &&
            Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < 1f &&
            Matho.AngleBetween(Matho.StdProj2D(mantle.Normal), movementDirection) < 45)
        {
            //generate target positions
            Vector3 ledgePosition = PlayerInfo.Player.transform.position;
            ledgePosition += (Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius) * mantle.Normal;
            ledgePosition.y = mantle.TopVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 basePosition = ledgePosition;
            basePosition += (PlayerInfo.Capsule.radius * 2) * mantle.Normal;
            basePosition.y = mantle.BottomVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 walkPosition = basePosition + 1f * mantle.Normal;

            Quaternion inverseNormalRotation = Quaternion.LookRotation(-mantle.Normal, Vector3.up);
            Quaternion normalRotation = Quaternion.LookRotation(mantle.Normal, Vector3.up);

            //create and queue match targets
            if (mantle.Type == Mantle.MantleType.Short)
            {
                var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, normalRotation, AvatarTarget.Root, Vector3.one * 1.5f, 1); 
                var landTarget = new PlayerAnimationManager.MatchTarget(basePosition, Quaternion.identity, AvatarTarget.Root, Vector3.one, 0);
                PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(landTarget);
            }
            else
            {
                var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, inverseNormalRotation, AvatarTarget.Root, Vector3.one * 1.5f, 1); 
                var landTarget = new PlayerAnimationManager.MatchTarget(basePosition, Quaternion.identity, AvatarTarget.Root, Vector3.one, 0);
                var standupTarget = new PlayerAnimationManager.MatchTarget(basePosition, normalRotation, AvatarTarget.Root, Vector3.zero, 1);
                PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(landTarget);
                PlayerInfo.AnimationManager.EnqueueTarget(standupTarget);
            }
            
            
            PlayerInfo.Manager.test = ledgePosition;
            PlayerInfo.Manager.test2 = basePosition;

            //transition animator
            animator.SetTrigger("interacting");
            animator.SetTrigger("mantle");
            
            if (mantle.Type == Mantle.MantleType.Short)
                animator.SetTrigger("shortMantle");
            else
                animator.SetTrigger("tallMantle");

            animator.SetTrigger("mantleTop");
            GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
            PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
            GameInfo.CameraController.AllowZoom = false;
            GameInfo.CameraController.TargetDirection = mantle.Normal;
            Exiting = true;
        }
    }

    private void MantleUp(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 facingDirection = Matho.StdProj2D(PlayerInfo.Player.transform.forward);

        //Potential mantle to attach to
        Mantle mantle = PlayerInfo.Sensor.MantleBottom;

        //Normal distance
        float normalProjectionScalar = Mathf.Abs(Matho.ProjectScalar(PlayerInfo.Player.transform.position - mantle.transform.position, mantle.Normal));

        if (normalProjectionScalar > 0 &&
            Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius < 1f &&
            Matho.AngleBetween(Matho.StdProj2D(-mantle.Normal), facingDirection) < 45 &&
            (mantle.Type == Mantle.MantleType.Short || (mantle.Type == Mantle.MantleType.Tall)))//&& Input.GetKeyDown(GameInfo.Settings.JumpKey)
        {
            //generate target positions
            Vector3 basePosition = PlayerInfo.Player.transform.position;
            basePosition += (Mathf.Abs(normalProjectionScalar) - PlayerInfo.Capsule.radius) * -mantle.Normal;
            basePosition.y = mantle.BottomVerticalPosition + PlayerInfo.Capsule.height / 2;

            Vector3 ledgePosition = basePosition;
            ledgePosition.y = (mantle.Type == Mantle.MantleType.Short) ? mantle.TopVerticalPosition : mantle.TopVerticalPosition - (PlayerInfo.Capsule.height / 2);

            Vector3 walkPosition = new Vector3(0, mantle.TopVerticalPosition + PlayerInfo.Capsule.height / 2, 0);

            Quaternion rotation = Quaternion.FromToRotation(PlayerInfo.Player.transform.forward, -mantle.Normal) * PlayerInfo.Player.transform.rotation;

            //create and queue match targets
            var baseTarget = new PlayerAnimationManager.MatchTarget(basePosition, rotation, AvatarTarget.Root, Vector3.one, 1);
            var ledgeTarget = new PlayerAnimationManager.MatchTarget(ledgePosition, rotation, AvatarTarget.Root, Vector3.one, 0); 
            var walkTarget = new PlayerAnimationManager.MatchTarget(walkPosition, Quaternion.identity, AvatarTarget.Root, new Vector3(0, 1, 0), 0, 0.9f, 1); 
            PlayerInfo.AnimationManager.EnqueueTarget(baseTarget);
            PlayerInfo.AnimationManager.EnqueueTarget(ledgeTarget);
            PlayerInfo.AnimationManager.EnqueueTarget(walkTarget);

            PlayerInfo.Manager.test = basePosition;
            PlayerInfo.Manager.test2 = ledgePosition;

            //transition animator
            animator.SetTrigger("interacting");
            animator.SetTrigger("mantle");
            
            if (mantle.Type == Mantle.MantleType.Short)
                animator.SetTrigger("shortMantle");
            else
                animator.SetTrigger("tallMantle");

            animator.SetTrigger("mantleBottom");
            GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
            PlayerInfo.AnimationManager.IgnoreFallingAnimation = true;
            GameInfo.CameraController.AllowZoom = false;
            GameInfo.CameraController.TargetDirection = mantle.Normal;
            Exiting = true;
        }
    }
}