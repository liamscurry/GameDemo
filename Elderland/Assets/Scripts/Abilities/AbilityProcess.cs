using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityProcess 
{
	public readonly Action Begin;
	public readonly Action Update;
	public readonly Action End;
	public float Duration { get; set; }
	public readonly bool Indefinite;
	public bool IndefiniteFinished { get; set; }
	
	public AbilityProcess(Action beginingMethod, Action updateMethod, Action endMethod, float normalizedDuration, bool indefinite = false)
	{
		Begin = beginingMethod;
		Update = updateMethod;
		End = endMethod;
		Duration = normalizedDuration;
		if (Duration < 0.1f)
			Duration = 0.1f;
		Indefinite = indefinite;
		IndefiniteFinished = false;
	}
}
