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

	public PlayerAnimationManager()
	{
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

			if (angle < 45 && Input.GetKeyDown(GameInfo.Settings.UseKey) && PlayerInfo.AbilityManager.CurrentAbility == null)
			{
				PlayerInfo.Sensor.Interaction.Exit();
			}
		}
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
			AnimatorTransitionInfo transitionInfo = PlayerInfo.Animator.GetAnimatorTransitionInfo(0);
			float offset = transitionInfo.duration * (1 - transitionInfo.normalizedTime);
			yield return new WaitForSeconds(offset);
		}

		yield return new WaitForFixedUpdate();

		MatchTargetWeightMask mask = new MatchTargetWeightMask(target.positionWeight, target.rotationWeight);
		float startTime = PlayerInfo.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (target.startTime > startTime)
			startTime = target.startTime;

		PlayerInfo.Animator.MatchTarget(target.position, target.rotation, AvatarTarget.Root, mask, startTime, target.endTime);
		PlayerInfo.Manager.currentTargetPosition = target.position;
	}

	/* 
	public void SetInteractionAnimation(AnimationClip interactionClip)
	{
		controller["Interaction"] = interactionClip;
	}
	*/

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
			this.startTime = Mathf.Clamp01(startTime);
			this.endTime = Mathf.Clamp01(endTime);
		}
	}
}