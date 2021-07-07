using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Custom, simplistic physics for 3D application. 
//Allows for gameplay movement and semi-realistic movement simultaneously.
public class PhysicsSystem
{
    //Constants//
    public const float GravitationalConstant = 30f;
    public const float SurfaceOffset = 0.5f;

    private float surfaceDistance = 0.175f;

    //Fields//
    //Object references
    private GameObject parent;
    private CapsuleCollider capsule;
    public Rigidbody Body { get; set; }
    public Vector3 BottomSphereOffset { get; private set; }

    //Velocity partitions
    private Vector3 constantVelocity;
    private float slopeMagnitude;
    private Vector3 slopeDirection;
    private Vector3 dynamicVelocity;
    private Vector3 animationVelocity;

    //Qualities
    private float weight;
    private float weightProportion;

    //Frame specific information
    private Vector3 frameStartPosition;

    private bool animating;
    private bool clampWhileAnimating;

    //Properties//
    //General
    public Vector3 LastPosition { get; private set; }

    //Velocity partitions
    public Vector3 ConstantVelocity { get { return constantVelocity; } set { constantVelocity = value; } }
    public Vector3 DynamicVelocity { get { return dynamicVelocity; } }
    public Vector3 SlopeVelocity { get { return slopeMagnitude * slopeDirection; } }
    public Vector3 CalculatedVelocity { get { return DynamicVelocity + ConstantVelocity + SlopeVelocity; } }
    public Vector3 AnimationVelocity { get { return animationVelocity; } set { animationVelocity = value; } }

    public Vector3 LastCollisionVelocity { get; private set; }
    public Vector3 LastConstantVelocity { get; private set; }
    public Vector3 LastDynamicVelocity { get; private set; }
    public Vector3 LastCalculatedVelocity { get; private set; }
    public Vector3 LastAnimationVelocity { get; private set; }

    //Geometry information
    public bool TouchingFloor { get; private set; }
	public bool EnteredFloor { get; private set; }
	public bool ExitedFloor { get; private set; }
    public bool EnteredGround { get; private set; }
 
    public bool Hit { get; private set; }
    //public RaycastHit Raycast { get; private set; }
    public float Distance { get; private set; }
    public Vector3 Normal { get; private set; }
    public float Theta { get; private set; }
    public bool HitLedge { get; private set; }
    public bool InGeometry { get; private set; }
    public Vector3 AverageNormal { get; private set; }
    
    public bool LastInGeometry { get; private set; }

    public event System.EventHandler SlidIntoGround;
    public float SlopeStrength { get; set; }

    //Settings
    public float Weight { get { return weight; } set { weight = value; weightProportion = (1 / ((Weight * .1f) + .9f)); } }
    public float GroundDetectionDistance { get; set; }
    public float GravityStrength { get; set; }

    public bool Animating 
    { 
        get { return animating; }
        set 
        { 
            //Frozen toggled on
            if (!animating && value)
                Body.velocity = Vector3.zero;

            //Frozen toggled off
            if (animating && !value)
                TotalZero(true, true, true);

            animating = value; 
        } 
    }

    public bool ClampWhileAnimating { get { return clampWhileAnimating; } set { clampWhileAnimating = value; } }

    private bool overlapResetReady;

    public bool OverlappingGroundContact { get; private set; }
    private ContactPoint groundContact;
    private bool overlappingCapsuleContact;
    private ContactPoint capsuleContact;

    private bool colliding;
    private List<Collider> collisionColliders;

    private Vector3 lateFramePosition;
    private Vector3 late2FramePosition;
    public Vector3 ExitDirection { get; private set; }
    public bool LastExitedFloor { get; private set; }

    public PhysicsSystem(GameObject parent, CapsuleCollider capsule, Rigidbody body, float weight, float gravityStrength = GravitationalConstant)
    {
        this.parent = parent;
        this.capsule = capsule;
        this.Body = body;
        this.Weight = weight;

        GravityStrength = gravityStrength;
        BottomSphereOffset = capsule.BottomSphereOffset();

        overlapResetReady = true;
        AverageNormal = Vector3.up;

        SlopeStrength = 1;
        collisionColliders = new List<Collider>();

        lateFramePosition = parent.transform.position;
        late2FramePosition = parent.transform.position;
    }

