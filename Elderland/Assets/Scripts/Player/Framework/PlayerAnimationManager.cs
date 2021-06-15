using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using System;

//Implement a queue based match target that is consumed by the target match behaviour (1 dequeue per state). 

public class PlayerAnimationManager 
{
	public Queue<MatchTarget> matchTargets;

	public StateMachineBehaviour AnimationPhysicsBehaviour { get; set; }
	public StateMachineBehaviour KinematicBehaviour { get; set; }
	public PlayerAnimationUpper UpperLayer { get; private set; }
	public bool IgnoreFallingAnimation { get; set; }

	private AnimationClip[] playerAnims;
	private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;
	public AnimatorController AnimatorController { get; private set; }

	// Anim layers
	private PlayerAnimationPersistLayer combatLayer;
	private PlayerAnimationPersistLayer walkAbilityLayer;
	public PlayerAnimationPersistLayer WalkAbilityLayer { get { return walkAbilityLayer; } }

	private Coroutine directTargetCorou;

	private bool rotatingModel;

	public const float ModelRotSpeedIdle = 2f;
	public const float ModelRotSpeedMoving = 9f;

	// Walk helper fields
	private Vector2 positionAnalogDirection;
    private Vector2 reverseAnalogDirection;

    private const float positionAnalogSpeed = 1.7f;
    private const float reverseAnalogSpeed = 1.35f;

	private bool movedThisFrame;

	public PlayerAnimationManager()
	{
		playerAnims = Resources.LoadAll<AnimationClip>(ResourceConstants.Player.Art.Model);
		matchTargets = new Queue<MatchTarget>();
		combatLayer = new PlayerAnimationPersistLayer(0.5f, "CombatStanceLayer");
		walkAbilityLayer = new PlayerAnimationPersistLayer(0.5f, "WalkAbilityLayer");
		UpperLayer = new PlayerAnimationUpper();

		rotatingModel = false;
		AnimatorController =
			Resources.Load<AnimatorController>(ResourceConstants.Player.Art.AnimatorController);

		positionAnalogDirection = Vector2.zero;
		movedThisFrame = false;
	}

	public void UpdateAnimations()
	{
        if (!PlayerInfo.Animator.GetBool("falling") &&
			PlayerInfo.PhysicsSystem.ExitedFloor &&
			!PlayerInfo.Animator.GetBool("jump") &&
			!IgnoreFallingAnimation)
        {
			if (PlayerInfo.AbilityManager.CurrentAbility == PlayerInfo.AbilityManager.Dodge ||
				PlayerInfo.AbilityManager.CurrentAbility == PlayerInfo.AbilityManager.Melee)
			{
				PlayerInfo.AbilityManager.CurrentAbility.FallUponFinish();
				PlayerInfo.Animator.SetBool("falling", true);
			}
			else
			{
				//moved shortcut to after triggers/bools, was before before
            	PlayerInfo.Animator.SetBool("falling", true);
				PlayerInfo.Animator.SetTrigger("fall");		
				PlayerInfo.AbilityManager.ShortCircuit(false);
			}
		}
	}

	public void LateUpdateAnimations()
	{
		if (PlayerInfo.Sensor.Interaction != null && GameInfo.Manager.ReceivingInput.Value == GameInput.Full)
		{
			float angle = 
				Matho.AngleBetween(
					Matho.StdProj2D(GameInfo.CameraController.Direction),
					Matho.StdProj2D(PlayerInfo.Sensor.Interaction.ValidityDirection));

			if (angle < 75 &&
			 	(Input.GetKeyDown(GameInfo.Settings.UseKey) ||
				 PlayerInfo.Sensor.Interaction.Access == StandardInteraction.AccessType.Trigger) &&
				PlayerInfo.AbilityManager.CurrentAbility == null &&
				!PlayerInfo.TeleportingThisFrame)
			{
				PlayerInfo.Sensor.Interaction.Invoke();
			}
		}

		PlayerInfo.TeleportingThisFrame = false;

		movedThisFrame = false;
	}

