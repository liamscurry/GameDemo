using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CompanionTrack : MonoBehaviour
{
    [SerializeField]
    private CompanionWaypoint[] waypoints;
	[SerializeField]
	private bool matchSpeed;
    [SerializeField]
    private UnityEvent endEvents;

    public CompanionWaypoint[] Waypoints { get { return waypoints; } }
	public bool MatchSpeed { get { return matchSpeed; } }
    public UnityEvent EndEvents { get { return endEvents; } }

    private bool calculated;

    public void TryCalculate()
    {
        if (!calculated)
        {
            calculated = true;
            foreach (CompanionWaypoint waypoint in waypoints)
            {   
                Quaternion globalRotation = Quaternion.Euler(-waypoint.verticalRotation, waypoint.horizontalRotation + 90, waypoint.tiltRotation);
                globalRotation = transform.rotation * globalRotation;
                Vector3 globalPosition = transform.position + Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).MultiplyPoint(waypoint.position);
                waypoint.position = globalPosition;
                waypoint.rotation = globalRotation;
            }
        }
    }

    private void OnDrawGizmosSelected()
	{
		if (waypoints.Length != 0)
		{
			Matrix4x4 originalMatrix = Gizmos.matrix;
			Matrix4x4 cutsceneMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

			for (int i = 0; i < waypoints.Length; i++)
			{
				Quaternion waypointRotation = Quaternion.Euler(-waypoints[i].verticalRotation, waypoints[i].horizontalRotation + 90, waypoints[i].tiltRotation);
				Matrix4x4 waypointMatrix = Matrix4x4.TRS(waypoints[i].position, waypointRotation, Vector3.one);
				
				//Waypoint position and connection
				Gizmos.matrix = cutsceneMatrix;
				Gizmos.color = Color.cyan;
				if (i != waypoints.Length - 1)
					Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

				Gizmos.DrawCube(waypoints[i].position, Vector3.one * 0.5f);

				//Waypoint rotation
				Gizmos.matrix = cutsceneMatrix * waypointMatrix;
				Gizmos.color = Color.red;
				Gizmos.DrawLine(Vector3.zero, Vector3.right);
				Gizmos.DrawCube(Vector3.right, Vector3.one * 0.125f);

				Gizmos.color = Color.green;
				Gizmos.DrawLine(Vector3.zero, Vector3.up);
				Gizmos.DrawCube(Vector3.up, Vector3.one * 0.125f);

				Gizmos.color = Color.blue;
				Gizmos.DrawLine(Vector3.zero, Vector3.forward);
				Gizmos.DrawCube(Vector3.forward, Vector3.one * 0.125f);
				
				//Next waypoint indicator
				Gizmos.matrix = cutsceneMatrix;
				if (i != waypoints.Length - 1)
				{
					Gizmos.color = Color.yellow;

					Vector3 indicatorPosition = waypoints[i].position + (waypoints[i + 1].position - waypoints[i].position).normalized;
					Gizmos.DrawLine(waypoints[i].position, indicatorPosition);
					Gizmos.DrawCube(indicatorPosition, Vector3.one * 0.25f);
				}
			}

			Gizmos.matrix = originalMatrix;	
		}
	}
}
