using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Implements basic movement ground movement based on the a provided PhysicsSystem parent.
//At the heart of the system is Move. This method is called when the object needs to move about the ground geometry. 
//Can be called multiple times per frame.
public class MovementSystem
{
    //Fields//
    //Object references
    protected GameObject parent;
    protected CapsuleCollider capsule;
    protected Vector3 bottomSphereOffset;
	protected PhysicsSystem physics;

    //Partitions
    public Vector3 movementVelocity;
    private Vector3 lastMovementVelocity;

    //Properties//
    //Information
    public Vector2 MovementDirection { get; protected set; }
    public Vector3 ExitPosition { get; protected set; }
    public Vector3 ExitVelocity { get; protected set; }

    //Settings
    public bool ExitEnabled { get; set; }

    public MovementSystem(GameObject parent, CapsuleCollider capsule, PhysicsSystem physics) 
    {
		this.parent = parent;
		this.capsule = capsule;
        this.physics = physics;
        bottomSphereOffset = capsule.BottomSphereOffset();
        ExitEnabled = true;
    }

    public virtual void UpdateSystem()
    {
        if (physics.EnteredFloor)
            OnEnterGround();

        if (physics.ExitedFloor)
            OnExitGround();

        if (physics.TouchingFloor)
            GroundClamp();

        lastMovementVelocity = movementVelocity;
        MovementDirection = Vector2.zero;
        movementVelocity = Vector3.zero;
    }

    public virtual void LateUpdateSystem()
    {
        if (!physics.Animating)
        {
            physics.ConstantVelocity += movementVelocity;
        }
        else
        {
            physics.AnimationVelocity += movementVelocity;
        }
    }

    public virtual void FixedUpdateSystem()
    {
        if (physics.TouchingFloor)
            physics.GroundDetectionDistance += movementVelocity.magnitude * Time.deltaTime;

        physics.SlopeStrength = movementVelocity.magnitude;
    }

    public virtual Vector3 Move(Vector2 direction, float speed, bool slopeEffectsSpeed = true)
    {
        if (physics.TouchingFloor && direction.magnitude != 0 && speed > 0)
        {          
            float slopeMagnitude = (slopeEffectsSpeed) ? SlopeMagnitude(physics.Theta) : 1;
            Vector3 slopeDirection = Matho.PlanarDirectionalDerivative(direction, physics.Normal).normalized;

            movementVelocity += speed * slopeMagnitude * slopeDirection;
            MovementDirection = Matho.StdProj2D(movementVelocity).normalized;

            return speed * slopeMagnitude * slopeDirection;
        }
        else
        { 
            return Vector3.zero;
        }
    }

    public float SlopeMagnitude(float theta)
    {
        if (theta < 45)
        {
            return 1.0f - (theta / 100);
        }
        else
        {
            return Mathf.Pow(theta / 45, 1/3f) - 0.65f;
        }
    }

    protected virtual void OnEnterGround() {}
    
    protected virtual void OnExitGround()
    {  
        //Dynamic velocity assignment.
        if (ExitEnabled)
        {
            physics.ImmediatePush(lastMovementVelocity);
            PlayerInfo.MovementManager.LockDirection();
            PlayerInfo.MovementManager.LockSpeed();
        }

        ExitPosition = capsule.transform.position;
        ExitVelocity = lastMovementVelocity;
    }

    protected virtual void GroundClamp()
    {
        if (physics.DynamicVelocity.y <= 0 && (!physics.Animating || (physics.Animating && physics.ClampWhileAnimating)))
        {
            Vector3 position = parent.transform.position + (physics.Distance - capsule.radius) * Vector3.down;
            parent.transform.position = position;
        }
    }
}