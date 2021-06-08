using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Copies the position of another transform to this transform. Useful for cases where this transform
* is not a child of the target transform.
*/
public class CopyPositionConstraint : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private void LateUpdate()
    {
        transform.position = target.transform.position;
    }
}
