using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBehaviour : StateMachineBehaviour 
{
	private Ability ability;
	private AbilitySegment segment;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (PlayerInfo.AbilityManager.CurrentAbility != null)
		{
			if (PlayerInfo.Animator.isMatchingTarget)
				PlayerInfo.Animator.InterruptMatchTarget(false);

			ability = PlayerInfo.AbilityManager.CurrentAbility;
			ability.StartSegmentCoroutine();
			segment = ability.ActiveSegment;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (PlayerInfo.AbilityManager.CurrentAbility != null)
		{
			if (!segment.Finished)
			{
				ability.StartFixed();
				if (ability.ActiveProcess.Update != null)
					ability.ActiveProcess.Update();
					
				ability.GlobalUpdate();
			}
		}
	}
}