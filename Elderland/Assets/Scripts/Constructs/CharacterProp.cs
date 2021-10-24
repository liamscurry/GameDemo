using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Todo: bone transforms from blender do not have their rotations set.

// A helper script that simulates prop movement locally on a character.
// This can remove clipping that occurs when a prop is a child of a moving mesh.
public class CharacterProp : MonoBehaviour
{
    public enum Axis { X, Y, Z, nX, nY, nZ }

    // A list of transforms that are not along the arch of the axis the prop is attached to.
    // Example: a backpack on a character has tilt transforms of the two shoulder bones.
    // This prop should be a child of a rotating joint bone (ex spine)
    [SerializeField]
    private Transform[] barLeftTransforms;
    [SerializeField]
    private Transform[] barRightTransforms;
    [SerializeField]
    private Transform mountingTransform;
    [SerializeField]
    private Transform mountingAboveTransform;
    [SerializeField]
    private Transform mountingBelowTransform;
    [SerializeField]
    private float mountingDistance;
    [SerializeField]
    private float mountingHeight;
    [SerializeField]
    private float mountingHorizontal;

    private Quaternion startRot;
    private Quaternion colliderRot;

    private void Start()
    {
        startRot = transform.localRotation;
        ReadColliderRot();
    }

    private void LateUpdate()
    {
        Vector3 mountNormal = GenerateMountNormal();
        Vector3 mountUp = (mountingTransform.position - mountingBelowTransform.position).normalized;
        Quaternion mountRot =
            Quaternion.LookRotation(-mountNormal, mountUp);
        transform.rotation = mountRot;
        transform.localRotation *= startRot;
        transform.position = mountingTransform.position;
        transform.position += mountNormal.normalized * mountingDistance;
        transform.position += mountUp * mountingHeight;
        transform.position += Vector3.Cross(mountNormal, mountUp) * -mountingHorizontal;
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
        Debug.Log(tiltSum);
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

    private Vector3 GenerateMountNormal()
    {
        Vector3 mountNormal = Vector3.zero;
        for (int i = 0; i < barRightTransforms.Length; i++)
        { 
            mountNormal +=
                Vector3.Cross(
                    (barRightTransforms[i].position - mountingTransform.position).normalized, 
                    (mountingAboveTransform.position - mountingTransform.position).normalized);
        }
        
        for (int i = 0; i < barLeftTransforms.Length; i++)
        { 
            mountNormal +=
                Vector3.Cross( 
                    (mountingAboveTransform.position - mountingTransform.position).normalized,
                    (barLeftTransforms[i].position - mountingTransform.position).normalized);
        }

        mountNormal *= 1.0f / (barLeftTransforms.Length + barRightTransforms.Length);
        
        return mountNormal;
    }

    [System.Obsolete]
    private Vector3 GenerateNetTiltDir()
    {
        Vector3 tiltSum = Vector3.zero;
        for (int i = 1; i < barRightTransforms.Length; i++)
        { 
            tiltSum += barRightTransforms[i].position - barRightTransforms[0].position;
        }
        tiltSum *= 1 / (barRightTransforms.Length - 1);
        return tiltSum;
    }

    // Generates a rotation about the parent transform only considering one local axis of the parent
    // of this gameObject's transform.
    [System.Obsolete]
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
