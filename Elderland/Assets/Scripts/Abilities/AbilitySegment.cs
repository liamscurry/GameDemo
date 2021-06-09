using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySegment 
{
	public AnimationClip Clip { get; set; }
	public AnimationClip UpperClip { get; set; }
	public AbilityProcess[] Processes { get; set; }
	public AbilitySegment Next { get; set; }
	public AbilitySegmentType Type { get; set; }
	public float LoopFactor { get; set; }
	public float NormalizedDuration { get; private set; }
	public bool Finished { get; set; }

	public AbilitySegment(AnimationClip clip, params AbilityProcess[] processes)
	{
		Clip = clip;
		UpperClip = null;
		Processes = processes;
		Next = null;
		Type = AbilitySegmentType.Normal;
		LoopFactor = 1;

		NormalizedDuration = CalculateNormalizedDuration();

		Finished = false;
	}
	
	public void Normalize()
	{
		if (NormalizedDuration == 0)
		{
			throw new System.ArgumentException("Ability segment cannot have a time length of zero");
		}
		else if (NormalizedDuration > 1)
		{
			float normalizationScale = 1 / NormalizedDuration;

			for (int i = 0; i < Processes.Length; i++)
			{
				Processes[i].Duration *= normalizationScale;
			}
		}
	}

	private float CalculateNormalizedDuration()
	{
		float netDuration = 0;
		for (int i = 0; i < Processes.Length; i++)
		{
			netDuration += Processes[i].Duration;
		}
		return netDuration;
	}
}