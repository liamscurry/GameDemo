using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbBaseBehaviour : StateMachineBehaviour 
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        PlayerInfo.PhysicsSystem.TotalZero(true, true, true);
	}
}
