using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* IK Monobehaviour that packages the IKSolver together to work automatically at runtime.
* Requires an fbx model.
*/
public class IKSystem : MonoBehaviour
{
    [Header("References")]
    // Empty transform (created manually). 
    // Child of this gameObject.
    // Rotation (in terms of first bone in hierarchy (such as hip or upperarm)):
    // Z up, Y behind limb and X is right of limb.
    [SerializeField]
    private Transform parent;
    // Empty transform (created manually).
    // Child of parent.
    // Rotation: any, as it will get overriden in IKSolver.
    [SerializeField]
    private Transform space;
    // Empty object (Imported from Blender, called ### IK)
    [SerializeField]
    private Transform target;
    // Empty transform. (Imported from Blender, last bone in bones hierarchy (suffixed with _end))
    [SerializeField]
    private Transform footEnd;
    // Empty transform hierarchy. (Imported from Blender.)
    // Imported Rotation: Y facing towards next bone, Z facing behind limb, X facing to right of limb.
    // If rotation is Z in front of limb and X to the right of the limb and Y in direction of next bone,
    // Check isPoleArm to true.
    [SerializeField]
    private Transform[] bones;
    // Any twist systems under this system.
    [SerializeField]
    private IKCopyRotation[] twistSystems;

    [Header("Pole")]
    // Empty transform. (Imported from Blender. Suffixed with Pole)
    [SerializeField]
    private Transform pole;
    // Needed so poleAngle is calculated based on pole correctly. See documentation above bones.
    [SerializeField]
    private bool isPoleArm; 
    [SerializeField]
    [HideInInspector]
    [Range(-90, 90)]
    private float poleAngle; // In system, poleAngle is calculated from the pole transform. Ignore.
    // Needed to make sure rest pose is facing correct direction (conversion from Blender to Unity).
    [SerializeField]
    private float poleScale;
    [SerializeField]
    private float basePoleAngle; 

    [Header("Settings")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float ridgity;
    // Should the last bone in the hierarchy copy the rotation of the IK target.
    // Generall true for arm limbs and false for leg limbs.
    [SerializeField]
    private bool endBoneFollowTarget;

    [HideInInspector]
    [SerializeField]
    private float maxX; // Not used in system. Ignore.

    // Fields
    private float[] transformLengths;
    private Vector3 startTargetPosition;
    private Quaternion startTargetRotation;
    private Quaternion startRootRotation;
    private float currentFootPercent;
    private Vector3 lastNormal;

    private Vector3 startPolePosition;
    private Quaternion startPoleRotation;

    private void Start()
    {     
        parent.transform.localRotation = 
            bones[0].localRotation * Quaternion.Euler(90, 0, 0);

        startPolePosition = pole.position;
        startPoleRotation = pole.rotation;

        IKSolver.CalculatePoleAngle(pole, basePoleAngle, poleScale, ref poleAngle, isPoleArm);
        IKSolver.InitializeTransformIKSolver(
                space,
                target,
                bones,
                ref transformLengths,
                ref startRootRotation,
                ref startTargetPosition,
                ref startTargetRotation,
                ref currentFootPercent,
                ref lastNormal);

        IKSolver.CalculatePoleAngle(pole, basePoleAngle, poleScale, ref poleAngle, isPoleArm);
        IKSolver.TransformIKSolve(
            parent,
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            basePoleAngle,
            maxX,
            ref currentFootPercent,
            ref lastNormal,
            true, 
            false,
            endBoneFollowTarget);
    }

    public void Reset()
    {
        pole.position = startPolePosition;
        pole.rotation = startPoleRotation;

        IKSolver.ResetTransformIKSolver(
            parent,
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            basePoleAngle,
            maxX,
            startRootRotation,
            startTargetPosition,
            startTargetRotation,
            ref currentFootPercent,
            ref lastNormal,
            true,
            false,
            endBoneFollowTarget);
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Reset();

        IKSolver.CalculatePoleAngle(pole, basePoleAngle, poleScale, ref poleAngle, isPoleArm);
        IKSolver.TransformIKSolve(
            parent,
            space,
            target,
            footEnd,
            bones,
            transformLengths,
            ridgity,
            poleAngle,
            basePoleAngle,
            maxX,
            ref currentFootPercent,
            ref lastNormal,
            true,
            false,
            endBoneFollowTarget);

        foreach (IKCopyRotation twistSystem in twistSystems)
        {
            twistSystem.UpdateTwist();
        }
    }
}