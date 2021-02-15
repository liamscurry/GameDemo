using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutscene 
{
	private bool turnWaypointUIOffOnEnd;

	public LinkedListNode<GameplayCutsceneWaypoint> CurrentWaypointNode { get; private set; }
	public float Timer { get; private set; }
	public float WaitTimer { get; private set; }
	public bool TurnWaypointUIOffOnEnd { get { return turnWaypointUIOffOnEnd; } }
	public LinkedList<GameplayCutsceneWaypoint> Waypoints { get; private set; }

	public GameplayCutscene(
		LinkedList<GameplayCutsceneWaypoint> waypoints,
		bool turnWaypointUIOffOnEnd)
	{
		Waypoints = waypoints;
		this.turnWaypointUIOffOnEnd = turnWaypointUIOffOnEnd;
	}

	public void Start()
	{
		Camera camera = GameInfo.CameraController.Camera;

		CurrentWaypointNode = Waypoints.First;
		Timer = 0;
		WaitTimer = 0;

		foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
		{
			GameInfo.CameraController.StartCoroutine(EventTimer(CurrentWaypointNode.Value, waypointEvent));
		}

		GameInfo.Manager.FreezeInput(this);
	}

	public IEnumerator EventTimer(GameplayCutsceneWaypoint waypoint, CameraCutsceneWaypointEvent waypointEvent)
	{
		yield return new WaitForSeconds(waypointEvent.normalizedTime * waypoint.clipSpeed);
		waypointEvent.methods.Invoke();
	}

	public void Update()
	{
		Timer += Time.deltaTime;
		if (Timer / CurrentWaypointNode.Value.clipSpeed >= 1)
		{
			WaitTimer += Time.deltaTime;
			if (WaitTimer > CurrentWaypointNode.Value.waitTime)
			{
				if (CurrentWaypointNode.Next != null)
				{
					CurrentWaypointNode = CurrentWaypointNode.Next;
					Timer = 0;
					WaitTimer = 0;

					foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
					{
						GameInfo.CameraController.StartCoroutine(EventTimer(CurrentWaypointNode.Value, waypointEvent));
					}
				}
				else
				{
					GameInfo.CameraController.StartGameplay();
					GameInfo.Manager.UnfreezeInput(this);
					
					PlayerInfo.PhysicsSystem.ForceTouchingFloor();
					PlayerInfo.PhysicsSystem.Animating = false;
				}
			}
		}
	}
}