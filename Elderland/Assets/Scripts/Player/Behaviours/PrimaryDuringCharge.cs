using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryDuringCharge : StateMachineBehaviour 
{
	private Ability ability { get { return PlayerInfo.AbilityManager.CurrentAbility; } }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//ability.StartChargeCoroutine(stateInfo.length * ((1 * ability.ClipMultiplier) - stateInfo.normalizedTime));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//ability.DuringCharge();
	}
}