	public void UpdateWalkProperties()
    {
		if (!movedThisFrame)
		{
			movedThisFrame = true;
			Vector2 forwardDir =
				Matho.StdProj2D(PlayerInfo.Player.transform.forward);
			Vector2 rightDir =
				Matho.Rotate(Matho.StdProj2D(PlayerInfo.Player.transform.forward), 90);

			if (forwardDir.magnitude == 0 ||
				rightDir.magnitude == 0 || 
				PlayerInfo.MovementManager.TargetDirection.magnitude == 0 ||
				PlayerInfo.MovementManager.CurrentDirection.magnitude == 0)
				return;

			float speed = 
				PlayerInfo.MovementManager.CurrentPercentileSpeed *
				PlayerInfo.StatsManager.MovespeedMultiplier.Value;

			Vector2 scaledCurrentDir = 
				PlayerInfo.MovementManager.CurrentDirection * PlayerInfo.MovementManager.CurrentPercentileSpeed;
			Vector2 analogDirection = 
				new Vector2(
					Matho.ProjectScalar(scaledCurrentDir, forwardDir),
					Matho.ProjectScalar(scaledCurrentDir, rightDir));

			if (analogDirection.x < 0)
				analogDirection.x *= 3;

			if (analogDirection.x > 0 &&
				Matho.AngleBetween(Vector2.up, new Vector2(analogDirection.y, analogDirection.x)) > 60f)
			{
				analogDirection.x = 0;
			}
			else if (
				analogDirection.x < 0 &&
				Matho.AngleBetween(Vector2.down, new Vector2(analogDirection.y, analogDirection.x)) > 60f)
			{
				analogDirection.x = 0;
			}

			// Geometry check.
			float geometryModifier = 1;
			Vector3 center = PlayerInfo.Player.transform.position;
			center += Vector3.up * PlayerInfo.Capsule.height / 4f;
			Vector3 currentDirection3D = 
				new Vector3(
					PlayerInfo.MovementManager.CurrentDirection.x,
					0,
					PlayerInfo.MovementManager.CurrentDirection.y);

			float sizeSideLength =
				PlayerInfo.Capsule.radius * 0.75f;
			Vector3 size = new Vector3(sizeSideLength, PlayerInfo.Capsule.height / 2f, sizeSideLength * 0.25f);
			Quaternion rotation = Quaternion.LookRotation(currentDirection3D, Vector3.up);
			RaycastHit obstructionHit;
			if (Physics.BoxCast(
				center,
				size, 
				currentDirection3D, 
				out obstructionHit,
				rotation, 
				1f,
				LayerConstants.GroundCollision))
			{
				Vector3 projectedNormal =
					Matho.StandardProjection3D(-obstructionHit.normal);
				if (projectedNormal.magnitude != 0)
				{
					geometryModifier = Matho.AngleBetween(projectedNormal, currentDirection3D) / 180f;
					geometryModifier *= 4f;
					if (geometryModifier > 1)
						geometryModifier = 1;
				}
			}

			analogDirection *= geometryModifier;

			PlayerInfo.MovementManager.PercSpeedObstructedModifier = geometryModifier;

			// Debug geometry check
			/*
			Debug.DrawLine(PlayerInfo.Player.transform.position, center, Color.gray, 3f);
			Debug.DrawLine(center, center + currentDirection3D, Color.gray, 3f);
			Debug.DrawLine(center, center + Vector3.up * size.y / 2, Color.gray, 3f);
			Debug.DrawLine(
				center + Vector3.up * size.y / 2,
				center + Vector3.up * size.y / 2 + Vector3.Cross(Vector3.up, currentDirection3D) * size.x / 2f,
				Color.gray,
				3f);
			*/

			positionAnalogDirection =
				Vector2.MoveTowards(positionAnalogDirection, analogDirection, positionAnalogSpeed * Time.deltaTime);
			reverseAnalogDirection =
				Vector2.MoveTowards(reverseAnalogDirection, analogDirection, reverseAnalogSpeed * Time.deltaTime);

			PlayerInfo.Animator.SetFloat(
				"speed",
				positionAnalogDirection.x);
			PlayerInfo.Animator.SetFloat(
				"strafe",
				positionAnalogDirection.y);
			PlayerInfo.Animator.SetFloat(
				"percentileSpeed",
				PlayerInfo.MovementManager.PercentileSpeed);
		}
    }

	/*
	* Helper function for behaviours to update model rotation to camera forward (or blended if in combat)
	*/
	public void UpdateRotation(bool moving)
    {
        if (!moving)
        {
            UpdateStillModelRotation();
        }
        else
        {
            UpdateMovingModelRotation();
        }
    }

	private void UpdateMovingModelRotation()
	{
		rotatingModel = false;

		Vector3 targetRotation =
			Matho.StandardProjection3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;
		Vector3 currentRotation =
			Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;

		Vector3 incrementedRotation =
			Vector3.RotateTowards(
				currentRotation,
				targetRotation,
				PlayerAnimationManager.ModelRotSpeedMoving * Time.deltaTime,
				0f);
		Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
		PlayerInfo.Player.transform.rotation = rotation;
	}

