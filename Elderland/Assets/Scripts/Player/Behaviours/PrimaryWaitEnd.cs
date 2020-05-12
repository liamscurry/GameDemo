using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryWaitEnd : StateMachineBehaviour 
{
	private Ability ability { get { return PlayerInfo.AbilityManager.Melee; } }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//ability.WaitEnd();
	}
}
