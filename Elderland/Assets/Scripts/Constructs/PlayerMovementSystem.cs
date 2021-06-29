using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Other movement systems will be deleted in addition to the physics system classes
// after this implementation is complete.

// Character controller base movement:
// away from slope, flies in air.
// towards slope, moves up hill
// straight down at slope, does not move.
// diagonal, moves horizontally and then vertically (or vice versa)
public class PlayerMovementSystem : MonoBehaviour
{
    private CharacterController controller;

    private const float gravityStrength = 0.1f;
    private const float groundGravityStrength = 5f;
    private const float walkSpeed = 12f;

    private Vector3 dynamicVelocity;
    private Vector3 gravityVelocity;

    private Vector3 constantVelocity;
    private Vector3 airVelocity;

    private bool grounded;
    private Vector3 groundNormal;

    private float groundSlopeLimit;

    // Will use game info version when porting to full game.
    public Vector2 LeftDirectionalInput 
    { 
        get 
        { 
            Vector2 v =
                new Vector2(Input.GetAxis("Left Joystick Horizontal"), Input.GetAxis("Left Joystick Vertical")); 

            //Unit length restriction
            if (v.magnitude > 1f)
                v = v.normalized;

            //Dead zone restriction
            if (v.magnitude < 0.2f)
                v = Vector2.zero;

            return v;
        } 
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        grounded = false;
        groundSlopeLimit = controller.slopeLimit;
    }

    private void Update()
    {
        if (grounded)
        {
            UpdateGroundMovement();
            controller.slopeLimit = 45;
        }
        else
        {
            UpdateAirMovement();
            controller.slopeLimit = Mathf.Infinity;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Time.timeScale = 0.05f;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 1f;
        }
    }

    private void LateUpdate()
    {
        Vector3 compoundVelocity = Vector3.zero;
        
        if (grounded)
        {
            compoundVelocity += constantVelocity;
            compoundVelocity += dynamicVelocity;
            controller.Move(compoundVelocity);
        }
        else
        {
            compoundVelocity += airVelocity;
            compoundVelocity += gravityVelocity;
            controller.Move(compoundVelocity);
        }
        

        if (grounded)
            CheckForGroundExit();

        constantVelocity = Vector3.zero;
    }

    private void UpdateGroundMovement()
    {
        Vector2 analogInput = LeftDirectionalInput.normalized;
        
        Vector3 worldInput = transform.forward * analogInput.y;
        worldInput += transform.right * analogInput.x;
        Vector2 projWorldInput = Matho.StdProj2D(worldInput);
        Vector3 movementDirection = Matho.PlanarDirectionalDerivative(projWorldInput, groundNormal);
        constantVelocity += movementDirection * walkSpeed * Time.deltaTime;
        constantVelocity += transform.up * -1 * groundGravityStrength * Time.deltaTime;
    }

    private void UpdateAirMovement()
    {
        gravityVelocity += gravityStrength * Time.deltaTime * Vector3.down;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckForGroundEnter(hit);
        HandleVelocityCollisions(hit);
    }

    private void CheckForGroundEnter(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) < groundSlopeLimit)
        {
            grounded = true;
            groundNormal = hit.normal;
            gravityVelocity = Vector3.zero;
            airVelocity = dynamicVelocity;
        }
    }

    private void CheckForGroundExit()
    {
        if (!controller.isGrounded)
        {
            grounded = false;
            airVelocity = constantVelocity;
        }
        /*
        if (!Physics.CapsuleCast(
            transform.position + (controller.height / 2 - controller.radius) * Vector3.up,
            transform.position + (controller.height / 2 - controller.radius) * Vector3.down,
            controller.radius - controller.skinWidth,
            Vector3.down,
            controller.stepOffset,
            LayerConstants.GroundCollision))
        {
            grounded = false;
            exitConstantVelocity = constantVelocity;
            Debug.Log(exitConstantVelocity.x + ", " + exitConstantVelocity.y + ", " + exitConstantVelocity.z);
        }*/
    }

    // Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    public void HandleGravityCollisions(ControllerColliderHit hit)
    {
        Vector3 n = hit.normal;
        Vector3 v = gravityVelocity.normalized;

        float velocityTheta = Matho.AngleBetween(n, v);

        if (velocityTheta >= 90)
        {
            Vector3 m = -1 * n;
            Vector3 nPerp = gravityVelocity - Matho.Project(gravityVelocity, m);
            gravityVelocity = nPerp;
        }
    }

    // Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    public void HandleVelocityCollisions(ControllerColliderHit hit)
    {
        Vector3 n = hit.normal;
        Vector3 v = dynamicVelocity.normalized;

        float velocityTheta = Matho.AngleBetween(n, v);

        if (velocityTheta >= 90)
        {
            Vector3 m = -1 * n;
            Vector3 nPerp = dynamicVelocity - Matho.Project(dynamicVelocity, m);
            dynamicVelocity = 0.5f * nPerp;
        }
    }
}
