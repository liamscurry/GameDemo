using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

//Deals with basic player movement.

public class PlayerMovementManager
{
    //References and associations
    private Ladder ladder;

    private Vector2 targetDirection;
    private float targetPercentileSpeed;

    private float directionSpeed = 500;

    private float speedVelocity;
    private float speedGradation = 0.2f; // 0.2f

    // For animator property percentileSpeed
    private const float percentileSpeedMin = 0.125f;
    private const float percentileSpeedSpeed = 1.5f;

    public Vector2 CurrentDirection { get; private set; }

    public float CurrentPercentileSpeed { get; private set; }
    public float AnimationPercentileSpeed { get; set; }
    public float PercSpeedObstructedModifier { get; set; }
    
    private bool sprintUnlocked;
    public bool SprintUnlocked { get { return sprintUnlocked; } }

    private const float sprintDisableAngle = 50;
    public const float FastFallSpeed = 7;

    private bool sprinting; // should only be referenced inside of property.
    public bool Sprinting {  
		get { return sprinting; } 

		set
		{
			if (sprinting && !value)
			{
				// Stopped sprinting
				GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0f, 0f));
			}
			else if (!sprinting & value)
			{
				// Started sprinting
				GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -2.5f, 0.9f));
			}
			sprinting = value;
		}
	}
    public bool SprintAvailable { get; set; }

    private const float jumpStrength = 4.0f;
    public bool Jumping { get; set; }

    private bool movedThisFrame;

    public Vector2 TargetDirection 
    {
        get { return targetDirection; }
        set
        {
            if (value.magnitude > 0)
                targetDirection = value.normalized;
        }
    }

    public float TargetPercentileSpeed 
    {  
        get { return targetPercentileSpeed; }
        set 
        { 
            targetPercentileSpeed = value; 
            if (targetPercentileSpeed < 0)
                targetPercentileSpeed = 0;
        }
    }

    public Vector3 ModelTargetForward 
    { 
        get 
        { 
            float percentage = 
                (Time.time - PlayerInfo.AbilityManager.LastDirFocus) / (PlayerAbilityManager.DirFocusDuration * 0.5f);
            percentage = Mathf.Clamp01(percentage);

            percentage = (percentage > 0.9) ? 1 : 0;
            
            return PlayerInfo.AbilityManager.DirFocus * (1 - percentage) +
                    GameInfo.CameraController.Direction * percentage;
        }
    }

    public PlayerMovementManager()
    {
        TargetDirection = Vector2.right;
        CurrentDirection = Vector2.right;
        TargetPercentileSpeed = 0;
        CurrentPercentileSpeed = 0;
        AnimationPercentileSpeed = 0;

        PlayerInfo.PhysicsSystem.SlidIntoGround += OnSlidIntoGround;
        movedThisFrame = false;
        SprintAvailable = true;
        Jumping = false;
    }

    public void UpdateMovement()
    {
        CurrentDirection =
            Matho.RotateTowards(CurrentDirection, TargetDirection, directionSpeed * Time.deltaTime);
        CurrentPercentileSpeed =
            Mathf.SmoothDamp(CurrentPercentileSpeed, TargetPercentileSpeed, ref speedVelocity, speedGradation);
        CurrentPercentileSpeed *= 1 - Matho.AngleBetween(TargetDirection, CurrentDirection) / 180f * 0.7f;

        /*if (PlayerInfo.PhysicsSystem.ExitedFloor && !PlayerInfo.CharMoveSystem.Jumping)
        {
            TargetPercentileSpeed = 0;
            SnapSpeed();
        }*/

        if (CurrentPercentileSpeed < percentileSpeedMin)
        {
            AnimationPercentileSpeed = 
                Mathf.MoveTowards(AnimationPercentileSpeed, 0, percentileSpeedSpeed * Time.deltaTime);
        }
        else
        {
            AnimationPercentileSpeed = 
                Mathf.MoveTowards(
                    AnimationPercentileSpeed,
                    1 * PercSpeedObstructedModifier,
                    percentileSpeedSpeed * Time.deltaTime);
        }
    }

    public void LateUpdateMovement()
    {
        movedThisFrame = false;
    }

    public bool TryJump()
    {
        if (GameInfo.Manager.ReceivingInput.Value == GameInput.Full)
        {
            GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.GameplayUnoverride);
            PlayerInfo.CharMoveSystem.HorizontalOnExit.ClaimLock(this, true);
            Jumping = true;
            PlayerInfo.CharMoveSystem.Push(Vector3.up * jumpStrength);
            PlayerInfo.Animator.SetBool(AnimationConstants.Player.Jump, true);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateWalkMovement(bool updateAnimProperties)
    {
        if (!movedThisFrame)
        {
            movedThisFrame = true;

            Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
            Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
            Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
            Vector2 movementDirection = forwardDirection + sidewaysDirection;

            UpdateSprinting(GameInfo.Settings.LeftDirectionalInput);

            //Direction and speed targets
            if (GameInfo.Settings.LeftDirectionalInput.magnitude <= 0.5f)
            {
                PlayerInfo.MovementManager.LockDirection();
                PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
                Sprinting = false;
                if (!PlayerInfo.Animator.IsInTransition(0))
                {
                    UpdateRotation(false);
                    if (updateAnimProperties)
                        PlayerInfo.AnimationManager.UpdateRotationProperties();
                }
            }
            else
            {
                Vector3 targetRotation =
                    Matho.StdProj3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;

                PlayerInfo.MovementManager.TargetDirection = movementDirection;

                float forwardsAngle = Matho.AngleBetween(Matho.StdProj2D(targetRotation), movementDirection);
                float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
                
                float sprintingModifier = (Sprinting) ? 2f : 1f;
            
                PlayerInfo.MovementManager.TargetPercentileSpeed =
                    GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier * sprintingModifier;
                if (!PlayerInfo.Animator.IsInTransition(0))
                {
                    UpdateRotation(true);
                    if (updateAnimProperties)
                        PlayerInfo.AnimationManager.UpdateRotationProperties();
                }
            }

            PlayerInfo.CharMoveSystem.GroundMove(
                PlayerInfo.MovementManager.CurrentDirection *
                PlayerInfo.MovementManager.CurrentPercentileSpeed *
                PlayerInfo.StatsManager.Movespeed);

            if (updateAnimProperties)
                PlayerInfo.AnimationManager.UpdateWalkProperties();
        }
    }

    /*
	Helper function for behaviours to update model rotation to camera forward (or blended if in combat).
	*/
	private void UpdateRotation(bool moving)
    {
        if (!moving)
        {
            UpdateStillModelRotation();
        }
        else
        {
            UpdateMovingModelRotation();
        }
    }

	private void UpdateStillModelRotation()
	{
		Vector3 targetRotation =
			Matho.StdProj3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;
		Vector3 currentRotation =
			Matho.StdProj3D(PlayerInfo.Player.transform.forward).normalized;

		if (Mathf.Abs(PlayerInfo.Animator.GetFloat("rotationSpeed")) > PlayerAnimationManager.RotationObserverMin)
		{
			Vector3 incrementedRotation =
				Vector3.RotateTowards(
					currentRotation,
					targetRotation,
					PlayerAnimationManager.ModelRotSpeedIdle * Time.deltaTime,
					0f);
			Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
			PlayerInfo.Player.transform.rotation = rotation;
		}
	}

	private void UpdateMovingModelRotation()
	{
		Vector3 targetRotation =
			Matho.StdProj3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;
		Vector3 currentRotation =
			Matho.StdProj3D(PlayerInfo.Player.transform.forward).normalized;

		Vector3 incrementedRotation =
			Vector3.RotateTowards(
				currentRotation,
				targetRotation,
				PlayerAnimationManager.ModelRotSpeedMoving * Time.deltaTime,
				0f);
		Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
		PlayerInfo.Player.transform.rotation = rotation;
	}

    private void UpdateSprinting(Vector2 analogMovementDirection)
    {
        if (Matho.AngleBetween(Vector2.up, analogMovementDirection) > sprintDisableAngle)
        {
            Sprinting = false;
            return;
        }

        if (GameInfo.Settings.CurrentGamepad.leftStickButton.isPressed &&
            SprintUnlocked &&   
            SprintAvailable &&
            PlayerInfo.AbilityManager.CurrentAbility == null)
        {
            Sprinting = true;
        }
    }

    public void ResetSprint()
    {
        Sprinting = false;
    }

    public Vector2 DirectionToPlayerCoord(Vector3 direction)
    {
        Vector2 up = Matho.StdProj2D(PlayerInfo.Player.transform.forward);
        Vector2 right = Matho.Rotate(up, 90f);
        Vector2 worldDir =
            GameInfo.CameraController.StdToCameraDir(direction);
        float projXInput = Matho.ProjectScalar(worldDir, right);
        float projYInput = Matho.ProjectScalar(worldDir, up);
        return new Vector2(projXInput, projYInput);
    }

    public void LockDirection()
    {
        TargetDirection = CurrentDirection;
    }

    public void SnapDirection()
    {
        CurrentDirection = TargetDirection;
    }

    public void LockSpeed()
    {
        TargetPercentileSpeed = CurrentPercentileSpeed;
    }

    public void SnapSpeed()
    {
        CurrentPercentileSpeed = TargetPercentileSpeed;
    }

    public void ZeroSpeed()
    {
        CurrentPercentileSpeed = 0;
        TargetPercentileSpeed = 0;
    }

    public void UnlockSprint()
    {
        sprintUnlocked = true;
    }

    public void LockSprint()
    {
        sprintUnlocked = false;
    }

    public void OnSlidIntoGround(object obj, System.EventArgs e)
    {
        ZeroSpeed();
    }
}