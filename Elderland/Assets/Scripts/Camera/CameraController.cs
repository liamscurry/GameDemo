using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Responsible for moving the camera during normal gameplay and in cutscenes using target objects.

public class CameraController : MonoBehaviour
{
    public enum State { Gameplay, Cutscene, Idle }

    //Fields//
    [Header("Default Gameplay Settings")]
    [SerializeField]
    private float defaultSpeed;
    [SerializeField]
    private float defaultZoom;
    [SerializeField]
    private float defaultLinearMultiplier;
    [SerializeField]
    private Vector3 defaultDirection;

    [SerializeField]
    private float defaultSpeedGradation;
    [SerializeField]
    private float defaultZoomGradation;
    [SerializeField]
    private float defaultLinearMultiplierGradation;
    [SerializeField]
    private float defaultDirectionGradation;

    private State state;
    private CameraCutscene cutscene;
   
    //Property assignment backings
    private Vector3 direction;
    private Vector3 targetDirection;

    //Smooth damp references
    private float speedVelocity;
    private float zoomVelocity;
    private float linearMultiplerVelocity;
    private float directionVelocity;
    private Vector3 positionVelocity;

    private float orientationModifier;

    //Properties//
    //Gameplay data
    public float Speed { get; private set; }
    public float Zoom { get; private set; }
    public float LinearMultiplier { get; private set; }
    public Vector3 Direction { get { return direction; } private set { direction = value.normalized; } }

    //Gameplay settings
    public float TargetSpeed { get; set; }
    public float TargetZoom { get; set; }
    public float TargetLinearMultiplier { get; set; }
    public Vector3 TargetDirection { get { return targetDirection; } set { targetDirection = value.normalized; } }

    //Camera interactions
    public CameraArea Area { get; set; }
    public CameraEffector Effector { get; set; }
    public CameraTransition Transition { get; set; }

    //Gameplay settings' gradations
    public float SpeedGradation { get; set; }
    public float ZoomGradation { get; set; }
    public float LinearMultiplierGradation { get; set; }
    public float DirectionGradation { get; set; }

    public bool Gradations { get; set; }

    public float HorizontalAngle { get; set; }
    public float VerticalAngle { get; set; }
    public float HorizontalOffset { get; set; }

    //Target information
    public Transform FollowTarget { get; set; }
    public Transform SecondaryTarget { get; set; }
    public Camera Camera { get; private set; }
    public float MaxRadius { get; private set; }
    public float Radius { get; private set; }

    public bool AllowZoom { get; set; }
    
    public float OrientationModifier 
    { 
        get { return orientationModifier; } 
        set { orientationModifier = Mathf.Clamp01(value); } 
    }

    public State CameraState { get { return state; } }

    float targetRadius;
    float radiusVelocity;
    float radiusSpeedGradation;

    float fov;
    float targetFov;
    float fovVelocity;
    float fovSpeedGradation;

    float zoomModifier;

    float sprintTimer;
    float sprintPercentage;
    float orientationDelta;
    float orientationPercentage;
    float orientationTimer;

    private void Start()
    {   
        //StartGameplay();
        //GameInfo.Manager.UnfreezeInput(null);
        StartIdle();

        //Default values
        DefaultGameplaySettings();

        Speed = defaultSpeed;
        Zoom = defaultZoom;
        LinearMultiplier = defaultLinearMultiplier;
        Direction = defaultDirection;

        //55
        VerticalAngle = 90;
        HorizontalAngle = 20 - 90f;

        Gradations = true;

        MaxRadius = 2.75f;
        Radius = MaxRadius;
        targetRadius = MaxRadius;
        radiusSpeedGradation = .28f;

        AllowZoom = true;
        OrientationModifier = 1;

        HorizontalOffset = 20;

        fov = 60;
        targetFov = 60;
        fovSpeedGradation = .125f;

        zoomModifier = 1;

        Camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        switch (state)
        {
            case State.Gameplay:
                Gameplay();
                break;
            case State.Cutscene:
                Cutscene();
                break;
        }
    }

