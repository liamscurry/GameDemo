using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* IK Monobehaviour that packages the IKSolver together to work automatically at runtime.
* Can turn off IK limiting for animations etc.
*/
public class IKSystem : MonoBehaviour
{
    /*
    * Hierarchy requirements:
    * Bones are a tree hierarchy with branching factor 1.
    * Bones starts at root bone (last to get effected) and goes to the foot bone.
    * The target transform is the IK transform. This should be a sibling of the bone hierarchy.
    * Space transform should also be a plain transform that is a sibling of the target transform.
    * All of these should be the child of this object (The one with the component on it).
    * Usually then the sibling of this object is the skinned mesh renderer and these two objects
    * are children of the mesh parent which has the animator on it.
    *
    * Pole angle is the angle of the limb in which way it is pivoting.
    * MaxX specified how far the target transform can be on its parent's local X axis.
    * Ridgity specifies how bendy the system is. Lower = more bendy.
    */
    [Header("References")]
    [SerializeField]
    private Transform space;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Transform footEnd;
    [SerializeField]
    private Transform[] bones;
    [Header("Settings")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float ridgity;
    [SerializeField]
    [Range(-90, 90)]
    private float poleAngle;
    [SerializeField]
    private float maxX;

    // Fields
    private float[] transformLengths;
    private Vector3 startTargetPosition;
    private Quaternion startRootRotation;
    private float currentFootPercent;
    private Vector3 lastNormal;

    private void Start()
    {
        IKSolver.InitializeTransformIKSolver(
                space,
                target,
                bones,
                ref transformLengths,
                ref startRootRotation,
                ref startTargetPosition,
                ref currentFootPercent,
                ref lastNormal);
        
        IKSolver.TransformIKSolve(
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            maxX,
            ref currentFootPercent,
            ref lastNormal);
    }

    public void Reset()
    {
        IKSolver.ResetTransformIKSolver(
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            maxX,
            startRootRotation,
            startTargetPosition,
            ref currentFootPercent,
            ref lastNormal);
    }

    private void LateUpdate()
    {
        IKSolver.TransformIKSolve(
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            maxX,
            ref currentFootPercent,
            ref lastNormal);
    }
}