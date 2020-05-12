using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToTuple : MonoBehaviour
{
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float rotationSpeed;

    public Transform TargetTransform { get { return targetTransform; } }
    public float Speed { get { return speed; } }
    public float RotationSpeed { get { return rotationSpeed; } }
}
