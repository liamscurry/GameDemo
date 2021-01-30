using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntEnemyDeflected : StateMachineBehaviour
{
    private GruntEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<GruntEnemyManager>();
        }

        Vector3 direction = manager.transform.position - PlayerInfo.Player.transform.position;
        manager.Push(direction * 1.4f);
        manager.IncreaseResolve(1);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
