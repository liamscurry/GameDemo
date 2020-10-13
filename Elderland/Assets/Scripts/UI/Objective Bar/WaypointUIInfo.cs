using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointUIInfo : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldPosition;

    public Vector3 WorldPosition { get { return worldPosition; } }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(worldPosition, Vector3.one);
    }
}
