using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Tool used in IKSystems to align a deform bone with a control bone.
* Ex: a twist bone sets its direction in line with its target and 
* sets its rotation along that axis based on its own rotation and the targets rotation
* along the same axis.
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
    [SerializeField]
    private GameObject fromBone;
    // Twist bones this object is attached to and linear gameobject hierarchy
    // including the end transform.
    [SerializeField]
    private GameObject[] twistBones; 
    [SerializeField]
    private float[] weights; 
    [SerializeField]
    private CopyType copyType;
    [SerializeField]
    private float twist;

    private float targetPercentage;
    private float[] twistPercentages;

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

    private void Update()
    {
        Track();
        ForwardRotate();
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
        fromBone.transform.rotation = 
            Quaternion.FromToRotation(Direction, TargetDirection) *
            fromBone.transform.rotation;
    }

    private void ForwardRotate()
    {
        Quaternion targetR = target.transform.rotation;
        Quaternion currentR = fromBone.transform.rotation;
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
                    Quaternion.Lerp(currentR, targetR, percentage);

                percentageUsed += twistPercentages[i];
            }
        }
    }

    private void Rotate()
    {
        float targetEuler = target.transform.localRotation.eulerAngles.y + twist;
        float rootEuler = 0;//twistBones[0].transform.localRotation.eulerAngles.y
        float percentageLeft = 1;
        
        for (int i = twistPercentages.Length - 1; i >= 0; i--)
        {
            Vector3 currentEuler = twistBones[i].transform.localRotation.eulerAngles;
            
            if (i == twistPercentages.Length - 1)
            {
                // End bone
                twistBones[i].transform.localRotation = 
                    Quaternion.Euler(currentEuler.x, targetEuler, currentEuler.z);

                percentageLeft -= twistPercentages[i];
            }
            else if (i == 0)
            {
                // Root bone
                Vector3 childEuler = twistBones[i + 1].transform.localRotation.eulerAngles;

                twistBones[i].transform.localRotation = 
                    Quaternion.Euler(currentEuler.x, 0, currentEuler.z);

                float lerpEuler = 
                    Mathf.LerpAngle(rootEuler, targetEuler, percentageLeft); // want to rotate other way.
                twistBones[i].transform.localRotation = 
                    Quaternion.Euler(currentEuler.x, lerpEuler, currentEuler.z);

                // Rotate child with opposite operation
                childEuler = twistBones[i + 1].transform.localRotation.eulerAngles;
                twistBones[i + 1].transform.localRotation =
                    Quaternion.Euler(childEuler.x, childEuler.y - lerpEuler, childEuler.z);
            }
            else
            {
                Vector3 childEuler = twistBones[i + 1].transform.localRotation.eulerAngles;

                twistBones[i].transform.localRotation = 
                    Quaternion.Euler(currentEuler.x, 0, currentEuler.z);

                // Rotate child with opposite operation, THIS has to not be working, 120 to 120 and spinning 180 degrees.
                // all reading as same value in inspector (local all has 114 etc)
                //twistBones[i + 1].transform.localRotation =
                //    Quaternion.Euler(childEuler.x, childEuler.y + currentEuler.y, childEuler.z);
                // Now its only angle, 0, 0. No change in angle per bone.
                // won't work for last assignment on root bone, local is weird (parent is not in line)

                float lerpEuler = 
                    Mathf.LerpAngle(rootEuler, targetEuler, percentageLeft); // want to rotate other way.
                twistBones[i].transform.localRotation = 
                    Quaternion.Euler(currentEuler.x, lerpEuler, currentEuler.z);

                // Rotate child with opposite operation
                childEuler = twistBones[i + 1].transform.localRotation.eulerAngles;
                twistBones[i + 1].transform.localRotation =
                    Quaternion.Euler(childEuler.x, childEuler.y - lerpEuler, childEuler.z);

                percentageLeft -= twistPercentages[i];
            }
        }
    }
}
