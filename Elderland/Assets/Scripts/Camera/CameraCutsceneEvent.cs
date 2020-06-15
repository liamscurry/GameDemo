using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraCutsceneEvent : MonoBehaviour 
{
	[SerializeField]
	private float startConnectionTime;
	[SerializeField]
	private CameraCutsceneWaypointEvent[] startConnectionEvents;
	[SerializeField]
	private float startWaitTime;
	[SerializeField]
	private bool startJumpcut;
	[SerializeField]
	private GeneratedWaypoint[] waypoints;
	[SerializeField]
	[Range(0f, 360f)]
	private float finalHorizontalAngle;
	[SerializeField]
	private bool lookAtFinalWaypoint;
	[SerializeField]
	[Range(30f, 150f)]
	private float finalVerticalAngle;
	[SerializeField]
	private bool makeLastWaypoint;
	[SerializeField]
	private bool transitionToGameplayUponFinish;
	[SerializeField]
	private bool unfreezeInputUponFinish;
	[SerializeField]
	private UnityEvent endEvent;

	public void Invoke()
	{
		if (waypoints.Length == 0)
		{
			throw new System.ArgumentException("Cutscene must have 1 or more waypoints");
		}
		else
		{
			Vector3 parentEulerAngles = transform.rotation.eulerAngles;
			var linkedWaypoints = new LinkedList<CameraCutsceneWaypoint>();
			foreach (GeneratedWaypoint waypoint in waypoints)
			{
				waypoint.CheckConnectionTime();

				Quaternion globalRotation = Quaternion.Euler(-waypoint.verticalRotation, waypoint.horizontalRotation + 90, waypoint.tiltRotation);
				globalRotation = transform.rotation * globalRotation;
				Vector3 globalPosition = transform.position + Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).MultiplyPoint(waypoint.position);
				linkedWaypoints.AddLast(new CameraCutsceneWaypoint(globalPosition, globalRotation, waypoint.connectionTime, waypoint.waitTime, waypoint.jumpCut, waypoint.connectionEvents));
			}

			GameInfo.CameraController.StartCutscene(
				new CameraCutscene(
				linkedWaypoints,
				startConnectionTime,
				startConnectionEvents,
				startWaitTime,
				startJumpcut,
				finalHorizontalAngle,
				lookAtFinalWaypoint,
				finalVerticalAngle,
				makeLastWaypoint,
				transitionToGameplayUponFinish,
				unfreezeInputUponFinish,
				endEvent));
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
			Gizmos.color = Color.red;
			Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
			Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
			Gizmos.DrawCube(transform.position + Vector3.up, Vector3.one * 0.125f);	
			Vector3 finalDirection = Matho.SphericalToCartesianX(1, finalHorizontalAngle - 20, finalVerticalAngle + 180);
			Gizmos.DrawLine(transform.position, transform.position + finalDirection);
			Gizmos.DrawCube(transform.position + finalDirection, Vector3.one * 0.25f);
		}
	}

	[System.Serializable]
	public class GeneratedWaypoint
	{
		[SerializeField]
		public Vector3 position;
		[SerializeField]
		public float horizontalRotation;
		[SerializeField]
		public float verticalRotation;
		[SerializeField]
		public float tiltRotation;
		[SerializeField]
		public float connectionTime;
		[SerializeField]
		public float waitTime;
		[SerializeField]
		public bool jumpCut;
		[SerializeField]
		public CameraCutsceneWaypointEvent[] connectionEvents;

		public GeneratedWaypoint(
			Vector3 position,
			float horizontalRotation,
			float verticalRotation,
			float tiltRotation,
			float connectionTime,
			float waitTime,
			bool jumpCut,
			CameraCutsceneWaypointEvent[] connectionEvents)
		{
			this.position = position;
			this.horizontalRotation = horizontalRotation;
			this.verticalRotation = verticalRotation;
			this.tiltRotation = tiltRotation;
			this.connectionTime = connectionTime;
			this.waitTime = waitTime;
			this.jumpCut = jumpCut;
			this.connectionEvents = connectionEvents;
		}

		public void CheckConnectionTime()
		{
			if (connectionTime < 0.1f)
				connectionTime = 0.1f;
		}
	}
}