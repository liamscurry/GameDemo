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

    private bool groundCheck;
    private bool grounded;
    public bool Grounded { get { return grounded; } }
    public StatLock<bool> ApplyGravity { get; private set; }
    private Vector3 groundNormal;

    private float groundSlopeLimit;
    private const float groundSlopeThreshold = 3f;
    private float minMoveDistance;

    private bool considerDynamicCollisions;
    private const float groundNormalDynamicDeviance = 45f;
    private const float groundNormalDynamicStrength = 2f;

    private GameObject effectedObject;

    private const float exitNormalDuration = 0.5f;
    private Queue<(float, Vector3)> exitNormalQueue;

    public void Initialize(GameObject effectedObject)
    {
        this.effectedObject = effectedObject;
        controller = effectedObject.GetComponent<CharacterController>();
        grounded = false;
        groundSlopeLimit = controller.slopeLimit;
        minMoveDistance = controller.minMoveDistance;
        considerDynamicCollisions = false;
        ApplyGravity = new StatLock<bool>(true);
        exitNormalQueue = new Queue<(float, Vector3)>();
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
            Vector3 movementDirection =
                Matho.PlanarDirectionalDerivative(normalizedInput, groundNormal).normalized;
            
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

        if (Input.GetKeyDown(KeyCode.B))
        {
            ApplyGravity.ClaimLock(this, false);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            Time.timeScale = 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Time.timeScale = 1;
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

            GroundCheck();

            GroundFriction();

            ParseExitNormalQueue();
            CheckForGroundExit();
        }
        else
        {
            compoundVelocity += airVelocity;
            compoundVelocity += gravityVelocity;

            considerDynamicCollisions = true;
            controller.Move(compoundVelocity * Time.deltaTime);
            considerDynamicCollisions = false;
        }

        constantVelocity = Vector3.zero;
    }

    private void GroundCheck()
    {
        if (!(Matho.AngleBetween(groundNormal, dynamicVelocity) < groundNormalDynamicDeviance &&
            dynamicVelocity.magnitude > groundNormalDynamicStrength))
        {
            float topOffset = (controller.height / 2 - controller.radius);

            // will make check consist of a circle of raycasts downwards around the circumference of the
            // capsule, if half of them miss, they we have disconnected from the ground,
            // tilt of ground effects length of cast (each may be different lengths)
            float percentageGrounded = PercentageOnGround();
            groundCheck = (percentageGrounded > 0.5f) ? true : false;

            controller.Move(effectedObject.transform.up * -2f * controller.skinWidth);
        }
        else
        {
            groundCheck = false;
        }
    }

    // Casts rays below the character, changing length based on the current ground normal
    // Adds constant length of multiple of step size of controller.
    private float PercentageOnGround()
    {
        // outer circle
        int outerCastCount = 8;
        int outerCount = RaycastCircle(outerCastCount, controller.radius);

        // inner circle
        int innerCastCount = 4;
        int innerCount = RaycastCircle(innerCastCount, controller.radius * 0.5f);

        return (outerCount + innerCount) / ((float) outerCastCount + innerCastCount);
    }

    private int RaycastCircle(int n, float radius)
    {
        int castsHit = 0;
        for (int i = 0; i < n; i++)
        {
            float angle = (i + 1f) / n * 360;
            Vector2 horizontalOffset = Matho.DirectionVectorFromAngle(angle) * radius;
            float normalAngle = Matho.AngleBetween(Vector3.up, groundNormal);
            
            float normalOffset =
                Matho.PlanarDirectionalDerivative(horizontalOffset, groundNormal).y * radius;

            Vector3 start =
                transform.position + new Vector3(horizontalOffset.x, 0, horizontalOffset.y);
            
            float castLength = controller.height / 2f;
            castLength -= normalOffset;
            castLength += radius * (1 - Mathf.Cos(normalAngle * Mathf.Deg2Rad));
            castLength += controller.skinWidth;
            castLength += controller.contactOffset * 2f;
            castLength += controller.stepOffset;

            if (Physics.Linecast(start, start + Vector3.down * castLength, LayerConstants.GroundCollision))
                castsHit++;
            
            // Visualize cast
            //Debug.DrawLine(start, start + Vector3.down * castLength, Color.black, 1f);
        }
        return castsHit;
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
        if (ApplyGravity.Value)
        {
            gravityVelocity += gravityStrength * Time.deltaTime * Vector3.down;
        }
        else
        {
            gravityVelocity = Vector3.zero;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckForGroundEnter(hit);
        if (considerDynamicCollisions)
            HandleVelocityCollisions(hit);
    }

    private void CheckForGroundEnter(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) < groundSlopeLimit + groundSlopeThreshold)
        {
            if (!grounded)
            {
                grounded = true;

                dynamicVelocity = airVelocity + gravityVelocity;
                constantVelocity = Vector3.zero;

                gravityVelocity = Vector3.zero;
                airVelocity = Vector3.zero;
                Debug.Log("ground entered");
            }
            
            groundNormal = hit.normal;
            if (grounded)
            {
                exitNormalQueue.Enqueue((Time.time, groundNormal));
            }
        }
    }

    private void CheckForGroundExit()
    {
        //if (!controller.isGrounded)
        if (!groundCheck)
        {
            grounded = false;

            Vector2 normalizedInput = Matho.StdProj2D(constantVelocity).normalized;
            Vector3 exitNormal = AverageExitNormalQueue();
            Vector3 movementDirection =
                Matho.PlanarDirectionalDerivative(normalizedInput, exitNormal).normalized;
            constantVelocity = movementDirection * constantVelocity.magnitude;
            exitNormalQueue.Clear();

            airVelocity = constantVelocity + dynamicVelocity; 
            Debug.Log("ground exited");
        }
    }

    // Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    // issue with grounded checks (called often and dynamic velocity approaches zero quickly)
    private void HandleVelocityCollisions(ControllerColliderHit hit)
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

    /*
    * Helper function for determining travel direction upon leaving ground
    * Parses out old vector time pairs that exceed direction average time.
    */
    private void ParseExitNormalQueue()
    {
        while (exitNormalQueue.Count != 0)
        {
            var pair = exitNormalQueue.Peek();
            if (Time.time - pair.Item1 > exitNormalDuration)
            {
                exitNormalQueue.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    private Vector3 AverageExitNormalQueue()
    {
        Vector3 normal = groundNormal;
        foreach (var pair in exitNormalQueue)
        {
            normal += pair.Item2;
        }
        normal *= 1f / (1f + exitNormalQueue.Count);
        return normal;
    }
}