    public virtual void UpdateSystem()
    {
        //Reset last frame information
        ConstantZero(true, true, true);
        animationVelocity = Vector3.zero;
        
        //Generate ground information
        float radius = capsule.radius - SurfaceOffset;
        Vector3 offset = (capsule.height / 2 - capsule.radius) * Vector3.up + radius * Vector3.down;
        Collider[] colliders = 
            Physics.OverlapCapsule(
                parent.transform.position + offset,
                parent.transform.position - offset,
                radius,
                LayerConstants.GroundCollision);

        InGeometry = colliders.Length != 0;

        if (TouchingFloor)
            UpdateGroundData();

        CheckGroundData();

        //Apply gravity
        if (!TouchingFloor)    
            Push(new Vector3(0, -GravityStrength * Time.deltaTime, 0));

        //Set frame information
        GroundDetectionDistance = 0;
        frameStartPosition = parent.transform.position;
        //Debug.Log("Update");
    }

	public virtual void LateUpdateSystem()
	{  
        if (ExitedFloor)
        {
            ExitDirection = lateFramePosition - late2FramePosition;
            //Debug.Log("new position " + parent.transform.position);
            //Debug.Log("calculated: " + ExitDirection.x + ", " + ExitDirection.y + "," + ExitDirection.z);
            //PlayerInfo.Manager.test = lateFramePosition + PlayerInfo.Capsule.BottomSphereOffset();
            //PlayerInfo.Manager.test2 = late2FramePosition + PlayerInfo.Capsule.BottomSphereOffset();
        }

        LastPosition = frameStartPosition;

        LastConstantVelocity = ConstantVelocity;
        LastDynamicVelocity = DynamicVelocity;
        LastCalculatedVelocity = CalculatedVelocity;
        LastAnimationVelocity = AnimationVelocity;

        LastExitedFloor = ExitedFloor;

        EnteredGround = false;
		EnteredFloor = false;
		ExitedFloor = false;
        HitLedge = false;
        LastInGeometry = InGeometry;

        //Debug.Log("lastPosition " + parent.transform.position);

        LimitConstantVelocity();
        //Debug.Log("Late Update");
	}

    public virtual void FixedUpdateSystem()
    {
        overlapResetReady = true;
        //Debug.Log("FixedUpdate");
        
        late2FramePosition = lateFramePosition;
        lateFramePosition = parent.transform.position;
    }

    protected virtual void OnSlidIntoGround(object obj)
    {
        SlidIntoGround.Invoke(obj, System.EventArgs.Empty);
    }

    public void ForceTouchingFloor()
    {
        EnteredGround = true;
        TouchingFloor = true;
        EnteredFloor = true;
            
        StandardGroundData();
    }

    //Drag methods. Apply drag or friction to physics. Called in fixed update.
    public void DynamicDragX(float strength)
    {
        float magnitude = Mathf.Abs(dynamicVelocity.x);
        magnitude -= strength * Time.deltaTime;
        if (magnitude < 0)
            magnitude = 0;
        
        dynamicVelocity.x = magnitude * Matho.Sign(dynamicVelocity.x);
    }

    public void DynamicDragY(float strength)
    {
        float magnitude = Mathf.Abs(dynamicVelocity.y);
        magnitude -= strength * Time.deltaTime;
        if (magnitude < 0)
            magnitude = 0;
        
        dynamicVelocity.y = magnitude * Matho.Sign(dynamicVelocity.y);
    }

    public void DynamicDragZ(float strength)
    {
        float magnitude = Mathf.Abs(dynamicVelocity.z);
        magnitude -= strength * Time.deltaTime;
        if (magnitude < 0)
            magnitude = 0;
        
        dynamicVelocity.z = magnitude * Matho.Sign(dynamicVelocity.z);
    }
    
