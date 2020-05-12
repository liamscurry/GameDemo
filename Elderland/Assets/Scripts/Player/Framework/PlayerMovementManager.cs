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
    private float speedGradation = 0.2f;

    private bool sprintAvailable;

    public Vector2 CurrentDirection { get; private set; }
    public float CurrentPercentileSpeed { get; private set; }
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

        PlayerInfo.PhysicsSystem.SlidIntoGround += OnSlidIntoGround;
    }

    public void UpdateMovement()
    {
        CurrentDirection = Matho.RotateTowards(CurrentDirection, TargetDirection, directionSpeed * Time.deltaTime);

        CurrentPercentileSpeed = Mathf.SmoothDamp(CurrentPercentileSpeed, TargetPercentileSpeed, ref speedVelocity, speedGradation);
        CurrentPercentileSpeed *= 1 - Matho.AngleBetween(TargetDirection, CurrentDirection) / 180f;

        if (PlayerInfo.PhysicsSystem.ExitedFloor && !PlayerInfo.MovementSystem.Jumping)
        {
            TargetPercentileSpeed = 0;
            SnapSpeed();
        }
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