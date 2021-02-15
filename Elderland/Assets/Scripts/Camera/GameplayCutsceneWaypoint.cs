using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayCutsceneWaypoint 
{
	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }
	public Vector3 CameraDirection { get; set; }
	public readonly float clipSpeed;
	public readonly float waitTime;
	public readonly AnimationClip travelClip;
	public readonly AnimationClip waitClip;
	public readonly CameraCutsceneWaypointEvent[] events;

	public GameplayCutsceneWaypoint(
		Vector3 position,
		Vector3 rotation,
		Vector3 cameraDirection,
		float time,
		float waitTime,
		AnimationClip travelClip,
		AnimationClip waitClip,
		CameraCutsceneWaypointEvent[] events)
	{
		Position = position;
		Rotation = rotation;
		CameraDirection = cameraDirection;
		this.clipSpeed = time;
		if (time < 0.1f)
			this.clipSpeed = 0.1f;
		this.waitTime = waitTime;
		this.travelClip = travelClip;
		this.waitClip = waitClip;
		this.events = events;
	}
}
