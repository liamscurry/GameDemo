using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrainShocked : StateMachineBehaviour
{
    private EnemyManager manager;
    private bool exiting;

    private const float damageDuration = 0.5f;
    private float damageTimer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<EnemyManager>();
        }

        exiting = false;

        damageTimer = damageDuration;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer > damageDuration)
            {
                damageTimer = 0;
                manager.ChangeHealth(-PlayerDrain.damage);
                PlayerInfo.Manager.ChangeHealth(PlayerDrain.damage * 2f);
            }

            if (!exiting)
                EndTransition();
        }
    }

    private void EndTransition()
    {
        
    }
}
