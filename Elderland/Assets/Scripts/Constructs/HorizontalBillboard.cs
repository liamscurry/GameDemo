using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class HorizontalBillboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.current != null)
        {
            Vector3 direction = 
                (Camera.current.transform.position - transform.position).normalized;

            transform.rotation = Quaternion.LookRotation(Vector3.up, direction);
        }

        //Debug.Log(direction);
        //Debug.Log("executing");
    }
}
