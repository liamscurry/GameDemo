using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyWatchFollow : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    private float checkTimer;
    private float checkDuration = 0.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        manager = animator.GetComponentInParent<RangedEnemyManager>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        checkTimer += Time.deltaTime;

        if (manager.IsSubscribeToAttackValid())
        {
            manager.SubscribeToAttack();
        } 
        else if (checkTimer >= checkDuration)
        {
            CheckForOverride();
        }

        manager.ClampToGround();

        if (checkTimer >= checkDuration)
            checkTimer = 0;
	}

    private void CheckForOverride()
    {
        EnemyManager otherManager = manager.FindOverrideAttacker();
        if (otherManager != null)
        {
            manager.OverrideAttacker(otherManager);
        }
    }
}
