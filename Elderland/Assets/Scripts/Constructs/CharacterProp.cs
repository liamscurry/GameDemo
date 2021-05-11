using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

// A helper script that simulates prop movement locally on a character.
// This can remove clipping that occurs when a prop is a child of a moving mesh.
public class CharacterProp : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    // A list of transforms that are not along the arch of the axis the prop is attached to.
    // Example: a backpack on a character has tilt transforms of the two shoulder bones.
    // This prop should be a child of a rotating joint bone (ex spine)
    [SerializeField]
    private Transform[] tiltTransforms;
    [SerializeField]
    private Vector3 tiltDirection;

    private Quaternion startRot;

    [SerializeField]
    [HideInInspector]
    private Quaternion colliderRot;

    private void Start()
    {
        startRot = transform.localRotation;
        ReadColliderRot();
    }

    private void LateUpdate()
    {
        transform.localRotation = startRot;
        UpdateColliderRot();
        transform.rotation = colliderRot * transform.rotation;
    }

    /*
    * Prints the locations of the collider transforms. This is used to set the initial positions via
    * inspector. SerializedFields and marking scene dirty was not working.
    * Step 1: Use this context menu call.
    * Step 2: copy and paste the corresponding positions to the start collider pos array in the
    * inspector.
    * Future implementation: make a context menu that stores this information in a file.
    * Then in the start method this class will read from the file to initialize the start offsets.
    */
    [ContextMenu("PrintColliderRot")]
    public void PrintColliderRot()
    {
        Vector3 tiltSum = GenerateNetTiltDir();
        Vector3 localTiltSum =
            transform.parent.worldToLocalMatrix.MultiplyPoint(tiltSum + transform.parent.position);
        Debug.Log("Tilt Sum Dir : " + localTiltSum.x +", " + localTiltSum.y + ", " + localTiltSum.z);
    }

    /*
    * Reads the beginning local offset of the collider transforms from this transform.
    */
    public void ReadColliderRot()
    {
        colliderRot = Quaternion.identity;
    }

    /*
    * Adds a delta rotation based on the directional offset from each tilt transform to this transform
    * in degrees rotated about this center.
    */
    private void UpdateColliderRot()
    {
        colliderRot = Quaternion.identity;
        Vector3 tiltSum = GenerateNetTiltDir();
        colliderRot *= GenerateTilt(tiltSum, tiltDirection, Axis.Z);
        colliderRot *= GenerateTilt(tiltSum, tiltDirection, Axis.Y);
    }

    private Vector3 GenerateNetTiltDir()
    {
        Vector3 tiltSum = Vector3.zero;
        for (int i = 1; i < tiltTransforms.Length; i++)
        { 
            tiltSum += tiltTransforms[i].position - tiltTransforms[0].position;
        }
        tiltSum *= 1 / (tiltTransforms.Length - 1);
        return tiltSum;
    }

    // Generates a rotation about the parent transform only considering one local axis of the parent
    // of this gameObject's transform.
    private Quaternion GenerateTilt(Vector3 tiltDirection, Vector3 startTiltDir, Axis axis)
    {
        Vector3 yPlaneProjection =
            transform.parent.worldToLocalMatrix.MultiplyPoint(tiltDirection + transform.parent.position);
        if (axis == Axis.X)
            yPlaneProjection.x = 0;
        if (axis == Axis.Y)
            yPlaneProjection.y = 0;
        if (axis == Axis.Z)
            yPlaneProjection.z = 0;
        
        yPlaneProjection = transform.parent.localToWorldMatrix.MultiplyPoint(yPlaneProjection);
        Vector3 projectedTiltDir = startTiltDir;
        if (axis == Axis.X)
            projectedTiltDir.x = 0;
        if (axis == Axis.Y)
            projectedTiltDir.y = 0;
        if (axis == Axis.Z)
            projectedTiltDir.z = 0;
        Vector3 worldTiltDirection = transform.parent.localToWorldMatrix.MultiplyPoint(projectedTiltDir);
        
        return Quaternion.FromToRotation(
                worldTiltDirection - transform.parent.position,
                yPlaneProjection - transform.parent.position);
    }
}
