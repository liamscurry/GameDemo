using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutscene 
{
	// Fields
	private const float travelWaitSpeed = 5;

	private bool turnWaypointUIOffOnEnd;

	private Vector3 position;
	private Quaternion rotationSpace;
	private Vector3 cameraDirection;
	private GameplayCutsceneEvent invokee;
	private AnimationLoop animationLoop;
	private string[] loopNames;

	// Properties
	public LinkedListNode<GameplayCutsceneWaypoint> CurrentWaypointNode { get; private set; }
	public float Timer { get; private set; }
	public float WaitTimer { get; private set; }
	public bool TurnWaypointUIOffOnEnd { get { return turnWaypointUIOffOnEnd; } }
	public LinkedList<GameplayCutsceneWaypoint> Waypoints { get; private set; }
	public Vector3 TargetPosition { get { return position; } }
	public Quaternion TargetRotation { get { return rotationSpace; } }
	public float CurrentStateDuration { get; private set; }
	public float CurrentStateNormDuration { get; private set; }

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

		animationLoop =
			new AnimationLoop(PlayerInfo.Controller, PlayerInfo.Animator, "GameplayCutsceneVertex");
		loopNames = new string[] { "GameplayCutsceneTravel", "GameplayCutsceneWait" };
	}

	public void Start()
	{
		Camera camera = GameInfo.CameraController.Camera;
		animationLoop.ResetSegmentIndex();

		CurrentWaypointNode = Waypoints.First;
		Timer = 0;
		WaitTimer = 0;

		GameInfo.CameraController.TargetDirection = 
			-CurrentWaypointNode.Value.CameraDirection;
		position = CurrentWaypointNode.Value.Position;
		rotationSpace =
			Quaternion.LookRotation(CurrentWaypointNode.Value.Rotation, Vector3.up);
		CurrentStateDuration = CalculateStateDuration();
		CurrentStateNormDuration = CalculateStateNormDuration();

		UpdateAnimationClips();
		InvokeEventTimers();

		PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.GameplayCutscene);
		PlayerInfo.Animator.ResetTrigger(AnimationConstants.Player.Proceed);
		PlayerInfo.Animator.ResetTrigger(AnimationConstants.Player.Exit);
		GameInfo.Manager.FreezeInput(this);
	}

	/*
	* Timer needed to invoke events at correct time in each target waypoint approach.
	* Dev: Currently works when clip length is 1. Doesn't work otherwise. (Timing off)
	* They seem to be called too early.
	* Works, had clips different in animation controller from event inspector.
	* Applying the clips is the next step.
	*/
	public IEnumerator EventTimer(
		GameplayCutsceneWaypoint waypoint,
		CameraCutsceneWaypointEvent waypointEvent)
	{
		yield return new WaitForSeconds(waypointEvent.normalizedTime * CurrentStateDuration);
		waypointEvent.methods.Invoke();
	}

	/*
	* Helper method needed for timing events.
	*/
	private float CalculateStateDuration()
	{
		return CalculateStateNormDuration() * CurrentWaypointNode.Value.travelClip.length;
	}

	/*
	* Helper method needed for timing events.
	*/
	private float CalculateStateNormDuration()
	{
		float distanceToTarget = 
            Vector3.Distance(
                PlayerInfo.Player.transform.position,
                GameInfo.CameraController.GameplayCutscene.TargetPosition);

		return distanceToTarget *
			   CurrentWaypointNode.Value.clipsPerDistance;
	}

	/*
	* Needed for joint animation blend assignment in vertex animation loop.
	*/
	private AnimationClip[] GetAnimationClips()
	{
		var clips =
			new AnimationClip[] 
			{ 
				CurrentWaypointNode.Value.travelClip,
				CurrentWaypointNode.Value.waitClip
			};
		return clips;
	}

	/*
	* Helper method needed for initialization and updating during animation loop traversal.
	*/
	private void UpdateAnimationClips()
	{
		animationLoop.SetNextSegmentClip(
			GetAnimationClips(), loopNames);
		animationLoop.SetCurrentSegmentSpeed(0);
	}

	/*
	* Helper needed for initialization and updating during animation loop traversal.
	*/
	private void InvokeEventTimers()
	{
		foreach (CameraCutsceneWaypointEvent waypointEvent in CurrentWaypointNode.Value.events)
		{
			GameInfo.CameraController.StartCoroutine(
				EventTimer(CurrentWaypointNode.Value, waypointEvent));
		}
	}

	public bool Update(bool completedMatch)
	{
		bool exiting = false;

		if (completedMatch)
		{
			WaitTimer += Time.deltaTime;
			animationLoop.ChangeCurrentSegmentSpeed(travelWaitSpeed * Time.deltaTime);
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

					CurrentStateDuration = CalculateStateDuration();
					CurrentStateNormDuration = CalculateStateNormDuration();

					UpdateAnimationClips();
					InvokeEventTimers();
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