    public void DefaultGameplaySettings()
    {
        TargetSpeed = defaultSpeed;
        TargetZoom = defaultZoom;
        TargetLinearMultiplier = defaultLinearMultiplier;
        TargetDirection = defaultDirection;

        SpeedGradation = defaultSpeedGradation;
        ZoomGradation = defaultZoomGradation;
        LinearMultiplierGradation = defaultLinearMultiplierGradation;
        DirectionGradation = defaultDirectionGradation;
    }

    public void StartGameplay()
    {
        state = State.Gameplay;
        FollowTarget = PlayerInfo.Player.transform;
        sprintTimer = 0;
        sprintPercentage = 0;
        orientationPercentage = 0;
    }

    public void StartCutscene(CameraCutscene cameraCutscene)
    {
        state = State.Cutscene;
        cutscene = cameraCutscene;
        cutscene.Start();
        targetFov = 60;
    }

    public void StartIdle()
    {
        state = State.Idle;
    }

    public Vector2 StandardToCameraDirection(Vector2 v)
    {
        Vector2 vertical = Matho.StandardProjection2D(direction).normalized;
        Vector2 horizontal = Matho.Rotate(vertical, 90);
        Vector2 w = v.y * vertical + v.x * horizontal;
        return w;
    }

    //Moves the camera towards specific positions based on the camera's primary and secondary targets. Uses gameplay settings.
    private void Gameplay()
    {
        if (FollowTarget != null)
        {     
            UpdateGameplaySettings();
            AdjustZoom();
            AdjustOrientation();

            if (SecondaryTarget == null)
            {
                HorizontalOffset = 20;
                MaxRadius = 2.75f;
                transform.rotation = GenerateRotation();
                UpdateSprintTimer();
                transform.position =
                    GeneratePosition(FollowTarget.transform.position + GenerateSprintOffset());
            }
            else
            {
                HorizontalOffset = 5;
                MaxRadius = 4.25f;
                transform.rotation = GenerateRotation();
                transform.position = GeneratePosition(FollowTarget.transform.position);
            }
        }
    }

    private void AdjustZoom()
    {
        if (Input.GetAxis("Left Trigger") != 0 && AllowZoom && PlayerInfo.AbilityManager.RangedAvailable)
        {
            targetFov = 40;
            zoomModifier = .4f;
        }
        else
        {
            if (PlayerInfo.StatsManager.Sprinting)
            {
                targetFov = 60;
                zoomModifier = 1;
            }
            else
            {
                targetFov = 60;
                zoomModifier = 1;
            }
        }
    }   

    private void UpdateSprintTimer()
    {
        if (PlayerInfo.StatsManager.Sprinting
            && PlayerInfo.PhysicsSystem.TouchingFloor &&
            Matho.AngleBetween(PlayerInfo.PhysicsSystem.Normal, Vector3.up) < 15f)
        {
            sprintTimer += Time.deltaTime;
            sprintPercentage += Time.deltaTime;
            if (sprintPercentage > 1)
                sprintPercentage = 1;
            
            if (orientationDelta != 0)
            {
                sprintPercentage -= 2 * Time.deltaTime;
                if (sprintPercentage < 0)
                    sprintPercentage = 0;
            }
        }
        else
        {
            sprintPercentage -= Time.deltaTime;
            if (sprintPercentage < 0)
                sprintPercentage = 0;
        }
    }

    private Vector3 GenerateSprintOffset()
    {
        float periodScalerVertical = Mathf.Sin(Mathf.Cos(sprintTimer * 1));
        float verticalOffset = -0.0009f * Mathf.Pow((Mathf.Sin(sprintTimer * 22 + periodScalerVertical) - 1), 5);
        float horizontalOffset = 0.5f * 0.05f * Mathf.Sin(sprintTimer * 12);
        //Debug.Log(Matho.AngleBetween(PlayerInfo.MovementManager.CurrentDirection, PlayerInfo.MovementManager.TargetDirection));
        return (verticalOffset * transform.up + horizontalOffset * transform.right) * sprintPercentage * 0.75f;
    }

