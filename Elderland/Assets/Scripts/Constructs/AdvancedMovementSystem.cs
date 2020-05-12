using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//More complex version of MovementSystem with added jumping logic.
public class AdvancedMovementSystem : MovementSystem
{
    //Properties//
    //Information
    public bool Jumping { get; private set; }
    
	public AdvancedMovementSystem(GameObject parent, CapsuleCollider capsule, PhysicsSystem physics) : base(parent, capsule, physics) { Jumping = true; }

    public override void LateUpdateSystem()
    {
        if (Jumping)
            movementVelocity.y = 0;

        if (!physics.Animating)
        {
            physics.ConstantVelocity += movementVelocity;
        }
        else
        {
            physics.AnimationVelocity += movementVelocity;
        }
    }

    public override void FixedUpdateSystem()
    {
        if (physics.TouchingFloor && !Jumping)
            physics.GroundDetectionDistance += movementVelocity.magnitude * Time.deltaTime;

        physics.SlopeStrength = movementVelocity.magnitude;
    }

    public void Jump(float strength)
    {
        if (physics.TouchingFloor && !Jumping)
        {
            Jumping = true;
            physics.DynamicZero(false, true, false);
            physics.Push(strength * Vector3.up);
        }
    }

    protected override void OnEnterGround()
    {
        Jumping = false;
    }

    protected override void GroundClamp()
    {
        if (physics.DynamicVelocity.y <= 0 && (!physics.Animating || (physics.Animating && physics.ClampWhileAnimating)) && !Jumping)
        {
            Vector3 position = parent.transform.position + (physics.Distance - capsule.radius) * Vector3.down;
            parent.transform.position = position;
        }
    }
}