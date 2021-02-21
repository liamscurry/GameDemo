using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutsceneWaypoint 
{
	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }
	public float RotationWeight { get; set; }
	public bool UseRotationAsGlobal { get; set; }
	public Vector3 CameraDirection { get; set; }
	public readonly float clipsPerDistance;
	public readonly float waitTime;
	public readonly AnimationClip travelClip;
	public readonly AnimationClip waitClip;
	public readonly CameraCutsceneWaypointEvent[] events;
	public readonly UnityEvent OnStateExit;
	public readonly UnityEvent OnCompleteMatch;

	public GameplayCutsceneWaypoint(
		Vector3 position,
		Vector3 rotation,
		float rotationWeight,
		bool useRotationAsGlobal,
		Vector3 cameraDirection,
		float time,
		float waitTime,
		AnimationClip travelClip,
		AnimationClip waitClip,
		CameraCutsceneWaypointEvent[] events,
		UnityEvent onStateExit,
		UnityEvent onCompleteMatch)
	{
		Position = position;
		Rotation = rotation;
		RotationWeight = rotationWeight;
		UseRotationAsGlobal = useRotationAsGlobal;
		CameraDirection = cameraDirection;
		this.clipsPerDistance = time;
		if (time < 0.1f)
			this.clipsPerDistance = 0.1f;
		this.waitTime = waitTime;
		this.travelClip = travelClip;
		this.waitClip = waitClip;
		this.events = events;
		OnStateExit = onStateExit;
		OnCompleteMatch = onCompleteMatch;
	}
}