    private void AdjustOrientation()
    {
        HorizontalAngle -= GameInfo.Settings.RightDirectionalInput.x * 125 * zoomModifier * OrientationModifier * Time.deltaTime;
        orientationDelta = Mathf.Sign(GameInfo.Settings.RightDirectionalInput.x);
        if (GameInfo.Settings.RightDirectionalInput.magnitude < 0.25f)
            orientationDelta = 0;

        if (orientationDelta != 0)
        {
            orientationTimer += Time.deltaTime;
        }
        else
        {
            orientationTimer = 0;
        }
        
        if (orientationDelta != 0 &&
            PlayerInfo.StatsManager.Sprinting &&
            orientationTimer > 0.35f &&
            Matho.AngleBetween(GameInfo.Settings.LeftDirectionalInput, Vector2.up) < 45f)
        {
            orientationPercentage = Mathf.MoveTowards(orientationPercentage, orientationDelta, 1.8f * Time.deltaTime);
        }
        else
        {
            orientationPercentage = Mathf.MoveTowards(orientationPercentage, 0, 3 * Time.deltaTime);
        }
        

        if (GameInfo.Settings.RightDirectionalInput.magnitude != 0)
        {
            HorizontalAngle -= GameInfo.Settings.RightDirectionalInput.x * 125 * zoomModifier * OrientationModifier * Time.deltaTime;
            VerticalAngle += GameInfo.Settings.RightDirectionalInput.y * 125 * zoomModifier * OrientationModifier * Time.deltaTime;
            VerticalAngle = Mathf.Clamp(VerticalAngle, 45, 135);
        }
    }

    public Vector3 GeneratePosition(Vector3 targetPosition)
    {
        Vector3 offset = Vector3.zero;
        //rot error here
        offset += Matho.SphericalToCartesianX(MaxRadius, HorizontalAngle, 90f);
        offset += Matho.SphericalToCartesianX(MaxRadius * 1f, HorizontalAngle - HorizontalOffset, VerticalAngle).y * Vector3.up;
        Vector3 pivotOffset = 0.8f * Vector3.up;
        
        RaycastHit pivotOffsetHit;
        if (Physics.SphereCast(targetPosition, 0.5f * 0.9f, pivotOffset.normalized, out pivotOffsetHit, pivotOffset.magnitude, LayerConstants.GroundCollision | LayerConstants.Destructable))
        {
            targetPosition += pivotOffsetHit.distance * pivotOffset.normalized;
        }
        else
        {
            targetPosition += pivotOffset;
        }

        RaycastHit offsetHit;
        if (Physics.SphereCast(targetPosition, 0.5f * 0.75f, offset.normalized, out offsetHit, offset.magnitude, LayerConstants.GroundCollision | LayerConstants.Destructable))
        {
            targetRadius = offsetHit.distance;
            targetPosition += Radius * offset.normalized;
        }
        else
        {
            targetRadius = offset.magnitude;
            targetPosition += Radius * offset.normalized;
        }

        return targetPosition;
    } 

    public Quaternion GenerateRotation()
    {
        //Rotation assignment
        Vector3 rotationDirection = Matho.SphericalToCartesianX(1, HorizontalAngle - HorizontalOffset, VerticalAngle + 180);
        Direction = rotationDirection;
        
        Quaternion sprintTilt = Quaternion.Euler(0,0, -3 * orientationPercentage);

        Quaternion q = Quaternion.LookRotation(rotationDirection) * sprintTilt;

        //Debug.Log(HorizontalAngle);
        return q;
    }

    public void SetDirection(Transform rotationTransform)
    {
        SetDirection(rotationTransform.rotation);
    }

