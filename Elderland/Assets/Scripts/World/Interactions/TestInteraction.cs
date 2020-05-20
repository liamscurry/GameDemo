using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TestInteraction : StandardInteraction 
{
	[SerializeField]
	private AnimationClip idleClip;
	[SerializeField]
	private AnimationClip activateClip;
	[SerializeField]
	private AnimationClip activatedClip;

	//Animator of interaction object, not player
	private Animator objectAnimator;

	private AnimatorOverrideController controller;
	private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;

	private void Start()
	{
		objectAnimator = GetComponent<Animator>();
		controller = new AnimatorOverrideController(objectAnimator.runtimeAnimatorController);
		objectAnimator.runtimeAnimatorController = controller;
		SetAnimationClip("InteractionBaseIdle", idleClip);
		SetAnimationClip("InteractionBaseActivate", activateClip);
		SetAnimationClip("InteractionBaseActivated", activatedClip);
	}

	public override void Reset()
	{
		activated = false;
		ui.SetActive(true);
	}

	protected override void OnExitBegin()
	{
		objectAnimator.SetTrigger("activated");
	}

	private void SetAnimationClip(string stateName, AnimationClip newClip)
	{
		overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
		controller.GetOverrides(overrideClips);

		int index = overrideClips.FindIndex(clip => clip.Key.name == stateName);

		if (index != -1)
		{
			overrideClips[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrideClips[index].Key, newClip);
		}

		if (overrideClips.Count != 0)
			controller.ApplyOverrides(overrideClips);
	}

}
