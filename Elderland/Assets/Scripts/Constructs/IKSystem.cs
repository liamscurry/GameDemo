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
    // Empty object (Imported from Blender, called ### IK)
    [SerializeField]
    private Transform target;
    // Empty transform. (Imported from Blender, last bone in bones hierarchy (suffixed with _end))
    [SerializeField]
    private Transform footEnd;
    // Empty transform hierarchy. (Imported from Blender.)
    // Imported Rotation: Y facing towards next bone, Z facing behind limb, X facing to right of limb.
    [SerializeField]
    private Transform[] bones;
    // Any twist systems under this system.
    [SerializeField]
    private IKCopyRotation[] twistSystems;

    [Header("Pole")]
    // Empty transform. (Imported from Blender. Suffixed with Pole)
    // Should be in front of character rig/limb.
    [SerializeField]
    private Transform pole;
    // Pole axis may be flipped in a limb so that the start bone isn't twisted.
    [SerializeField]
    private bool flipPole;
    // Sync this field with test animations from blender so that the limb is in correct positions
    // in at least two places in the animation.
    [SerializeField]
    private float basePoleAngle; 

    [Header("Settings")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float ridgity;
    [SerializeField]
    private bool flipFoot;
    // Generally true for arms and false for legs.
    [SerializeField]
    private bool ignoreNormalFootRotation;

    private Vector3 spaceForward, spaceUp, spaceRight;
    private IKSolver.IKPackage iKPackage;

    private void Start()
    {     
        iKPackage = new IKSolver.IKPackage(bones.Length - 1);
        iKPackage.Transforms = bones;
        iKPackage.TargetTransform = target;
        iKPackage.PoleTransform = pole;
        iKPackage.FootEndTransform = footEnd;
        iKPackage.Ridgity = ridgity;
        iKPackage.BasePoleAngle = basePoleAngle;
        iKPackage.FlipFoot = flipFoot;
        iKPackage.ReverseRight = flipPole;
        iKPackage.IgnoreNormalFootRotation = ignoreNormalFootRotation;
        
        IKSolver.CalculatePoleSpace(iKPackage);
        IKSolver.InitializeTransformIKSolver(iKPackage);

        IKSolver.CalculatePoleSpace(iKPackage);
        IKSolver.TransformIKSolve(iKPackage);
    }

    private void LateUpdate()
    {
        IKSolver.CalculatePoleSpace(iKPackage);
        IKSolver.TransformIKSolve(iKPackage);

        foreach (IKCopyRotation twistSystem in twistSystems)
        {
            twistSystem.UpdateTwist();
        }
    }
}