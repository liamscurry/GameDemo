using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTowardsCamera : MonoBehaviour
{
    private void Update()
    {
        Vector3 direction = 
            Matho.StandardProjection3D(GameInfo.CameraController.transform.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);  
        }
    }
}
