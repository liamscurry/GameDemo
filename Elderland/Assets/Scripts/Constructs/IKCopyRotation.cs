using System.Collections;
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

    [SerializeField]
    private GameObject target;
    // The next bone in the chain under the target bone.
    [SerializeField]
    private GameObject targetChild;
    // Bone used to make sure rotations don't converge to one value.
    // This is the bone that follows the target bone.
    // Should be an empty transform that is a sibling of this gameObject.
    // This bone should rotated in its local space around its local axis so that the bone
    // is not twisted at the joint. The rest should be zero rotation and position locally.
    // This can be paired with the systems flipPole option.
    [SerializeField]
    private GameObject fromBone;
    // Twist bones this object is attached to a linear transform hierarchy
    // including the end transform.
    [SerializeField]
    private GameObject[] twistBones; 
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
                    return target.transform.forward;
                case CopyType.Up:
                    return target.transform.up;
                case CopyType.Right:
                    return target.transform.right;
                default:
                    throw new System.Exception("Must set copy type to forward, up or right.");
            }
        }
    }

    private void Start()
    {
        GeneratePercentages();
    }

    public void UpdateTwist()
    {
        Track();
        Rotate();
    }

    // Tested initially, passed.
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
        Quaternion targetR = target.transform.rotation;
        float percentageUsed = 0;

        for (int i = 0; i < twistPercentages.Length; i++)
        {
            if (i == twistPercentages.Length - 1)
            {
                // last bone
                twistBones[i].transform.rotation = targetR;
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
