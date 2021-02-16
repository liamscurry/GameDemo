using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutsceneEvent : MonoBehaviour 
{
	[SerializeField]
	private GeneratedWaypoint[] waypoints;
	[SerializeField]
	private bool turnWaypointUIOffOnEnd;

	public void Invoke()
	{
		if (waypoints.Length == 0)
		{
			throw new System.ArgumentException("Gameplay Cutscene must have 1 or more waypoints");
		}
		else
		{
			var linkedWaypoints = new LinkedList<GameplayCutsceneWaypoint>();

			Vector3 position;
			Quaternion rotationSpace;
			Vector3 rotationVector;
			Vector3 cameraDirection;
			GenerateStartWaypoint(
				out position,
				out rotationSpace,
				out rotationVector,
				out cameraDirection);

			linkedWaypoints.AddLast(
				new GameplayCutsceneWaypoint(
					position,
					rotationVector,
					cameraDirection,
					waypoints[0].clipSpeed,
					waypoints[0].waitTime,
					waypoints[0].travelClip,
					waypoints[0].waitClip,
					waypoints[0].connectionEvents));

			for (int i = 1; i < waypoints.Length; i++)
			{
				waypoints[i].CheckConnectionTime();

				linkedWaypoints.AddLast(
					new GameplayCutsceneWaypoint(
						waypoints[i].position,
						waypoints[i].rotation,
						waypoints[i].cameraDirection,
						waypoints[i].clipSpeed,
						waypoints[i].waitTime,
						waypoints[i].travelClip,
						waypoints[i].waitClip,
						waypoints[i].connectionEvents));
			}

			GameInfo.CameraController.StartGameplayCutscene(
				new GameplayCutscene(
					linkedWaypoints,
					position,
					rotationSpace,
					cameraDirection,
					turnWaypointUIOffOnEnd,
					this));
		}
	}

	/*
	* Needed for gizmos and cutscene origin.
	*/
	public void GenerateStartWaypoint(
		out Vector3 position,
		out Quaternion rotationSpace,
		out Vector3 rotationVector,
		out Vector3 cameraDirection)
	{
		Matrix4x4 gameObjectMRotation = 
			Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
		Vector3 globalPosition =
				transform.position +
				gameObjectMRotation.MultiplyPoint(waypoints[0].position);
		Quaternion globalRotation =
			Quaternion.FromToRotation(Vector3.forward, waypoints[0].rotation);
		globalRotation = transform.rotation * globalRotation;
		Vector3 globalRotationVector = 
			Matrix4x4.TRS(
				Vector3.zero,
				globalRotation,
				Vector3.one).MultiplyPoint(Vector3.forward);
		Vector3 globalCameraDirection = 
			waypoints[0].cameraDirection;

		position = globalPosition;
		rotationSpace = globalRotation;
		rotationVector = globalRotationVector;
		cameraDirection = globalCameraDirection;
	}

	/*
	* Needed for gizmos and real time generation based on current player position.
	*/
	public void GenerateNextWaypoint(
		ref Vector3 position,
		ref Quaternion rotationSpace,
		ref Vector3 cameraDirection,
		GeneratedWaypoint waypoint)
	{
		rotationSpace =
			Quaternion.Euler(waypoint.rotation.x, waypoint.rotation.y, waypoint.rotation.z) *
			rotationSpace;
		position =
			position +
			SpaceMultiply(waypoint.position, rotationSpace);
		cameraDirection =
			SpaceMultiply(waypoint.cameraDirection, rotationSpace);
	}

	/*
	* Needed to call GenearteNextWaypoint with altered type
	*/
	public void GenerateConcreteNextWaypoint(
		ref Vector3 position,
		ref Quaternion rotationSpace,
		ref Vector3 cameraDirection,
		GameplayCutsceneWaypoint waypoint)
	{
		var generatedWaypoint = 
			new GeneratedWaypoint(
				waypoint.Position,
				waypoint.Rotation,
				waypoint.CameraDirection,
				waypoint.clipSpeed,
				waypoint.waitTime,
				waypoint.travelClip,
				waypoint.waitClip,
				waypoint.events
			);

		GenerateNextWaypoint(
			ref position,
			ref rotationSpace,
			ref cameraDirection,
			generatedWaypoint);
	}

	private void OnDrawGizmosSelected()
	{
		if (waypoints.Length != 0)
		{
			Vector3 position;
			Quaternion rotationSpace;
			Vector3 rotationVector;
			Vector3 cameraDirection;
			GenerateStartWaypoint(
				out position,
				out rotationSpace,
				out rotationVector,
				out cameraDirection);

			Gizmos.color = Color.cyan;	
			Gizmos.DrawCube(position, Vector3.one * 0.5f);	
			Gizmos.color = Color.yellow;	
			Gizmos.DrawLine(position + cameraDirection, position);
			Gizmos.DrawCube(position + cameraDirection, Vector3.one * 0.25f);

			for (int i = 1; i < waypoints.Length; i++)
			{
				Vector3 oldGlobalPosition = position;

				GenerateNextWaypoint(
					ref position,
					ref rotationSpace,
					ref cameraDirection,
					waypoints[i]);
				
				Gizmos.color = Color.cyan;	
				Gizmos.DrawLine(position, oldGlobalPosition);		
				Gizmos.DrawCube(position, Vector3.one * 0.5f);	
				
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(position + cameraDirection, position);
				Gizmos.DrawCube(position + cameraDirection, Vector3.one * 0.25f);
			}
		}
	}

	private Vector3 SpaceMultiply(Vector3 direction, Quaternion globalRotation)
	{
		Matrix4x4 rotationMatrix =
			Matrix4x4.TRS(
				Vector3.zero,
				globalRotation,
				Vector3.one);
		
		Vector3 forward =
			rotationMatrix.MultiplyVector(Vector3.forward);
		Vector3 right =
			rotationMatrix.MultiplyVector(Vector3.right);
		Vector3 up =
			rotationMatrix.MultiplyVector(Vector3.up);
			
		return
			direction.x * right + 
			direction.y * up + 
			direction.z * forward;
	}

	[System.Serializable]
	public class GeneratedWaypoint
	{
		[SerializeField]
		public Vector3 position;
		[SerializeField]
		public Vector3 rotation;
		[SerializeField]
		public Vector3 cameraDirection;
		[SerializeField]
		public float clipSpeed;
		[SerializeField]
		public AnimationClip travelClip;
		[SerializeField]
		public AnimationClip waitClip;
		[SerializeField]
		public float waitTime;
		[SerializeField]
		public CameraCutsceneWaypointEvent[] connectionEvents;

		public GeneratedWaypoint(
			Vector3 position,
			Vector3 rotation,
			Vector3 cameraDirection,
			float clipSpeed,
			float waitTime,
			AnimationClip travelClip,
			AnimationClip waitClip,
			CameraCutsceneWaypointEvent[] connectionEvents)
		{
			this.position = position;
			this.rotation = rotation;
			this.cameraDirection = cameraDirection;
			this.clipSpeed = clipSpeed;
			this.waitTime = waitTime;
			this.travelClip = travelClip;
			this.waitClip = waitClip;
			this.connectionEvents = connectionEvents;
		}

		public void CheckConnectionTime()
		{
			if (clipSpeed < 0.1f)
				clipSpeed = 0.1f;
		}
	}
}