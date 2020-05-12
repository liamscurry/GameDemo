using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mantle : MonoBehaviour 
{
	public enum MantleType { Short, Tall }
	
	[SerializeField]
	private MantleType type;

	public Vector3 Normal { get; private set; }
    public Vector3 RightDirection { get; private set; }
    public Vector3 UpDirection { get; private set; }
	public float TopVerticalPosition { get; private set; }
	public float BottomVerticalPosition { get; private set; }
	public MantleType Type { get { return type; } }

    private void Start()
    {
        Normal = transform.right;
        UpDirection = Vector3.up;
        RightDirection = transform.forward;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        TopVerticalPosition = transform.position.y + (boxCollider.center.y + boxCollider.size.y / 2) * transform.lossyScale.y;
		BottomVerticalPosition = transform.position.y + (boxCollider.center.y - boxCollider.size.y / 2) * transform.lossyScale.y;
    }
}