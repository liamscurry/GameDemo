using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemySpawn : StateMachineBehaviour
{
    private bool exiting;
    private GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        manager = animator.GetComponentInParent<GruntEnemyManager>();
        manager.SpawnParticles.Play();
        manager.BehaviourLock = this;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            animator.SetTrigger("toAttack");
            exiting = true;
            manager.ClampToGround();
        }
	}
}