    public void DynamicDrag(float strength)
    {
        float magnitude = dynamicVelocity.magnitude;
        magnitude -= strength * Time.deltaTime;
        if (magnitude < 0)
            magnitude = 0;
        
        dynamicVelocity = magnitude * dynamicVelocity.normalized;
    }

    //Zero methods. Sets specified components of partitions to zero.
    public void ConstantZero(bool x, bool y, bool z)
    {
        if (x)
            constantVelocity.x = 0;
        if (y)
            constantVelocity.y = 0;
        if (z)
            constantVelocity.z = 0;
    }

    public void DynamicZero(bool x, bool y, bool z)
    {
        if (x)
            dynamicVelocity.x = 0;
        if (y)
            dynamicVelocity.y = 0;
        if (z)
            dynamicVelocity.z = 0;
    }

    public void TotalZero(bool x, bool y, bool z)
    {
        if (x)
        {
            dynamicVelocity.x = 0;
            constantVelocity.x = 0;
        }

        if (y)
        {
            dynamicVelocity.y = 0;
            constantVelocity.y = 0;
        }

        if (z)
        {
            dynamicVelocity.z = 0;
            constantVelocity.z = 0;
        }
    }

    //Pushes physics body with gradual force, has friction.
    public void Push(Vector3 force)
    {
        dynamicVelocity.x += weightProportion * force.x;
        dynamicVelocity.y += weightProportion * force.y;
        dynamicVelocity.z += weightProportion * force.z;
    }

    public void ImmediatePush(Vector3 force)
    {
        dynamicVelocity.x += force.x;
        dynamicVelocity.y += force.y;
        dynamicVelocity.z += force.z;
    }

    //Collision handling for all custom physics objects. Redirects dynamic velocity when hitting walls.
    public static void HandleVelocityCollisions(PhysicsSystem physics, CapsuleCollider capsule, Vector3 capsulePosition, Collision other)
    {
        //Velocity adjustment
        RaycastHit netHitVelocity;
        Physics.CapsuleCast(
            capsulePosition + new Vector3(0, (capsule.height / 2) - capsule.radius, 0),
            capsulePosition - new Vector3(0, (capsule.height / 2) - capsule.radius, 0),
            capsule.radius - 0.05f,
            physics.CalculatedVelocity.normalized,
            out netHitVelocity,
            0.1f,
            LayerConstants.GroundCollision
        );

        if (netHitVelocity.collider != null && netHitVelocity.collider == other.collider)
        {
            Vector3 n = netHitVelocity.normal;
            Vector3 v = physics.DynamicVelocity.normalized;

            float velocityTheta = Matho.AngleBetween(n, v);

            if (velocityTheta >= 90)
            {
                Vector3 m = -1 * n;
                Vector3 nPerp = physics.DynamicVelocity - Matho.Project(physics.DynamicVelocity, m);
                physics.dynamicVelocity = 0.5f * nPerp;
            }
        }
    }

    public static void HandleGroundCollisions(PhysicsSystem physics, CapsuleCollider capsule, Vector3 capsulePosition, Collision other)
    {
        foreach (ContactPoint contactPoint in other.contacts)
        {
            Vector3 n = contactPoint.normal;
            Vector3 up = Vector3.up;

            float verticalTheta = Matho.AngleBetween(n, up);

            //if (!physics.TouchingFloor)
            //    Debug.Log(verticalTheta);

            // verticalTheta <= 45
            //contactPoint.point.y < (physics.capsule.transform.position + physics.BottomSphereOffset).y
            if (!physics.TouchingFloor && verticalTheta <= 45)
            {
                physics.EnteredGround = true;
                //Debug.Log("EnteredGround");
                break;
            }
        }
    }

