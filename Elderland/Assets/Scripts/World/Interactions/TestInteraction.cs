using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TestInteraction : StandardInteraction 
{
	//Animator of interaction object, not player
	private Animator objectAnimator;

	private void Start()
	{
		objectAnimator = GetComponent<Animator>();
	}
}
