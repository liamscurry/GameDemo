using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Deals with basic player movement.

public class PlayerMovementManager
{
    public enum State { Traversing, Jumping, Climbing }

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

    // 0 is stationary, 1 is rotation to the right of the character, -1 is rotating left.
    public float CurrentRotationSpeed { get; private set; }
    public const float RotationStartMin = 45f;
    public const float RotationStopMin = 2f;
    private const float rotationSpeedSpeedIncrease = 7f;
    private const float rotationSpeedSpeedDecrease = 3f;

    public float CurrentPercentileSpeed { get; private set; }
    public float PercentileSpeed { get; set; }
    public float PercSpeedObstructedModifier { get; set; }
    
    private bool sprintUnlocked;
    public bool SprintUnlocked { get { return sprintUnlocked; } }

    private const float sprintDisableAngle = 50;

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
            
            return PlayerInfo.AbilityManager.DirFocus * (1 - percentage) +
                    GameInfo.CameraController.Direction * percentage;
        }
    }

    public State MovementState { get; private set; }

    public PlayerMovementManager()
    {
        MovementState = State.Traversing;
        TargetDirection = Vector2.right;
        CurrentDirection = Vector2.right;
        TargetPercentileSpeed = 0;
        CurrentPercentileSpeed = 0;
        PercentileSpeed = 0;

        PlayerInfo.PhysicsSystem.SlidIntoGround += OnSlidIntoGround;
        movedThisFrame = false;
        SprintAvailable = true;
    }

    public void UpdateMovement()
    {
        CurrentDirection = Matho.RotateTowards(CurrentDirection, TargetDirection, directionSpeed * Time.deltaTime);

        CurrentPercentileSpeed = Mathf.SmoothDamp(CurrentPercentileSpeed, TargetPercentileSpeed, ref speedVelocity, speedGradation);
        CurrentPercentileSpeed *= 1 - Matho.AngleBetween(TargetDirection, CurrentDirection) / 180f * 0.7f;

        if (PlayerInfo.PhysicsSystem.ExitedFloor && !PlayerInfo.MovementSystem.Jumping)
        {
            TargetPercentileSpeed = 0;
            SnapSpeed();
        }

        if (CurrentPercentileSpeed < percentileSpeedMin)
        {
            PercentileSpeed = 
                Mathf.MoveTowards(PercentileSpeed, 0, percentileSpeedSpeed * Time.deltaTime);
        }
        else
        {
            PercentileSpeed = 
                Mathf.MoveTowards(
                    PercentileSpeed,
                    1 * PercSpeedObstructedModifier,
                    percentileSpeedSpeed * Time.deltaTime);
        }
        
        UpdateRotationSpeed();
    }

    public void LateUpdateMovement()
    {
        movedThisFrame = false;
    }

    public void UpdateWalkMovement()
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
                    PlayerInfo.AnimationManager.UpdateRotation(false);
            }
            else
            {
                Vector3 targetRotation =
                    Matho.StandardProjection3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;

                PlayerInfo.MovementManager.TargetDirection = movementDirection;

                float forwardsAngle = Matho.AngleBetween(Matho.StdProj2D(targetRotation), movementDirection);
                float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
                
                float sprintingModifier = (Sprinting) ? 2f : 1f;
            
                PlayerInfo.MovementManager.TargetPercentileSpeed =
                    GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier * sprintingModifier;
                if (!PlayerInfo.Animator.IsInTransition(0))
                    PlayerInfo.AnimationManager.UpdateRotation(true);
            }

            PlayerInfo.MovementSystem.Move(
                PlayerInfo.MovementManager.CurrentDirection,
                PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.Movespeed);
        }
    }

    private void UpdateSprinting(Vector2 analogMovementDirection)
    {
        if (Matho.AngleBetween(Vector2.up, analogMovementDirection) > sprintDisableAngle)
        {
            Sprinting = false;
            return;
        }

        if (Input.GetKeyDown(GameInfo.Settings.SprintKey) && SprintUnlocked && SprintAvailable &&
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
            GameInfo.CameraController.StandardToCameraDirection(direction);
        float projXInput = Matho.ProjectScalar(worldDir, right);
        float projYInput = Matho.ProjectScalar(worldDir, up);
        return new Vector2(projXInput, projYInput);
    }

    private void UpdateRotationSpeed()
    {
        int targetRotationSpeed = 0;
        Vector3 targetDirection3D = Matho.StandardProjection3D(GameInfo.CameraController.Direction).normalized;
        Vector3 currentDirection3D = Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;
   
        if (Matho.AngleBetween(targetDirection3D, currentDirection3D) > RotationStartMin)
        {
            Vector3 zenith = Vector3.Cross(targetDirection3D, currentDirection3D);
            
            if (Matho.AngleBetween(zenith, Vector3.up) < 90f)
            {
                targetRotationSpeed = -1;
            }
            else
            {
                targetRotationSpeed = 1;
            }
        }

        float usedRotSpeed = (targetRotationSpeed != 0) ? rotationSpeedSpeedIncrease : rotationSpeedSpeedDecrease;
        CurrentRotationSpeed =
            Mathf.MoveTowards(
                CurrentRotationSpeed,
                targetRotationSpeed,
                usedRotSpeed * Time.deltaTime);
        
        PlayerInfo.Animator.SetFloat("rotationSpeed", CurrentRotationSpeed);
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