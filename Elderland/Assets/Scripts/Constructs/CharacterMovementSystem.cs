//#define DebugOutput

using System;
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
    public CharacterController Controller { get { return controller; } }

    private const float gravityStrength = 11f;
    private const float groundFrictionStrength = 50 * 0.3f;
    private const float dynamicMin = 0.0001f;

    private Vector3 dynamicVelocity;
    public Vector3 DynamicVelocity { get { return dynamicVelocity; } }

    private Vector3 constantVelocity;
    private Vector3 dynamicAirVelocity;
    public Vector3 DynamicAirVelocity { get { return dynamicAirVelocity; } }
    private Vector3 constantAirVelocity;

    private bool clampCheck;
    private const float centerClampThreshold = 0.01f;
    private const float clampCastWidth = 0.025f;
    private bool groundCheck;
    private bool grounded;
    public bool Grounded { get { return grounded; } }
    private Vector3 groundNormal;
    public Vector3 GroundNormal { get { return (grounded) ? groundNormal : Vector3.up; } }

    public StatLock<bool> HorizontalOnExit { get; private set; }
    public StatLock<float> MaxConstantOnExit { get; private set; }

    private float groundSlopeLimit;
    private const float groundSlopeThreshold = 3f;
    private float minMoveDistance;

    private bool considerDynamicCollisions;
    private const float groundNormalAngleDeviance = 90f - 25f;
    private const float groundNormalMagStrength = 2f;

    private GameObject effectedObject;

    private const float exitNormalDuration = 0.5f;
    private Queue<(float, Vector3)> exitNormalQueue;

    private const float clampCheckTreshold = 0.2f;

    private Action onKinematicOff;
    public StatLock<bool> Kinematic { get; private set; }

    /*
    Initialize needed instead of constructor. Called in player info initializer.

    Inputs:
    GameObject : effectedObject : which object the script is moving.
    Action : onKinematicOff : method to be called when the character controller starts considering its velocity.
    */
    public void Initialize(GameObject effectedObject)
    {
        this.effectedObject = effectedObject;
        controller = effectedObject.GetComponent<CharacterController>();
        grounded = false;
        groundSlopeLimit = controller.slopeLimit;
        minMoveDistance = controller.minMoveDistance;
        considerDynamicCollisions = false;
        HorizontalOnExit = new StatLock<bool>(false);
        MaxConstantOnExit = new StatLock<float>(float.MaxValue);
        exitNormalQueue = new Queue<(float, Vector3)>();
        Kinematic = new StatLock<bool>(false, OnKinematicChange);
    }

    /*
    Helper delagate used to notify the system that kinematic has been enabled or disabled.
    Only gets called when new value is set for the lock.

    Inputs:
    bool : newValue : new value lock has been changed to.

    Outputs:
    None
    */
    private void OnKinematicChange(bool newValue)
    {
        if (newValue)
        {
            grounded = false;
        }
        else
        {
            constantAirVelocity = Vector3.zero;
            constantVelocity = Vector3.zero;
            dynamicAirVelocity = Vector3.zero;
            dynamicVelocity = Vector3.zero;

            KinematicCheckForGround();

            if (onKinematicOff != null)
                onKinematicOff.Invoke();
        }
    }

    // Explicit setter for Action onKinematicOff
    public void SetOnKinematicOff(Action onKinematicOff)
    {
        this.onKinematicOff = onKinematicOff;
    }

    /*
    Helper method called when kinematic mode is switched to kinematic off.
    This method checks to see if calls to ground checks would result in the character
    leaving the ground.

    Inputs:
    None

    Outputs:
    None
    */
    private void KinematicCheckForGround()
    {
        bool kinematicClampCheck = ClampCheck();
        if (kinematicClampCheck)
        {
            controller.Move(effectedObject.transform.up * -2f * controller.skinWidth);
        }
    }

    /*
    Resets character move system's velocity partitions to zero.

    Inputs:
    None

    Outputs:
    None
    */
    public void ResetVelocities()
    {
        dynamicAirVelocity = Vector3.zero;
        dynamicVelocity = Vector3.zero;
        constantAirVelocity = Vector3.zero;
        constantVelocity = Vector3.zero;
    }

    // Almost all ported over.
    // analogInput : world space x and z deltas
    public void GroundMove(Vector2 analogInput)
    {
        if (grounded)
        {
            Vector2 normalizedInput = analogInput.normalized;
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
        if (!Kinematic.Value)
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
    }

    public void LateUpdateSystem()
    {
        if (!Kinematic.Value)
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

                clampCheck = ClampCheck();
                if (clampCheck)
                {
                    controller.Move(effectedObject.transform.up * -2f * controller.skinWidth);
                    groundCheck = GroundCheck(compoundVelocity, false);
                }
                else
                {
                    groundCheck = false;
                    #if DebugOutput
                    Debug.Log("Exit: Clamp detected steep ledge");
                    #endif
                }

                GroundFriction();
                //ParseExitNormalQueue();
                CheckForGroundExit();
            }
            else
            {
                compoundVelocity += constantAirVelocity;
                compoundVelocity += dynamicAirVelocity;

                #if DebugOutput
                Debug.Log("Constant Air velocity: " + constantAirVelocity);
                Debug.Log("Dynamic Air  velocity: " + dynamicAirVelocity);
                Debug.Log("Compound velocity: " + compoundVelocity);
                #endif

                considerDynamicCollisions = true;
                controller.Move(compoundVelocity * Time.deltaTime);
                considerDynamicCollisions = false;
            }

            constantVelocity = Vector3.zero;
        }
    }

    /*
    * Returns whether character should clamp to ground.
    */
    private bool ClampCheck()
    {
        RaycastHit hitInfo;
        Vector3 topOffset = (controller.height / 2 - controller.radius) * Vector3.up;
        bool hit = Physics.CapsuleCast(
            transform.position + topOffset,
            transform.position - topOffset,
            controller.radius,
            Vector3.down,
            out hitInfo,
            controller.height / 2,
            LayerConstants.GroundCollision
        );

        if (hit)
        {
            Vector3 normal = hitInfo.normal;
            Vector2 hitOffset = Matho.StdProj2D(hitInfo.point - transform.position);
            if (hitOffset.magnitude < centerClampThreshold)
            {
                return true;
            }
            else
            { 
                return ClampEdgeCheck(hitInfo);
            }
        }
        else
        {
            return false;
        }
    }

    private bool ClampEdgeCheck(RaycastHit capsuleHitInfo)
    {
        // works up to here, developing step 3. 
        // So far in step, cast start positions correct.
        Vector3 castOffset = Matho.StdProj3D(capsuleHitInfo.point - transform.position);
        Vector3 castOrientation = castOffset.normalized;
        Vector3 nearCastStart =
            transform.position + castOffset + castOrientation * clampCastWidth;
        Vector3 farCastStart =
            transform.position + castOffset - castOrientation * clampCastWidth;

        RaycastHit nearHitInfo;
        RaycastHit farHitInfo;

        bool nearHit = Physics.Raycast(
            nearCastStart,
            Vector3.down,
            out nearHitInfo,
            controller.height,
            LayerConstants.GroundCollision
        );

        bool farHit = Physics.Raycast(
            farCastStart,
            Vector3.down,
            out farHitInfo,
            controller.height,
            LayerConstants.GroundCollision
        );

        Debug.DrawLine(nearCastStart, nearCastStart + Vector3.down, Color.magenta, 0.1f);
        Debug.DrawLine(farCastStart, farCastStart + Vector3.down, Color.magenta, 0.1f);

        if (!nearHit || !farHit)
        {
            return false;
        }
        else
        {
            float clampDistance =
                controller.stepOffset +
                controller.skinWidth +
                controller.contactOffset * 2 +
                clampCheckTreshold;
            if (nearHitInfo.point.y + clampDistance < capsuleHitInfo.point.y)
            {
                return false;
            }

            if (farHitInfo.point.y + clampDistance  < capsuleHitInfo.point.y)
            {
                return false;
            }

            return true;
        }
    }

    private bool GroundCheck(Vector3 compoundVelocity, bool hitGroundViaEvent)
    {
        bool check;
        if (!(Matho.AngleBetween(groundNormal, compoundVelocity) < groundNormalAngleDeviance &&
            compoundVelocity.magnitude > groundNormalMagStrength))
        {
            if (!hitGroundViaEvent)
            {
                check = controller.isGrounded;
                #if DebugOutput
                if (!check)
                    Debug.Log("Exit: controller not grounded from .IsGroundedFlag");
                #endif
            }
            else
            {
                check = true;
            }
        }
        else
        {
            #if DebugOutput
            Debug.Log("Exit: velocity is ~normal to surface");
            #endif
            check = false;
        }
        return check;
    }

    private bool WeightCheck(Vector3 compoundVelocity)
    {
        float topOffset = (controller.height / 2 - controller.radius);

        // will make check consist of a circle of raycasts downwards around the circumference of the
        // capsule, if half of them miss, they we have disconnected from the ground,
        // tilt of ground effects length of cast (each may be different lengths)
        int numberGrounded;
        float percentageGrounded;
        Vector2 groundedAveragePos;
        (numberGrounded, percentageGrounded, groundedAveragePos) = PercentageOnGround();

        #if DebugOutput
        Debug.Log("Percent Grounded: " + percentageGrounded + ", Grounded Avg Pos: " + groundedAveragePos);
        #endif

        bool check;
        if (numberGrounded == 0)
        {
            #if DebugOutput
            Debug.Log("Exited: no ground raycasts hit");
            #endif
            check = false;
        }
        else
        {
            if (groundedAveragePos.magnitude < 0.33f)
            {
                #if DebugOutput
                Debug.Log("Okay to enter/stay: stable");
                #endif
                check = true;
            }
            else if (groundedAveragePos.magnitude < 0.66f)
            {
                Vector2 projectedCompound = Matho.StdProj2D(compoundVelocity);
                if (Matho.AngleBetween(groundedAveragePos, projectedCompound) > 135f)
                {
                    #if DebugOutput
                    Debug.Log("Exited: controller stable but velocity points towards edge");
                    #endif
                    check = false;
                }
                else
                {
                    #if DebugOutput
                    Debug.Log("Okay to enter/stay: unstable but velocity points away from edge");
                    #endif
                    check = true;
                }
            }
            else
            {
                #if DebugOutput
                Debug.Log("Exited: controller unstable, redirecting velocity towards edge");
                #endif

                check = false;
                
                if (grounded)
                {
                    Vector2 edgeDirection = -groundedAveragePos.normalized;
                    #if DebugOutput
                    Debug.Log("Edge direction: " + edgeDirection);
                    #endif
                    RedirectExitVelocity(edgeDirection);
                }
            }
        }
        return check;
    }

    /*
    * Adjust dynamic velocity and constant velocity to be facing towards the edge direction on 
    * leaving the ground. This way the character is guarenteed to have velocity facing the edge of
    * the ground upon leaving it (not parallel to it).
    */
    private void RedirectExitVelocity(Vector2 edgeDirection)
    {
        float minExitMagnitude = 1;
        constantVelocity =
            minExitMagnitude * new Vector3(edgeDirection.x, 0, edgeDirection.y);
        
        dynamicVelocity =
            minExitMagnitude * new Vector3(edgeDirection.x, 0, edgeDirection.y);

        #if DebugOutput
        Debug.Log("Redirected constant: " + constantVelocity);
        Debug.Log("Redirected dynamic: " + dynamicVelocity);
        #endif
    }

    // Casts rays below the character, changing length based on the current ground normal
    // Adds constant length of multiple of step size of controller.
    private (int, float, Vector2) PercentageOnGround()
    {
        // outer circle
        int outerCastCount = 8;
        int outerCount;
        Vector2 outerWeight;
        (outerCount, outerWeight) = RaycastCircle(outerCastCount, controller.radius);

        // inner circle
        int innerCastCount = 4;
        int innerCount;
        Vector2 innerWeight;
        (innerCount, innerWeight) = RaycastCircle(innerCastCount, controller.radius * 0.5f);

        int castsHit = outerCount + innerCount;
        Vector2 hitWeight = Vector2.zero;
        if (castsHit != 0)
        {
            hitWeight = (outerWeight + innerWeight) / (castsHit);
            hitWeight *= 1.0f / controller.radius;
        }

        float percentageOnGround = 
            (outerCount + innerCount) / ((float) outerCastCount + innerCastCount);
        return (outerCount + innerCount, percentageOnGround, hitWeight);
    }

    // Returns
    // (int : number of casts hit), (Vector2 : position of casts hit summed)
    private (int, Vector2) RaycastCircle(int n, float radius)
    {
        int castsHit = 0;
        Vector2 hitSum = Vector3.zero;
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
            {
                castsHit++;
                hitSum += horizontalOffset;
            }
            // Visualize cast
            Debug.DrawLine(start, start + Vector3.down * castLength, Color.black, 0.5f);
        }
        
        return (castsHit, hitSum);
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
        dynamicAirVelocity += gravityStrength * Time.deltaTime * Vector3.down;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        UpdateGroundInfo(hit);
        if (considerDynamicCollisions)
        {
            if (grounded)
            {
                HandleVelocityCollisions(hit, ref dynamicVelocity);
            }
            else
            {
                HandleVelocityCollisions(hit, ref dynamicAirVelocity);
                HandleVelocityCollisions(hit, ref constantAirVelocity);
                #if DebugOutput
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.black, 5f);
                #endif

                // Needed to get rid of glancing jump boost issue. See Jump Boost Constant Velocity Diagram (7.13.21)
                if (HorizontalOnExit.Value && constantAirVelocity.y > 0)
                    constantAirVelocity.y = 0;
            }
        }
    }

    private void UpdateGroundInfo(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) < groundSlopeLimit + groundSlopeThreshold)
        {
            groundNormal = hit.normal;

            if (!grounded && ClampCheck() &&
                GroundCheck(dynamicAirVelocity + constantAirVelocity, true))
            {
                #if DebugOutput
                Debug.Log("entered");
                #endif
                grounded = true;

                dynamicVelocity = dynamicAirVelocity;
                constantVelocity = Vector3.zero;

                dynamicAirVelocity = Vector3.zero;
                constantAirVelocity = Vector3.zero;
            }
        }
    }

    private void CheckForGroundExit()
    {
        if (!groundCheck)
        {
            #if DebugOutput
            Debug.Log("exited");
            #endif
            grounded = false;

            //exitNormalQueue.Clear();

            dynamicAirVelocity = dynamicVelocity; 

            if (constantVelocity.magnitude > MaxConstantOnExit.Value)
            {
                constantVelocity = constantVelocity.normalized * MaxConstantOnExit.Value;
            }

            if (HorizontalOnExit.Value)
            {
                constantAirVelocity = Matho.StdProj3D(constantVelocity);
            }
            else
            {
                constantAirVelocity = constantVelocity;
            }

            dynamicVelocity = Vector3.zero;
            constantVelocity = Vector3.zero;
        }
    }

    // Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    // issue with grounded checks (called often and dynamic velocity approaches zero quickly)
    private void HandleVelocityCollisions(ControllerColliderHit hit, ref Vector3 velocityPartition)
    {
        Vector3 n = hit.normal;
        Vector3 v = velocityPartition.normalized;

        float velocityTheta = Matho.AngleBetween(n, v);

        if (velocityTheta >= 90)
        {
            Vector3 m = -1 * n;
            Vector3 nPerp = velocityPartition - Matho.Project(velocityPartition, m);
            velocityPartition = nPerp;
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