	private void UpdateStillModelRotation()
	{
		Vector3 targetRotation =
			Matho.StandardProjection3D(PlayerInfo.MovementManager.ModelTargetForward).normalized;
		Vector3 currentRotation =
			Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;

		if (Matho.AngleBetween(targetRotation, currentRotation) > PlayerMovementManager.RotationStartMin)
			rotatingModel = true;  

		if (rotatingModel)
		{
			Vector3 incrementedRotation =
				Vector3.RotateTowards(
					currentRotation,
					targetRotation,
					PlayerAnimationManager.ModelRotSpeedIdle * Time.deltaTime,
					0f);
			Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
			PlayerInfo.Player.transform.rotation = rotation;

			if (Matho.AngleBetween(targetRotation, currentRotation) < PlayerMovementManager.RotationStopMin)
				rotatingModel = false;
		}
	}

	/*
	* The following two methods are helper methods for combat layer that transitions from and to
	* the combat layers when fighting enemies. This is needed to pull out the sword and put it away.
	*/
	public void ToCombatStance()
	{
		combatLayer.TurnOn();
		UpperLayer.RequestAction(
			GetAnim("TakeOutSword"), 
			PlayerInfo.AbilityManager.OnCombatStanceOn,
			PlayerInfo.AbilityManager.ShortCircuitCombatStanceOn);
		GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.Gameplay);
		PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
	}

	public void AwayCombatStance()
	{
		combatLayer.TurnOff();
		UpperLayer.RequestAction(
			GetAnim("PutSwordAway"),
			PlayerInfo.AbilityManager.OnCombatStanceOff,
			PlayerInfo.AbilityManager.ShortCircuitCombatStanceOff);
		GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.Gameplay);
		PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
	}

	public void AwayCombatStanceNoClaim()
	{
		combatLayer.TurnOff();
		UpperLayer.RequestAction(
			GetAnim("PutSwordAway"),
			PlayerInfo.AbilityManager.OnCombatStanceOff,
			PlayerInfo.AbilityManager.ShortCircuitCombatStanceOff);
	}

	/*
	* Helper getter to find animation clips from the player's fbx model.
	*/
	public AnimationClip GetAnim(string key)
	{
		foreach (var anim in playerAnims)
		{
			if (anim.name == key)
				return anim;
		}
		
		throw new System.ArgumentException("Animation Key does not exist in player fbx model");
	}

	/*
    * Helper function for assigning clips to player animator. Only use when overriding one animation.
	* POSS: If need to override multiple clips at a time, make a function SetAnims that loops through an array
	* of names and clips and applies the overrides at the end of them method.
    */
    public void SetAnim(AnimationClip newClip, string genericName)
    {
        overrideClips =
            new List<KeyValuePair<AnimationClip, AnimationClip>>(PlayerInfo.Controller.overridesCount);
        PlayerInfo.Controller.GetOverrides(overrideClips);
        
		int index =
			overrideClips.FindIndex(
				clip => clip.Key.name == genericName);

		if (index != -1)
		{
			overrideClips[index] =
				new KeyValuePair<AnimationClip, AnimationClip>(
					overrideClips[index].Key, newClip);
		}
     
        if (overrideClips.Count != 0)
            PlayerInfo.Controller.ApplyOverrides(overrideClips);
    }

	public void EnqueueTarget(MatchTarget target)
	{
		matchTargets.Enqueue(target);
	}

	public void StartTarget()
	{
		PlayerInfo.Manager.StartCoroutine(CoUpdateTargetMatch(matchTargets.Dequeue()));
	}

	public void StartTarget(MatchTarget target)
	{
		PlayerInfo.Manager.StartCoroutine(CoUpdateTargetMatch(target));
	}

	public void StartTargetImmediately(MatchTarget target)
	{
		MatchTargetWeightMask mask = new MatchTargetWeightMask(target.positionWeight, target.rotationWeight);
		float startTime = PlayerInfo.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (target.startTime > startTime)
			startTime = target.startTime;

		PlayerInfo.Animator.MatchTarget(target.position, target.rotation, AvatarTarget.Root, mask, startTime, target.endTime);
		PlayerInfo.Manager.currentTargetPosition = target.position;
	}

	public IEnumerator CoUpdateTargetMatch(MatchTarget target)
	{
		if (PlayerInfo.Animator.IsInTransition(0))
		{
			yield return new WaitUntil(() => !PlayerInfo.Animator.IsInTransition(0));
		}

		yield return new WaitForFixedUpdate();
	
		// essentially, the transition duration is a percentage of the current state, not the target state.
		// thus when the current state is longer than the target state, it  doesn't get called until the state is over.

		MatchTargetWeightMask mask = new MatchTargetWeightMask(target.positionWeight, target.rotationWeight);
		float startTime = PlayerInfo.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (target.startTime > startTime)
			startTime = target.startTime;

		PlayerInfo.Animator.MatchTarget(
			target.position,
			target.rotation,
			AvatarTarget.Root,
			mask,
			startTime, 
			target.endTime);
		PlayerInfo.Manager.currentTargetPosition = target.position;
	}

	/* 
	public void SetInteractionAnimation(AnimationClip interactionClip)
	{
		controller["Interaction"] = interactionClip;
	}
	*/
	
	/*
	* Direct target methods are for abilities. These are custom target coroutines that allow for
	* movement even when the player animator is in a transition.
	*/
	public void StartDirectTarget(MatchTarget target)
	{
		if (directTargetCorou != null)
			PlayerInfo.Manager.StopCoroutine(directTargetCorou);
		
		directTargetCorou = PlayerInfo.Manager.StartCoroutine(DirectTargetCoroutine(target));
	}

	private IEnumerator DirectTargetCoroutine(MatchTarget target)
	{
		float startTime = 0;
		float animDuration = 0;
		if (PlayerInfo.Animator.IsInTransition(0))
		{
			var nextState = 
				PlayerInfo.Animator.GetNextAnimatorStateInfo(0);
			var nextClip = 
				PlayerInfo.Animator.GetNextAnimatorClipInfo(0);
			startTime = nextState.normalizedTime;
			animDuration = nextClip[0].clip.length;
		}
		else
		{
			var currentState = 
				PlayerInfo.Animator.GetCurrentAnimatorStateInfo(0);
			var currentClip = 
				PlayerInfo.Animator.GetCurrentAnimatorClipInfo(0);
			startTime = currentState.normalizedTime;
			animDuration = currentClip[0].clip.length;
		}
		
		float currentTime = startTime * animDuration;
		Vector3 startPosition = PlayerInfo.Player.transform.position;
		Quaternion startRotation = PlayerInfo.Player.transform.rotation;
		Vector3 percTargetPos =
			startPosition * (1 - target.positionWeight.x) + target.position * target.positionWeight.x;
		Quaternion percTargetRot =
			Quaternion.Lerp(startRotation, target.rotation, target.rotationWeight);
		while (currentTime < animDuration)
		{
			float percentage = currentTime / animDuration;
			PlayerInfo.Player.transform.position =
				startPosition * (1 - percentage) + percTargetPos * percentage;
			PlayerInfo.Player.transform.rotation = 
				Quaternion.Lerp(startRotation, percTargetRot, percentage);
			float deltaTimeStart = Time.time;
			yield return new WaitForEndOfFrame();
			currentTime += Time.time - deltaTimeStart;
		}

		PlayerInfo.Player.transform.position = percTargetPos;
		PlayerInfo.Player.transform.rotation = percTargetRot;
		directTargetCorou = null;
	}

	public void KinematicEnable()
	{
		PlayerInfo.Body.isKinematic = true;
		PlayerInfo.Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		PlayerInfo.Animator.applyRootMotion = true;
	}

	public void KinematicDisable()
	{
		PlayerInfo.Body.isKinematic = false;
		PlayerInfo.Animator.updateMode = AnimatorUpdateMode.Normal;
		PlayerInfo.Animator.applyRootMotion = false;
	}

	public void AnimationPhysicsEnable()
	{
		PlayerInfo.PhysicsSystem.Animating = true;
	}

	public void AnimationPhysicsDisable()
	{
		PlayerInfo.PhysicsSystem.Animating = false;
	}

	[System.Serializable]
	public struct MatchTarget
	{
		[SerializeField]
		public Vector3 position;
		[SerializeField]
		public Quaternion rotation;
		[SerializeField]
		public readonly AvatarTarget avatarTarget;
		[SerializeField]
		public Vector3 positionWeight;
		[SerializeField]
		public readonly float rotationWeight;
		[SerializeField]
		public readonly float startTime;
		[SerializeField]
		public readonly float endTime;

		public MatchTarget(Vector3 position, Quaternion rotation, AvatarTarget avatarTarget, Vector3 positionWeight, float rotationWeight, float startTime = 0, float endTime = 1)
		{
			this.position = position;
			this.rotation = rotation;
			this.avatarTarget = avatarTarget;
			this.positionWeight = positionWeight;
			this.rotationWeight = rotationWeight;
			this.startTime = startTime;
			this.endTime = endTime;
		}
	}
}