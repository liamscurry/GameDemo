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

    private bool sprintAvailable;
    
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
    public bool SprintAvailable { get { return sprintAvailable; } }

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
        sprintAvailable = true;
    }

    public void LockSprint()
    {
        sprintAvailable = false;
    }

    public void OnSlidIntoGround(object obj, System.EventArgs e)
    {
        ZeroSpeed();
    }
}