using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAbilityBehaviour : StateMachineBehaviour 
{
    private EnemyAbilityManager abilityManager;
	private Ability ability;
	private AbilitySegment segment;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (abilityManager == null)
		    abilityManager = animator.transform.parent.GetComponentInParent<EnemyManager>().AbilityManager;

        ability = abilityManager.CurrentAbility;
		ability.StartSegmentCoroutine();
		segment = ability.ActiveSegment;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if (!segment.Finished)
		{
			ability.StartFixed();
			if (ability.ActiveProcess.Update != null)
				ability.ActiveProcess.Update();
		}

		if (!segment.Finished)
			ability.GlobalUpdate();
	}
}