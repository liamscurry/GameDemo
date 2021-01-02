using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyEnemyDeflected : StateMachineBehaviour
{
    private HeavyEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<HeavyEnemyManager>();
        }

        Vector3 direction = manager.transform.position - PlayerInfo.Player.transform.position;
        manager.Zero();
        manager.Push(direction * 1.4f);
        manager.IncreaseResolve(1);
    }
}
