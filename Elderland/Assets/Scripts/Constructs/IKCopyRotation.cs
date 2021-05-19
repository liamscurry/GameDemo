﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Tool used in IKSystems to align a deform bone with a control bone.
* Ex: a twist bone sets its direction in line with its target and 
* sets its rotation along that axis based on its own rotation and the targets rotation
* along the same axis.
* Requires an fbx model/IKSystem.
*/
public class IKCopyRotation : MonoBehaviour
{
    private enum CopyType { Forward, Up, Right }

    // Bone used to make sure rotations don't converge to one value.
    // This is the bone that follows the target bone.
    // Should be an empty transform that is a sibling of this gameObject.
    // This bone should rotated in its local space around its local y axis so that the bone
    // is not twisted at the joint. The rest should be zero rotation and position locally.
    // This can be paired with the systems flipPole option.
    [SerializeField]
    private GameObject fromBone;
    // Twist bones this object is attached to a linear transform hierarchy
    // including the end transform (such as the hand or foot).
    [SerializeField]
    private GameObject[] twistBones; 
    // Higher weights means more twist at that bone (includes the second to last bone. So if there
    // are three bones, there are two weights).
    [SerializeField]
    private float[] weights; 
    [SerializeField]
    private CopyType copyType;

    private float targetPercentage;
    private float[] twistPercentages;
    private Quaternion currentRotation;

    private Vector3 Direction
    {
        get
        {
            switch(copyType)
            {
                case CopyType.Forward:
                    return fromBone.transform.forward;
                case CopyType.Up:
                    return fromBone.transform.up;
                case CopyType.Right:
                    return fromBone.transform.right;
                default:
                    throw new System.Exception("Must set copy type to forward, up or right.");
            }
        }
    }

    private Vector3 TargetDirection
    {
        get
        {
            switch(copyType)
            {
                case CopyType.Forward:
                    return twistBones[twistBones.Length - 2].transform.forward;
                case CopyType.Up:
                    return twistBones[twistBones.Length - 2].transform.up;
                case CopyType.Right:
                    return twistBones[twistBones.Length - 2].transform.right;
                default:
                    throw new System.Exception("Must set copy type to forward, up or right.");
            }
        }
    }

    public void InitializeTwist()
    {
        GeneratePercentages();
    }

    public void UpdateTwist()
    {
        Track();
        //Rotate();
    }

    // Tested initially, passed.
    /*
    * Needed to apply weights in rotate method.
    */
    private void GeneratePercentages()
    {
        targetPercentage = 0;

        twistPercentages = new float[twistBones.Length - 1];
        for (int i = 0; i < twistPercentages.Length; i++)
        {
            twistPercentages[i] = 
                (twistBones[i + 1].transform.position - twistBones[i].transform.position).magnitude *
                weights[i];
            targetPercentage += twistPercentages[i];
        }

        for (int i = 0; i < twistPercentages.Length; i++)
        {
            twistPercentages[i] = 
                twistPercentages[i] / targetPercentage;
        }
        targetPercentage = 1;
    }

    // Tested initially, passed.
    private void Track()
    {
        currentRotation = 
            Quaternion.FromToRotation(Direction, TargetDirection) *
            fromBone.transform.rotation;
    }

    private void Rotate()
    {
        Quaternion targetR = twistBones[twistBones.Length - 2].transform.rotation;
        float percentageUsed = 0;

        for (int i = 0; i < twistPercentages.Length; i++)
        {
            if (i == twistPercentages.Length - 1)
            {
                // last bone
                //twistBones[i].transform.rotation = targetR;
            }
            else
            {
                float percentage = 
                    percentageUsed + twistPercentages[i];
                twistBones[i].transform.rotation =
                    Quaternion.Lerp(currentRotation, targetR, percentage);

                percentageUsed += twistPercentages[i];
            }
        }
    }
}
