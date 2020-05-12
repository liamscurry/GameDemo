using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Must call Update Abilities in update and FixedUpdateAbilities in fixedupdate every frame.
//Furthermore, the implementation of UpdateAbilities must call UpdateAbility on each of its abilities every.
public abstract class AbilitySystem 
{
    protected Ability currentAbility;
    protected AnimatorOverrideController controller;
	protected List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;

    public Animator Animator { get; private set; }
    public PhysicsSystem Physics { get; private set; }
    public MovementSystem Movement { get; private set; }
    public GameObject Parent { get; private set; }
    public Ability CurrentAbility { get { return currentAbility; } set { currentAbility = value; } }
	public int CurrentSegmentIndex { get; private set; }

    public AbilitySystem(Animator animator, PhysicsSystem physics, MovementSystem movement, GameObject parent)
    {
        Animator = animator;
        Physics = physics;
        Movement = movement;
        Parent = parent;

        controller = new AnimatorOverrideController(Animator.runtimeAnimatorController);
		Animator.runtimeAnimatorController = controller;
    }

	public abstract void UpdateAbilities();

    public virtual void FixedUpdateAbilities()
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.FixedUpdateAbility();
        }
    }

    public virtual bool Ready()
    {
        return Physics.TouchingFloor;
    }
    
    public void ResetSegmentIndex()
	{
		CurrentSegmentIndex = -1;
	}

	public void SetNextSegmentClip(AnimationClip nextClip)
	{
		CurrentSegmentIndex = ((CurrentSegmentIndex + 1) % 3);

		overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
		controller.GetOverrides(overrideClips);

		int index = overrideClips.FindIndex(clip => clip.Key.name == ("Segment" + (CurrentSegmentIndex + 1)));

		if (index != -1)
		{
			overrideClips[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrideClips[index].Key, nextClip);
		}

		if (overrideClips.Count != 0)
			controller.ApplyOverrides(overrideClips);
	}
}