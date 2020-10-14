using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointUIInfo : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldPosition;
    [Header("For side missions")]
    [SerializeField]
    private GameObject waypoint;

    public Vector3 WorldPosition { get { return worldPosition; } }
    public GameObject Waypoint { get { return waypoint; } }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(worldPosition, Vector3.one);
    }
}
