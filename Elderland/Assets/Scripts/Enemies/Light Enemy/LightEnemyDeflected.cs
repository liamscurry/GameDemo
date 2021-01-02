using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemyDeflected : StateMachineBehaviour
{
    private LightEnemyManager manager;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<LightEnemyManager>();
        }

        Vector3 direction = manager.transform.position - PlayerInfo.Player.transform.position;
        manager.Push(direction * 1.4f);
        manager.IncreaseResolve(1);
    }
}
