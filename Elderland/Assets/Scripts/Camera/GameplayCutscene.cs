using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayCutscene 
{
	public enum TransitionChoice { WaitNext, TravelNext, Exit }

	// Fields
	private const float minimumWaitTime = 0.1f;

	private bool turnWaypointUIOffOnEnd;

	private Vector3 position;
	private Quaternion rotationSpace;
	private Vector3 cameraDirection;
	private GameplayCutsceneEvent invokee;
	private AnimationLoop animationLoop;
	private string[] loopNames;

	private bool delayTargetDirection;

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
	public UnityEvent OnStateExit { get; private set; }

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

	public void StartCutscene()
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
		CurrentStateDuration =
			CalculateStateDuration(PlayerInfo.Player.transform.position);
		CurrentStateNormDuration =
			CalculateStateNormDuration(PlayerInfo.Player.transform.position);

		UpdateAnimationClips();
		InvokeEventTimers();

		PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.GameplayCutscene);
		PlayerInfo.Animator.ResetTrigger(AnimationConstants.Player.Proceed);
		PlayerInfo.Animator.ResetTrigger(AnimationConstants.Player.Exit);
		GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);

		delayTargetDirection = false;
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
	* Needed to set final transform backings after teleporting player on a teleporter.
	*/
	public void PostTeleportPlayer()
	{
		//position = PlayerInfo.Player.transform.position;
		//rotationSpace = PlayerInfo.Player.transform.rotation;
		GameInfo.CameraController.TargetDirection = -cameraDirection;
	}

	/*
	* Needed to update transform backings yet not move player to retain trigger copy on
	* real teleport.
	*/
	public void PreTeleportPlayer(PortalTeleporter teleporter)
	{
		Vector3 tempPosition;
		Quaternion tempRotation;
		Vector3 tempForward;
		teleporter.PreTeleportPlayer(
			out tempPosition,
			out tempRotation,
			out tempForward);
		position = tempPosition;
		rotationSpace = tempRotation;
		cameraDirection = tempForward;
		delayTargetDirection = true;
	}

	/*
	* Helper method needed for timing events.
	*/
	private float CalculateStateDuration(Vector3 startPosition)
	{
		return CalculateStateNormDuration(startPosition) *
			   CurrentWaypointNode.Value.travelClip.length;
	}

	/*
	* Helper method needed for timing events.
	*/
	private float CalculateStateNormDuration(Vector3 startPosition)
	{
		float distanceToTarget = 
            Vector3.Distance(
                startPosition,
                position);

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

	/*
	* Update method needed to run travel logic. May exit cutscene, go to next node or go to wait state.
	*/
	public bool UpdateTravel(bool completedMatch)
	{
		bool exiting = false;

		if (completedMatch)
		{
			OnStateExit = CurrentWaypointNode.Value.OnStateExit;
			if (CurrentWaypointNode.Value.OnCompleteMatch != null)
				CurrentWaypointNode.Value.OnCompleteMatch.Invoke();
			exiting = true;

			if (CurrentWaypointNode.Value.waitTime > minimumWaitTime)
			{
				PlayerInfo.Animator.SetInteger(
					AnimationConstants.Player.ChoiceSeparator,
					(int) TransitionChoice.WaitNext);
			}
			else
			{
				if (CurrentWaypointNode.Next != null)
				{
					IncrementNode();
				}
				else
				{
					ExitNode();
				}
			}
		}

		return exiting;
	}

	/*
	* Update method needed to run wait timer. May exit cutscene or go to next node.
	*/
	public bool UpdateWait()
	{
		bool exiting = false;

		WaitTimer += Time.deltaTime;
		if (WaitTimer > CurrentWaypointNode.Value.waitTime)
		{
			exiting = true;

			if (CurrentWaypointNode.Next != null)
			{
				IncrementNode();
			}
			else
			{
				ExitNode();
			}
		}

		return exiting;
	}

	/*
	* Needed to properly go to next cutscene state. Moves animation loop to next travel segment.
	*/
	private void IncrementNode()
	{
		CurrentWaypointNode = CurrentWaypointNode.Next;
		Timer = 0;
		WaitTimer = 0;
		
		Vector3 positionBefore = position;

		invokee.GenerateConcreteNextWaypoint(
			ref position,
			ref rotationSpace,
			ref cameraDirection,
			CurrentWaypointNode.Value);

		if (!delayTargetDirection)
			GameInfo.CameraController.TargetDirection = -cameraDirection;
		delayTargetDirection = false;

		CurrentStateDuration = CalculateStateDuration(positionBefore);
		CurrentStateNormDuration = CalculateStateNormDuration(positionBefore);

		UpdateAnimationClips();
		InvokeEventTimers();

		PlayerInfo.Animator.SetInteger(
			AnimationConstants.Player.ChoiceSeparator,
			(int) TransitionChoice.TravelNext);
	}

	/*
	* Needed to properly exit cutscene and update player animator to do so.
	*/
	private void ExitNode()
	{
		GameInfo.CameraController.TargetDirection = Vector3.zero;
		GameInfo.CameraController.StartGameplay();
		GameInfo.Manager.ReceivingInput.TryReleaseLock(this, GameInput.Full);
		
		PlayerInfo.PhysicsSystem.ForceTouchingFloor();
		PlayerInfo.PhysicsSystem.Animating = false;

		PlayerInfo.Animator.SetTrigger(AnimationConstants.Player.Exit);

		PlayerInfo.Animator.SetInteger(
			AnimationConstants.Player.ChoiceSeparator,
			(int) TransitionChoice.Exit);

		PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
		PlayerInfo.MovementManager.SnapSpeed();
	}
}