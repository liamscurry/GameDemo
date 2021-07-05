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

// Has to be a monobehaviour to get collision event calls.
public class CharacterMovementSystem : MonoBehaviour
{
    private CharacterController controller;

    private const float gravityStrength = 11f;
    private const float groundFrictionStrength = 50 * 0.3f;
    private const float dynamicMin = 0.0001f;

    private Vector3 dynamicVelocity;
    public Vector3 DynamicVelocity { get { return dynamicVelocity; } }
    private Vector3 gravityVelocity;
    public Vector3 GravityVelocity { get { return gravityVelocity; } }

    private Vector3 constantVelocity;
    private Vector3 airVelocity;

    private bool grounded;
    public bool Grounded { get { return grounded; } }
    private Vector3 groundNormal;

    private float groundSlopeLimit;
    private float minMoveDistance;

    private bool considerDynamicCollisions;
    private const float groundNormalDynamicDeviance = 45f;
    private const float groundNormalDynamicStrength = 2f;

    private GameObject effectedObject;

    public void Initialize(GameObject effectedObject)
    {
        this.effectedObject = effectedObject;
        controller = effectedObject.GetComponent<CharacterController>();
        grounded = false;
        groundSlopeLimit = controller.slopeLimit;
        minMoveDistance = controller.minMoveDistance;
        considerDynamicCollisions = false;
    }

    // Almost all ported over.
    // analogInput : world space x and z deltas
    public void GroundMove(Vector2 analogInput)
    {
        if (grounded)
        {
            Vector2 normalizedInput = analogInput.normalized;
            //Vector3 worldInput = effectedObject.transform.forward * normalizedInput.y;
            //worldInput += effectedObject.transform.right * normalizedInput.x;
            Vector3 movementDirection = Matho.PlanarDirectionalDerivative(normalizedInput, groundNormal);
            
            constantVelocity += movementDirection * analogInput.magnitude;
        }
    }

    // All ported over.
    public void Push(Vector3 velocity)
    {
        dynamicVelocity += velocity;
    }

    public void UpdateSystem()
    {
        if (!grounded)
        {
            UpdateAirMovement();
            controller.slopeLimit = Mathf.Infinity;
        }
        else
        {
            controller.slopeLimit = groundSlopeLimit;
        }
    }

    public void LateUpdateSystem()
    {
        Vector3 compoundVelocity = Vector3.zero;
        
        controller.minMoveDistance = minMoveDistance * Time.timeScale;

        if (grounded)
        {
            compoundVelocity += constantVelocity;
            compoundVelocity += dynamicVelocity;
            
            considerDynamicCollisions = true;
            controller.Move(compoundVelocity * Time.deltaTime);
            considerDynamicCollisions = false;

            GroundClamp();

            GroundFriction();
        }
        else
        {
            compoundVelocity += airVelocity;
            compoundVelocity += gravityVelocity;
            
            considerDynamicCollisions = true;
            controller.Move(compoundVelocity * Time.deltaTime);
            considerDynamicCollisions = false;
        }

        if (grounded)
            CheckForGroundExit();

        constantVelocity = Vector3.zero;
    }

    private void GroundClamp()
    {
        if (!(Matho.AngleBetween(groundNormal, dynamicVelocity) < groundNormalDynamicDeviance &&
            dynamicVelocity.magnitude > groundNormalDynamicStrength))
        {
            controller.Move(effectedObject.transform.up * -1 * controller.stepOffset);
        }
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

    private void UpdateAirMovement()
    {
        gravityVelocity += gravityStrength * Time.deltaTime * Vector3.down;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckForGroundEnter(hit);
        if (considerDynamicCollisions)
            HandleVelocityCollisions(hit);
    }

    private void CheckForGroundEnter(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) < groundSlopeLimit)
        {
            if (!grounded)
            {
                grounded = true;

                dynamicVelocity = airVelocity + gravityVelocity;
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
