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

    private const float gravityStrength = 9.8f;
    private const float walkSpeed = 15;
    private const float groundFrictionStrength = 50 * 0.3f;
    private const float dynamicMin = 0.0001f;

    private Vector3 dynamicVelocity;
    private Vector3 gravityVelocity;

    private Vector3 constantVelocity;
    private Vector3 airVelocity;

    private bool grounded;
    private Vector3 groundNormal;

    private float groundSlopeLimit;
    private float minMoveDistance;

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
        minMoveDistance = controller.minMoveDistance;
    }

    private void Update()
    {
        if (grounded)
        {
            UpdateGroundMovement();
            controller.slopeLimit = groundSlopeLimit;
        }
        else
        {
            UpdateAirMovement();
            controller.slopeLimit = Mathf.Infinity;
        }
    }

    private void LateUpdate()
    {
        Vector3 compoundVelocity = Vector3.zero;
        
        controller.minMoveDistance = minMoveDistance * Time.timeScale;

        if (grounded)
        {
            compoundVelocity += constantVelocity;
            compoundVelocity += dynamicVelocity;
            
            controller.Move(compoundVelocity * Time.deltaTime);
            controller.Move(transform.up * -1 * controller.stepOffset);

            GroundFriction();
        }
        else
        {
            compoundVelocity += airVelocity;
            compoundVelocity += gravityVelocity;
            
            controller.Move(compoundVelocity * Time.deltaTime);
        }

        if (grounded)
            CheckForGroundExit();

        constantVelocity = Vector3.zero;
    }

    private void GroundFriction()
    {
        Vector3 dynamicDir = dynamicVelocity.normalized;
        float dynamicMag = dynamicVelocity.magnitude;
        dynamicMag -= groundFrictionStrength * Time.deltaTime;

        if (dynamicMag > dynamicMin)
        {
            dynamicVelocity = dynamicMag * dynamicDir;
        }
        else
        {
            dynamicVelocity = Vector3.zero;
        }
    }

    private void UpdateGroundMovement()
    {
        Vector2 analogInput = LeftDirectionalInput.normalized;
        
        Vector3 worldInput = transform.forward * analogInput.y;
        worldInput += transform.right * analogInput.x;
        Vector2 projWorldInput = Matho.StdProj2D(worldInput);
        Vector3 movementDirection = Matho.PlanarDirectionalDerivative(projWorldInput, groundNormal);
        
        constantVelocity += movementDirection * walkSpeed;
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
            if (!grounded)
            {
                grounded = true;

                dynamicVelocity = airVelocity;
                constantVelocity = Vector3.zero;

                gravityVelocity = Vector3.zero;
                airVelocity = Vector3.zero;
            }
            
            groundNormal = hit.normal;
        }
    }

    private void CheckForGroundExit()
    {
        if (!controller.isGrounded)
        {
            grounded = false;
            airVelocity = constantVelocity + dynamicVelocity;
        }
    }

    // Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    // issue with grounded checks (called often and dynamic velocity approaches zero quickly)
    public void HandleVelocityCollisions(ControllerColliderHit hit)
    {
        Vector3 n = hit.normal;
        Vector3 v = dynamicVelocity.normalized;

        float velocityTheta = Matho.AngleBetween(n, v);

        if (velocityTheta >= 90)
        {
            Vector3 m = -1 * n;
            Vector3 nPerp = dynamicVelocity - Matho.Project(dynamicVelocity, m);
            dynamicVelocity = nPerp;
        }
    }
}