    public static void HandleOverlapCollisions(PhysicsSystem physics, CapsuleCollider capsule, Vector3 capsulePosition, Collision other)
    {
        if (physics.overlapResetReady)
        {
            physics.OverlappingGroundContact = false;
            physics.overlappingCapsuleContact = false;
            physics.colliding = false;
            physics.overlapResetReady = false;
        }

        if (other != null)
        {
            foreach (ContactPoint contactPoint in other.contacts)
            {
                Vector3 n = contactPoint.normal;
                Vector3 up = Vector3.up;

                float verticalTheta = Matho.AngleBetween(n, up);

                //Ground contact
                if (verticalTheta >= 0 && verticalTheta <= 75)
                {
                    if (!physics.OverlappingGroundContact)
                    {
                        physics.OverlappingGroundContact = true;
                        physics.groundContact = contactPoint;
                    }
                    else
                    {
                        if (contactPoint.point.y > physics.groundContact.point.y)
                        {
                            physics.groundContact = contactPoint;
                        }
                    }
                }

                //Capsule contacts
                if (verticalTheta >= 0 && verticalTheta < 90f)
                {
                    if (!physics.overlappingCapsuleContact)
                    {
                        physics.overlappingCapsuleContact = true;
                        physics.capsuleContact = contactPoint;
                    }
                    else
                    {
                        if (contactPoint.point.y > physics.capsuleContact.point.y)
                        {
                            physics.capsuleContact = contactPoint;
                        }
                    }
                }
            }
        }

        //All contacts
        if (!physics.colliding)
        {
            physics.colliding = true;
            physics.collisionColliders = new List<Collider>();
        }

        if (other != null)
            physics.collisionColliders.Add(other.collider);
    }

    private void LimitConstantVelocity()
    {
        RaycastHit netHitVelocity;
        Physics.CapsuleCast(
            capsule.transform.position + new Vector3(0, (capsule.height / 2) - capsule.radius, 0),
            capsule.transform.position - new Vector3(0, (capsule.height / 2) - capsule.radius, 0),
            capsule.radius - 0.05f,
            CalculatedVelocity.normalized,
            out netHitVelocity,
            0.1f,
            LayerConstants.GroundCollision
        );

        if (netHitVelocity.collider != null && collisionColliders.Contains(netHitVelocity.collider))
        {
            Vector3 n = netHitVelocity.normal;
            Vector3 v = constantVelocity.normalized;

            float velocityTheta = Matho.AngleBetween(n, v);

            if (velocityTheta >= 90)
            {
                Vector3 m = -1 * n;
                Vector3 nPerp = constantVelocity - Matho.Project(constantVelocity, m);
                constantVelocity = 0.5f * nPerp;
            }
        }
    }

    private void UpdateGroundData()
    {   
        //Only update data if the player has moved during the last frame
        if (Vector3.Distance(LastPosition, parent.transform.position) != 0 && !InGeometry)
            StandardGroundData();

        //Only generate ledge data if standard is successful and if the object has moved during the last frame
        if (Hit && Theta <= 45)
        {
            float distance = Vector2.Distance(Matho.StdProj2D(LastPosition), Matho.StdProj2D(parent.transform.position));

            if (distance != 0 && !InGeometry && !LastInGeometry)
                LedgeGroundData();
        }
    }

    private void StandardGroundData()
    {
        RaycastHit raycast;

        bool hit = UnityEngine.Physics.SphereCast(
            parent.transform.position + BottomSphereOffset + capsule.radius * Vector3.up,
            capsule.radius,
            Vector3.down,
            out raycast,
            capsule.radius + SurfaceOffset + GroundDetectionDistance,
            LayerConstants.GroundCollision);

        Hit = hit;

        if (Hit)
        {
            Distance = raycast.distance;
            //Debug.Log(Distance + ", " + capsule.radius + ", " + (parent.transform.position + BottomSphereOffset + capsule.radius * Vector3.up).y);
            Normal = raycast.normal;
            Theta = Matho.AngleBetween(Normal, Vector3.up);

            //Checks to see if player is currently touching a wall and ground in order to get correct ground info.
            if (Theta > 75 && OverlappingGroundContact)
            {
                CalculateGroundInfoFromContact();
            }

            if (OverlappingGroundContact && overlappingCapsuleContact)
            {
                CalculateSlopeVelocity(Matho.AngleBetween(raycast.normal, Vector3.up));
            }
        }
    }

