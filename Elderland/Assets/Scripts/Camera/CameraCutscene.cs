using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraCutscene 
{
	public readonly LinkedList<CameraCutsceneWaypoint> waypoints;

	private float startConnectionTime;
	private CameraCutsceneWaypointEvent[] startConnectionEvents;
	private float startWaitTime;
	private bool startJumpcut;
	private float finalHorizontalAngle;
	private bool lookAtFinalWaypoint;
	private float finalVerticalAngle;
	private bool generatedLastWaypoint;
	private bool makeLastWaypoint;
	private bool transitionToGameplayUponFinish;
	private bool unfreezeInputUponFinish;
	private bool turnWaypointUIOffOnEnd;
	private UnityEvent endEvent;

	public LinkedListNode<CameraCutsceneWaypoint> CurrentWaypointNode { get; private set; }
	public LinkedListNode<CameraCutsceneWaypoint> TargetWaypointNode { get; private set; }
	public float Timer { get; private set; }
	public float WaitTimer { get; private set; }
	public bool TurnWaypointUIOffOnEnd { get { return turnWaypointUIOffOnEnd; } }

	public CameraCutscene(
		LinkedList<CameraCutsceneWaypoint> waypoints,
		float startConnectionTime,
		CameraCutsceneWaypointEvent[] startConnectionEvents,
		float startWaitTime,
		bool startJumpcut,
		float finalHorizontalAngle,
		bool lookAtFinalWaypoint,
		float finalVerticalAngle,
		bool makeLastWaypoint,
		bool transitionToGameplayUponFinish,
		bool unfreezeInputUponFinish,
		bool turnWaypointUIOffOnEnd,
		UnityEvent endEvent)
	{
		this.waypoints = waypoints;
		this.startConnectionTime = startConnectionTime;
		this.startConnectionEvents = startConnectionEvents;
		this.startWaitTime = startWaitTime;
		this.startJumpcut = startJumpcut;
		this.finalHorizontalAngle = finalHorizontalAngle;
		this.lookAtFinalWaypoint = lookAtFinalWaypoint;
		this.finalVerticalAngle = finalVerticalAngle;
		this.makeLastWaypoint = makeLastWaypoint;
		this.transitionToGameplayUponFinish = transitionToGameplayUponFinish;
		this.unfreezeInputUponFinish = unfreezeInputUponFinish;
		this.turnWaypointUIOffOnEnd = turnWaypointUIOffOnEnd;
		this.endEvent = endEvent;
	}

	public void Start()
	{
		Camera camera = GameInfo.CameraController.Camera;
		CameraCutsceneWaypoint autoWaypoint = new CameraCutsceneWaypoint(camera.transform.position, camera.transform.rotation, startConnectionTime, startWaitTime, startJumpcut, startConnectionEvents);
		waypoints.AddFirst(autoWaypoint);

		CurrentWaypointNode = waypoints.First;
		TargetWaypointNode = waypoints.First.Next;
		Timer = 0;
		WaitTimer = 0;

		foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
		{
			GameInfo.CameraController.StartCoroutine(EventTimer(CurrentWaypointNode.Value, waypointEvent));
		}

		generatedLastWaypoint = false;

		GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);
	}

	public IEnumerator EventTimer(CameraCutsceneWaypoint waypoint, CameraCutsceneWaypointEvent waypointEvent)
	{
		yield return new WaitForSeconds(waypointEvent.normalizedTime * waypoint.time);
		waypointEvent.methods.Invoke();
	}

	public void Update()
	{
		Timer += Time.deltaTime;
		if (Timer / CurrentWaypointNode.Value.time >= 1)
		{
			WaitTimer += Time.deltaTime;
			if (WaitTimer > CurrentWaypointNode.Value.waitTime)
			{
				if (TargetWaypointNode.Next != null)
				{
					CurrentWaypointNode = TargetWaypointNode;
					TargetWaypointNode = TargetWaypointNode.Next;
					Timer = 0;
					WaitTimer = 0;

					foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
					{
						GameInfo.CameraController.StartCoroutine(EventTimer(CurrentWaypointNode.Value, waypointEvent));
					}
				}
				else
				{
					if (!generatedLastWaypoint && makeLastWaypoint)
					{
						generatedLastWaypoint = true;
						//Adding a final waypoint so that the cutscene transitions to gameplay when done.
						GenerateFinalWaypoint();
						CurrentWaypointNode = TargetWaypointNode;
						TargetWaypointNode = TargetWaypointNode.Next;
						Timer = 0;
						WaitTimer = 0;

						foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
						{
							GameInfo.CameraController.StartCoroutine(EventTimer(CurrentWaypointNode.Value, waypointEvent));
						}
					}
					else
					{
						waypoints.RemoveFirst();
						if (makeLastWaypoint)
							waypoints.RemoveLast();
						if (transitionToGameplayUponFinish)
						{
							GameInfo.CameraController.StartGameplay();
						}
						else
						{
							GameInfo.CameraController.StartIdle();
						}

						Debug.Log(GameInfo.Manager.ReceivingInput.Tracker == this);

						if (unfreezeInputUponFinish)
							GameInfo.Manager.ReceivingInput.TryReleaseLock(this, GameInput.Full);

						if (endEvent != null)
							endEvent.Invoke();
						PlayerInfo.PhysicsSystem.ForceTouchingFloor();
						PlayerInfo.PhysicsSystem.Animating = false;
					}
				}
			}
		}
	}

	private void GenerateFinalWaypoint()
	{
		if (!lookAtFinalWaypoint)
		{
			GameInfo.CameraController.HorizontalAngle = finalHorizontalAngle;
			GameInfo.CameraController.VerticalAngle = finalVerticalAngle;
		}
		else
		{
			Vector2 rotationDirection = Matho.StdProj2D(PlayerInfo.Player.transform.position - TargetWaypointNode.Value.Position);
			GameInfo.CameraController.HorizontalAngle = Matho.Angle(rotationDirection) + GameInfo.CameraController.HorizontalOffset;
			GameInfo.CameraController.VerticalAngle = finalVerticalAngle;
		}

		GameInfo.CameraController.GeneratePosition(PlayerInfo.Player.transform.position);
		GameInfo.CameraController.ForceRadius();
		
		Quaternion targetRotation = GameInfo.CameraController.GenerateRotation();
		Vector3 targetPosition = GameInfo.CameraController.GeneratePosition(PlayerInfo.Player.transform.position);

		waypoints.AddLast(new CameraCutsceneWaypoint(targetPosition, targetRotation, 0, 0, false, null));
	}
}