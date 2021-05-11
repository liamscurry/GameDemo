using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

// A helper script that simulates prop movement locally on a character.
public class CharacterProp : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private Transform colliderParent;
    [SerializeField]
    private Transform[] colliderTransforms;
    [SerializeField]
    private Vector3[] colliderTransformsNormal;
    [SerializeField]
    private Vector3[] colliderStartPos;

    private const float sampleDuration = 0.125f;
    private float sampleTimer;

    private Quaternion lastSample;
    private Quaternion lastlastSample;

    private Vector3 startEulerAngles;
    private Quaternion targetRot;
    private Quaternion currentRot;
    private Quaternion startRot;

    [SerializeField]
    [HideInInspector]
    private Quaternion colliderRot;

    private void Start()
    {
        sampleTimer = 0;

        startEulerAngles = transform.localRotation.eulerAngles;
        currentRot = Quaternion.identity;
        startRot = transform.localRotation;
        targetRot = Quaternion.identity;
        lastSample = transform.parent.rotation;
        lastlastSample = transform.parent.rotation;

        ReadColliderRot();
    }

    private void LateUpdate()
    {
        sampleTimer += Time.deltaTime;
        if (sampleTimer > sampleDuration)
        {
            sampleTimer = 0;

            lastlastSample = lastSample;
            lastSample = transform.parent.rotation;

            targetRot = lastSample * Quaternion.Inverse(lastlastSample);
        }

        UpdateColliderRot();
        currentRot = Quaternion.RotateTowards(currentRot, targetRot, speed * Time.deltaTime);
        transform.localRotation =
            colliderRot * startRot;//Quaternion.Slerp(currentRot, Quaternion.identity, 0.6f) 
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
        foreach (Transform otherCollider in colliderTransforms)
        {
            Vector3 offset = otherCollider.transform.position;
            offset = colliderParent.worldToLocalMatrix.MultiplyPoint(offset);
            Debug.Log(otherCollider.name + ": " + offset.x + ", " + offset.y + ", " + offset.z);
        }
    }

    /*
    * Reads the beginning local offset of the collider transforms from this transform.
    */
    public void ReadColliderRot()
    {
        colliderRot = Quaternion.identity;
        /*List<Vector3> startPos = new List<Vector3>();
        foreach (Transform otherCollider in colliderTransforms)
        {
            Vector3 offset = otherCollider.transform.position;
            offset = colliderParent.worldToLocalMatrix.MultiplyPoint(offset);
            offset = Vector3.up;
            //startPos.Add(offset);
        }
        startPos.Add(new Vector3(-0.154456f, 0.573054f, -0.05880374f));*/
        
        //colliderStartPos = startPos;
    }

    /*
    * Adds a delta rotation based on the directional offset from the transform to this transform
    * in degrees rotated about this center.
    */
    private void UpdateColliderRot()
    {
        colliderRot = Quaternion.identity;
        for (int i = 0; i < colliderTransforms.Length; i++)
        { 
            Vector3 startOffset = colliderStartPos[i];
            startOffset = colliderParent.localToWorldMatrix.MultiplyPoint(startOffset);
            
            Vector3 currentOffset =
                colliderTransforms[i].transform.position;
            /*
            Debug.Log("start");
            Debug.Log(startOffset.x + ", " + startOffset.y + ", " + startOffset.z);
            Debug.Log(currentOffset.x + ", " + currentOffset.y + ", " + currentOffset.z);
            */

            Vector3 objectNormal =
                colliderParent.localToWorldMatrix.MultiplyPoint(colliderTransformsNormal[i]) -
                colliderParent.transform.position;
            objectNormal.Normalize();

            Vector3 rotAxis = Vector3.Cross(startOffset - transform.position, objectNormal);
            rotAxis.Normalize();
            currentOffset -= transform.position;
            currentOffset = currentOffset - Matho.Project(currentOffset, rotAxis);
            float objectCurrentAngle = Matho.AngleBetween(currentOffset, objectNormal);
            float startCurrentAngle =
                Matho.AngleBetween(startOffset - colliderParent.transform.position, currentOffset);
            currentOffset += transform.position;

            float objectStartAngle = 
                Matho.AngleBetween(
                    startOffset - colliderParent.transform.position,
                    objectNormal);
            
            /*
            if (!(objectCurrentAngle < objectStartAngle && startCurrentAngle < objectStartAngle)) 
            // outside of valid range, clamp
            {
                // current clamp incorrect.
                if (objectCurrentAngle < startCurrentAngle)
                {
                    currentOffset = transform.position + objectNormal;
                }
                else
                {
                    currentOffset = colliderTransforms[i].transform.position;
                    // this is wrong
                }
            }*/
            Debug.Log("max: " + objectStartAngle);
            Debug.Log("start to current : " + startCurrentAngle);
            Debug.Log("back to current : " + objectCurrentAngle);


            startOffset = transform.worldToLocalMatrix.MultiplyPoint(startOffset);
            currentOffset =
                transform.worldToLocalMatrix.MultiplyPoint(currentOffset);
            
            /*if (currentOffset.y > startOffset.y)
            {
                currentOffset = startOffset;
            }
            
            else if (startOffset.y < -1 * defaultPoseMargin)
            {
                startOffset.y = 
            }*/
           
            Quaternion limitRot = Quaternion.FromToRotation(startOffset, currentOffset);
            colliderRot *= limitRot;
        }
    }
}
