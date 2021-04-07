using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Implement a queue based match target that is consumed by the target match behaviour (1 dequeue per state). 

public class PlayerAnimationManager 
{
	public Queue<MatchTarget> matchTargets;

	public StateMachineBehaviour AnimationPhysicsBehaviour { get; set; }
	public StateMachineBehaviour KinematicBehaviour { get; set; }
	public bool IgnoreFallingAnimation { get; set; }
	public bool Interuptable { get; set; }

	private AnimationClip[] playerAnims;

	private Coroutine directTargetCorou;

	public PlayerAnimationManager()
	{
		playerAnims = Resources.LoadAll<AnimationClip>(ResourceConstants.Player.Art.Model);
		matchTargets = new Queue<MatchTarget>();
		Interuptable = true;
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
		if (PlayerInfo.Sensor.Interaction != null && GameInfo.Manager.ReceivingInput)
		{
			float angle = 
				Matho.AngleBetween(
					Matho.StandardProjection2D(GameInfo.CameraController.Direction),
					Matho.StandardProjection2D(PlayerInfo.Sensor.Interaction.ValidityDirection));

			if (angle < 75 &&
			 	(Input.GetKeyDown(GameInfo.Settings.UseKey) ||
				 PlayerInfo.Sensor.Interaction.Access == StandardInteraction.AccessType.Trigger) &&
				PlayerInfo.AbilityManager.CurrentAbility == null &&
				!PlayerInfo.TeleportingThisFrame)
			{
				PlayerInfo.Sensor.Interaction.Exit();
			}
		}

		PlayerInfo.TeleportingThisFrame = false;
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
		while (currentTime < animDuration)
		{
			float percentage = currentTime / animDuration;
			PlayerInfo.Player.transform.position =
				startPosition * (1 - percentage) + target.position * percentage;
			PlayerInfo.Player.transform.rotation = 
				Quaternion.Lerp(startRotation, target.rotation, percentage);
			float deltaTimeStart = Time.time;
			yield return new WaitForEndOfFrame();
			currentTime += Time.time - deltaTimeStart;
		}

		PlayerInfo.Player.transform.position = target.position;
		PlayerInfo.Player.transform.rotation = target.rotation;
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