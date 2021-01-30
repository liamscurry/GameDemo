using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EncounterBounds : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemyHealth")
        {
            EnemyManager enemyManager = other.transform.parent.GetComponent<EnemyManager>();

            if (enemyManager.AbilityManager.CurrentAbility != null)
            {
                enemyManager.AbilityManager.CurrentAbility.ShortCircuit();
            }

            
            enemyManager.Animator.SetTrigger("spawnReturn");
        }
    }
}