    public void SetDirection(Quaternion rotation)
    {
        Vector3 eulerAngles = rotation.eulerAngles;
        Vector3 direction = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one).MultiplyPoint(Vector3.forward);
        Direction = direction;
        //0 = z forward
        //-90 = z forward
        //-90 - angle = horizontal angle - horizontal offset;
        //-90 - angle + horizontaloffset = horizontal angle
        HorizontalAngle = -90 - eulerAngles.y + HorizontalOffset;
        VerticalAngle = eulerAngles.z + 90;
        //Quaternion.Euler(0, rotationAngles.y, rotationAngles.z)
        transform.rotation = GenerateRotation();
        transform.position = GeneratePosition(FollowTarget.transform.position);
    }

    private Vector3 GenerateAverageEnemyPosition()
    {
        /* 
        Vector3 sumPosition = Vector3.zero;

        foreach (GameObject enemy in PlayerInfo.FightingSensor.Enemies)
        {
            sumPosition += enemy.transform.position;
        }

        return sumPosition / PlayerInfo.FightingSensor.Enemies.Count;
        */
        return Vector3.zero;
    }

    //Moves camera towards and interpolates camera settings based on the next and current waypoint.
    private void Cutscene()
    {      
        UpdateCutsceneSettings();

        cutscene.Update();
        float waypointTime = cutscene.CurrentWaypointNode.Value.time;
        float lerpTime = (cutscene.CurrentWaypointNode.Value.jumpCut) ? 1 : cutscene.Timer / waypointTime;
        float x = Mathf.SmoothStep(cutscene.CurrentWaypointNode.Value.Position.x, cutscene.TargetWaypointNode.Value.Position.x, lerpTime);
        float y = Mathf.SmoothStep(cutscene.CurrentWaypointNode.Value.Position.y, cutscene.TargetWaypointNode.Value.Position.y, lerpTime);
        float z = Mathf.SmoothStep(cutscene.CurrentWaypointNode.Value.Position.z, cutscene.TargetWaypointNode.Value.Position.z, lerpTime);
        transform.position = new Vector3(x, y, z);

        transform.rotation = Quaternion.Slerp(cutscene.CurrentWaypointNode.Value.Rotation, cutscene.TargetWaypointNode.Value.Rotation, lerpTime);
    }

    private void UpdateGameplaySettings()
    {
        if (Gradations)
        {
            Speed = Mathf.SmoothDamp(Speed, TargetSpeed, ref speedVelocity, SpeedGradation, 100f);
            Zoom = Mathf.SmoothDamp(Zoom, TargetZoom, ref zoomVelocity, ZoomGradation, 100f);
            LinearMultiplier = Mathf.SmoothDamp(LinearMultiplier, TargetLinearMultiplier, ref linearMultiplerVelocity, LinearMultiplierGradation, 100f);

            float currentTheta = Matho.AngleBetween(Direction, TargetDirection);
            float newTheta = Mathf.SmoothDamp(currentTheta, 0, ref directionVelocity, DirectionGradation, 100f);
            float deltaTheta = currentTheta - newTheta;

            fov = Mathf.SmoothDamp(fov, targetFov, ref fovVelocity, fovSpeedGradation, 100f);
        }
        else
        {
            Speed = TargetSpeed;
            Zoom = TargetZoom;
            LinearMultiplier = TargetLinearMultiplier;
            //Direction = TargetDirection;
        }

        if (targetRadius > Radius)
        {
            Radius = Mathf.SmoothDamp(Radius, targetRadius, ref radiusVelocity, radiusSpeedGradation, 100f);
        }
        else
        {
            Radius = targetRadius;
        }

        Camera.fieldOfView = fov;
    }

    private void UpdateCutsceneSettings()
    {
        fov = Mathf.SmoothDamp(fov, targetFov, ref fovVelocity, fovSpeedGradation, 100f);
        Camera.fieldOfView = fov;
    }

    public void ForceRadius()
    {
        Radius = targetRadius;
    }

    private Vector3 FollowPosition(Vector3 targetPosition)
    {
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, Speed, 100, Time.deltaTime);
        return newPosition;
    }
}