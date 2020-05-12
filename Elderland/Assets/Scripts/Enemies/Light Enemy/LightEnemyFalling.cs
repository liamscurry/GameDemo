using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemyFalling : StateMachineBehaviour
{
    private LightEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<LightEnemyManager>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((manager.PhysicsSystem.EnteredFloor || manager.PhysicsSystem.TouchingFloor) && !animator.IsInTransition(0))
		{
			if (manager.PhysicsSystem.LastCalculatedVelocity.y < -15)
			{
				animator.SetFloat("speed", 1);
			}
			else
			{
				animator.SetFloat("speed", 0);
			}

			animator.SetBool("falling", false);
		}	
    }
}