    private void CalculateGroundInfoFromContact()
    {
        //NEEDS TO BE FIXED
        Normal = groundContact.normal;
        Theta = Matho.AngleBetween(Normal, Vector3.up);

        Vector3 point = groundContact.point - groundContact.separation * groundContact.normal;
        float horizontalContactDistance = Vector3.Distance(Matho.StandardProjection3D(capsule.transform.position), Matho.StandardProjection3D(point));
        float horizontalSquareDifference = Mathf.Pow(capsule.radius, 2) - Mathf.Pow(horizontalContactDistance, 2);
        if (horizontalSquareDifference < 0)
            horizontalSquareDifference = 0;
        float bottomCapsuleContactCenter = Mathf.Sqrt(horizontalSquareDifference) + point.y;
        PlayerInfo.Manager.test = Matho.StandardProjection3D(PlayerInfo.Player.transform.position) + bottomCapsuleContactCenter * Vector3.up;
        Distance = (capsule.transform.position.y + BottomSphereOffset.y) - bottomCapsuleContactCenter + capsule.radius;

        //Ground contact offset adjustment
        float contactOffset = groundContact.otherCollider.contactOffset + groundContact.thisCollider.contactOffset;
        Distance -= (1 / Mathf.Cos(Theta * Mathf.Deg2Rad)) * (contactOffset / 2);
    }      

    private void CalculateSlopeVelocity(float slopeTheta)
    {   
        if (slopeTheta < 75f)
        {
            if (slopeTheta > 45)
            {
                slopeMagnitude += (Mathf.Pow(slopeTheta / 45, 1/3f) - 0.65f) * Mathf.Pow(SlopeStrength, 1.5f) * (1 / 130f);
            }
            else
            {
                slopeMagnitude = 0;
            }

            if (slopeTheta != 0)
            {
                Vector3 projectedNormal = Matho.StandardProjection3D(Normal);
                Vector3 crossedNormal = Vector3.Cross(projectedNormal, Normal);
                slopeDirection = Matho.Rotate(Normal, crossedNormal, -90);
            }
            else
            {
                if (slopeDirection.y != 0)
                {
                    //slopeDirection = Matho.StandardProjection3D(slopeDirection).normalized;
                    //OnSlidIntoGround(this);
                }
            }
        }
        else
        {
            float groundTheta = Matho.AngleBetween(groundContact.normal, Vector3.up);
            slopeMagnitude = (groundTheta < 45) ? (1.0f - (groundTheta / 100)) : (1.0f - (groundTheta / 75));
            Vector2 projectedGroundNormal = Matho.StdProj2D(capsuleContact.normal);
            slopeDirection =
                Matho.PlanarDirectionalDerivative(projectedGroundNormal, groundContact.normal).normalized;
        }
    }

    private void LedgeGroundData()
    {
        List<Vector3> startHits = new List<Vector3>();
        List<RaycastHit> mapHits = new List<RaycastHit>();

        GeneratePoints(ref startHits, ref mapHits);
        HitLedge = CheckPoints(startHits, mapHits);
        DrawLedgeData(ref startHits, ref mapHits);
    }

    private void GeneratePoints(ref List<Vector3> startHits, ref List<RaycastHit> mapHits)
    {
        RaycastHit mapCast;
        float phi = 90 - Theta;
        float slopeRadius = capsule.radius / Mathf.Sin(phi * Mathf.Deg2Rad);
        surfaceDistance = slopeRadius + SurfaceOffset + GroundDetectionDistance;

        //Add initial data point
        UnityEngine.Physics.Raycast(
            LastPosition + BottomSphereOffset,
            Vector3.down,
            out mapCast,
            surfaceDistance,
            LayerConstants.GroundCollision);

        startHits.Add(LastPosition + BottomSphereOffset);
        mapHits.Add(mapCast);

        //Add middle data points
        int count = 1;
        float interpolationDistance = 0.5f;
        float mapDistance = Vector3.Distance(parent.transform.position, LastPosition);
        while (mapDistance >= interpolationDistance)
        {
            Vector3 start = Vector3.MoveTowards(
                LastPosition + BottomSphereOffset,
                parent.transform.position + BottomSphereOffset,
                interpolationDistance * count);

            UnityEngine.Physics.Raycast(
                start,
                Vector3.down,
                out mapCast,
                surfaceDistance,
                LayerConstants.GroundCollision);

            startHits.Add(start);
            mapHits.Add(mapCast);

            mapDistance -= interpolationDistance;
            count++;
        }

        //Add last data point
        UnityEngine.Physics.Raycast(
            parent.transform.position + BottomSphereOffset,
            Vector3.down,
            out mapCast,
            surfaceDistance,
            LayerConstants.GroundCollision);
            
        startHits.Add(parent.transform.position + BottomSphereOffset);
        mapHits.Add(mapCast);
    }

