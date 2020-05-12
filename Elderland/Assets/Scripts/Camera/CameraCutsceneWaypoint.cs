using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCutsceneWaypoint 
{
	public Vector3 Position { get; set; }
	public Quaternion Rotation { get; set; }
	public readonly float time;
	public readonly float waitTime;
	public readonly CameraCutsceneWaypointEvent[] events;

	public CameraCutsceneWaypoint(Vector3 position, Quaternion rotation, float time, float waitTime, CameraCutsceneWaypointEvent[] events)
	{
		Position = position;
		Rotation = rotation;
		this.time = time;
		if (time < 0.1f)
			this.time = 0.1f;
		this.waitTime = waitTime;
		this.events = events;
	}
}
