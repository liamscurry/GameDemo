using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryDuringAct : StateMachineBehaviour 
{
	private Ability ability;
	private bool exiting;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//ability = PlayerInfo.AbilityManager.CurrentAbility;
		//ability.StartActCoroutine(stateInfo.length * (1 - stateInfo.normalizedTime));
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//if (!ability.FinishedActTimer)
		//	ability.DuringAct();
	}
}