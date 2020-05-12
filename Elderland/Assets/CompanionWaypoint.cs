using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompanionWaypoint
{
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public float speed;
    [SerializeField]
    public float horizontalRotation;
    [SerializeField]
    public float verticalRotation;
    [SerializeField]
    public float tiltRotation;
    [SerializeField]
    public float rotationSpeed;
    [SerializeField]
    public float endWaitTime;
    
    public Quaternion rotation;

    public CompanionWaypoint(Vector3 position, float speed, float horizontalRotation, float verticalRotation, float tiltRotation, float rotationSpeed, float endWaitTime)
    {
        this.position = position;
        this.speed = speed;
        this.horizontalRotation = horizontalRotation;
        this.verticalRotation = verticalRotation;
        this.tiltRotation = tiltRotation;
        this.rotationSpeed = rotationSpeed;
        this.endWaitTime = endWaitTime;
    }
}
