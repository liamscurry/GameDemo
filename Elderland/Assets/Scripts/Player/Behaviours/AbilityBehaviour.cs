using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateMachineBehaviour incorporated.
public class AbilityBehaviour : PlayerStateMachineBehaviour 
{
	private Ability ability;
	private AbilitySegment segment;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (PlayerInfo.AbilityManager.CurrentAbility != null)
		{
			ability = PlayerInfo.AbilityManager.CurrentAbility;
			if (ability == null)
				throw new System.Exception("No current ability specified during player ability behaviour");

			ability.StartSegmentCoroutine();

			segment = ability.ActiveSegment;
			if (segment == null)
				throw new System.Exception("No active segment for ability during player ability behaviour");
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!Exiting)
		{
			if (PlayerInfo.AbilityManager.CurrentAbility != null)
			{
				if (!segment.Finished)
				{
					ability.StartFixed();
					if (ability.ActiveProcess.Update != null &&
						(!ability.ActiveProcess.Indefinite || !ability.ActiveProcess.IndefiniteFinished))
						ability.ActiveProcess.Update();
						
					ability.GlobalUpdate();
				}
				else
				{
					// Ability segment finished as normal.
					Exiting = true;
				}
			}
			else
			{
				// Ability must have been assigned to be called, could be done with ability.
				Exiting = true;
			}
		}
	}
}