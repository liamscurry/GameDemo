using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Construct needed to streamline arbitrary animation loop clip assignment.
// Ex: abilty system and gameplay cutscenes.
public class AnimationLoop
{
    // Fields
    private AnimatorOverrideController controller;
    private Animator animator;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;
    private string animationName;

    // Properties
    public Animator Animator { get; private set; }
    public int CurrentSegmentIndex { get; private set; }

    public AnimationLoop(
        AnimatorOverrideController controller,
        Animator animator,
        string animationName)
    {
        this.controller = controller;
        this.animator = animator;
        this.animationName = animationName;
    }

    public void ResetSegmentIndex()
    {
        CurrentSegmentIndex = -1;
    }
    
    public void SetNextSegmentClip(AnimationClip nextClip)
    {
        CurrentSegmentIndex = ((CurrentSegmentIndex + 1) % 3);

        overrideClips =
            new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
        controller.GetOverrides(overrideClips);

        int index =
            overrideClips.FindIndex(
                clip => clip.Key.name == (animationName + (CurrentSegmentIndex + 1)));

        if (index != -1)
        {
            overrideClips[index] =
                new KeyValuePair<AnimationClip, AnimationClip>(overrideClips[index].Key, nextClip);
        }

        if (overrideClips.Count != 0)
            controller.ApplyOverrides(overrideClips);
    }

    /*
    * Needed for more complex animation loops that use blend trees.
    * Assigns clips to a state's blend tree/multi-state segment.
    */
    public void SetNextSegmentClip(AnimationClip[] nextClips, string[] nextNames)
    {
        CurrentSegmentIndex = ((CurrentSegmentIndex + 1) % 3);

        overrideClips =
            new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
        controller.GetOverrides(overrideClips);

        for (int i = 0; i < nextClips.Length; i++)
        {
            int index =
                overrideClips.FindIndex(
                    clip => clip.Key.name == (nextNames[i] + (CurrentSegmentIndex + 1)));

            if (index != -1)
            {
                overrideClips[index] =
                    new KeyValuePair<AnimationClip, AnimationClip>(
                        overrideClips[index].Key, nextClips[i]);
            }
        }

        if (overrideClips.Count != 0)
            controller.ApplyOverrides(overrideClips);
    }
}