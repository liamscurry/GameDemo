using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour 
{
	public Vector3 Normal { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float Depth { get; private set; }
    public Vector3 RightDirection { get; private set; }
    public Vector3 UpDirection { get; private set; }

    private void Start()
    {
        /*
        Vector3 angles = transform.rotation.eulerAngles;
		Quaternion inverseQuaternion = Quaternion.Euler(-angles.x, -angles.y, -angles.z);
		Matrix4x4 conversion = Matrix4x4.TRS(Vector3.zero, inverseQuaternion, Vector3.one);
		Vector3 standardNormal = conversion.MultiplyPoint(new Vector3(1, 0, 0)).normalized;
        Normal = new Vector3(standardNormal.x, 0, standardNormal.z).normalized;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Width = boxCollider.size.z * transform.localScale.z;
        Height = boxCollider.size.y * transform.localScale.y;
        Depth = boxCollider.size.x * transform.localScale.x;

        UpDirection = Vector3.up;
        RightDirection = Matho.Rotate(Normal, Vector3.up, -90);
        */

        Normal = transform.right;
        UpDirection = Vector3.up;
        RightDirection = transform.forward;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Width = boxCollider.size.z * transform.lossyScale.z;
        Height = boxCollider.size.y * transform.lossyScale.y;
        Depth = boxCollider.size.x * transform.lossyScale.x;
    }
}