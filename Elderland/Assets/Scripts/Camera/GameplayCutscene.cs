using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutscene 
{
	// Fields
	private bool turnWaypointUIOffOnEnd;

	Vector3 position;
	Quaternion rotationSpace;
	Vector3 cameraDirection;
	GameplayCutsceneEvent invokee;

	// Properties
	public LinkedListNode<GameplayCutsceneWaypoint> CurrentWaypointNode { get; private set; }
	public float Timer { get; private set; }
	public float WaitTimer { get; private set; }
	public bool TurnWaypointUIOffOnEnd { get { return turnWaypointUIOffOnEnd; } }
	public LinkedList<GameplayCutsceneWaypoint> Waypoints { get; private set; }
	public Vector3 TargetPosition { get { return position; } }
	public Quaternion TargetRotation { get { return rotationSpace; } }

	public GameplayCutscene(
		LinkedList<GameplayCutsceneWaypoint> waypoints,
		Vector3 position,
		Quaternion rotationSpace,
		Vector3 cameraDirection,
		bool turnWaypointUIOffOnEnd,
		GameplayCutsceneEvent invokee)
	{
		Waypoints = waypoints;
		this.position = position;
		this.rotationSpace = rotationSpace;
		this.cameraDirection = cameraDirection;
		this.turnWaypointUIOffOnEnd = turnWaypointUIOffOnEnd;
		this.invokee = invokee;
	}

	public void Start()
	{
		Camera camera = GameInfo.CameraController.Camera;

		CurrentWaypointNode = Waypoints.First;
		Timer = 0;
		WaitTimer = 0;
		GameInfo.CameraController.TargetDirection = 
			-CurrentWaypointNode.Value.CameraDirection;
		position = CurrentWaypointNode.Value.Position;
		rotationSpace =
			Quaternion.LookRotation(CurrentWaypointNode.Value.Rotation, Vector3.up);

		foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
		{
			GameInfo.CameraController.StartCoroutine(
				EventTimer(CurrentWaypointNode.Value, waypointEvent));
		}

		PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.GameplayCutscene);
		GameInfo.Manager.FreezeInput(this);
	}

	public IEnumerator EventTimer(
		GameplayCutsceneWaypoint waypoint,
		CameraCutsceneWaypointEvent waypointEvent)
	{
		yield return new WaitForSeconds(waypointEvent.normalizedTime * waypoint.clipsPerDistance);
		waypointEvent.methods.Invoke();
	}

	public bool Update(bool completedMatch)
	{
		bool exiting = false;

		if (completedMatch)
		{
			WaitTimer += Time.deltaTime;
			if (WaitTimer > CurrentWaypointNode.Value.waitTime)
			{
				exiting = true;

				if (CurrentWaypointNode.Next != null)
				{
					CurrentWaypointNode = CurrentWaypointNode.Next;
					Timer = 0;
					WaitTimer = 0;

					invokee.GenerateConcreteNextWaypoint(
						ref position,
						ref rotationSpace,
						ref cameraDirection,
						CurrentWaypointNode.Value);

					GameInfo.CameraController.TargetDirection = 
						-cameraDirection;

					foreach (CameraCutsceneWaypointEvent waypointEvent in
							 CurrentWaypointNode.Value.events)
					{
						GameInfo.CameraController.StartCoroutine(
							EventTimer(CurrentWaypointNode.Value, waypointEvent));
					}
				}
				else
				{
					GameInfo.CameraController.TargetDirection = Vector3.zero;
					GameInfo.CameraController.StartGameplay();
					GameInfo.Manager.UnfreezeInput(this);
					
					PlayerInfo.PhysicsSystem.ForceTouchingFloor();
					PlayerInfo.PhysicsSystem.Animating = false;

					PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.Exit);
				}
			}
		}

		return exiting;
	}
}