    private bool CheckPoints(List<Vector3> startHits, List<RaycastHit> mapHits)
    {
        //Check data points for invalid formations
        for (int i = 1; i < mapHits.Count; i++)
        {
            //Ledge Case: A raycast didn't hit
            if (mapHits[i].collider == null && mapHits[i - 1].collider != null)
            {
                //Debug.Log("oh kay");
                //Time.timeScale = 0;
                return true;
            }
            else
            {
                //Ledge Case: mapped angle not valid ground
                Vector3 deltaDirection = (mapHits[i].point - mapHits[i - 1].point);

                if (deltaDirection.y < 0 && deltaDirection.y < -SurfaceOffset)
                {
                    float deltaHorizontal = Mathf.Sqrt(Mathf.Pow(deltaDirection.x, 2) + Mathf.Pow(deltaDirection.z, 2));
                    float deltaVertical = Mathf.Abs(deltaDirection.y);
                    float theta = Matho.Angle(new Vector2(deltaHorizontal, deltaVertical));

                    if (theta > 75)
                    {
                        Debug.Log("oh kay");
                        return true;
                    }
                    else
                    {
                        //Ledge Case: mapped angle not valid ground (height less than 0.5)
                        RaycastHit crossHit;
                        UnityEngine.Physics.Raycast(
                            mapHits[i].point + Vector3.up * (mapHits[i - 1].point.y- mapHits[i].point.y) / 2,
                            (startHits[i - 1] - startHits[i]).normalized,
                            out crossHit,
                            Vector3.Distance(startHits[i - 1], startHits[i]),
                            LayerConstants.GroundCollision);

                        if (crossHit.collider != null && Matho.AngleBetween(crossHit.normal, Vector3.up) > 75)
                        {
                            Debug.Log("oh kay");
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void CheckGroundData()
    {
        if (!TouchingFloor)
		{
            //Enter ground proc when colliding into ground geometry
            if (EnteredGround)
            {
                TouchingFloor = true;
                EnteredFloor = true;
                
                StandardGroundData();
            }
        }
        else
		{
            //Exit ground proc when clamping fails
            // || HitLedge
            if (!Hit || ((Distance - capsule.radius) > SurfaceOffset && Theta > 75) || HitLedge)
            {
                //Debug.Log(!Hit + ", " + (Distance > SurfaceOffset && Theta > 75) + ", " + HitLedge);
                //Debug.Log(Theta + ", " + Normal);
                //Debug.Log(Hit + ", " + (Theta > 45) + ", " + (Theta <= 75) + ", " + overlappingGroundContact);
                //Time.timeScale = 0;
                //Debug.Log("exited");
                ExitDirection = (parent.transform.position - lateFramePosition).normalized;
                TouchingFloor = false;
                ExitedFloor = true;
            }
        }
    }

    private void DrawLedgeData(ref List<Vector3> startHits, ref List<RaycastHit> mapHits)
    {
        for (int i = 0; i < startHits.Count - 1; i++)
        {
            if (mapHits[i].collider != null)
            {
                Debug.DrawLine(startHits[i], mapHits[i].point, Color.red, 1f);
            }
            else
            {
                Debug.DrawLine(startHits[i], startHits[i] + surfaceDistance * Vector3.down, Color.black, 5f);
            }
        }
    }
}