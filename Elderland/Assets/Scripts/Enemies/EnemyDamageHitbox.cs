using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageHitbox : MonoBehaviour
{
    private bool hit;
    private EnemyAbility ability;

    public void Invoke(EnemyAbility ability)
    {
        Reset();
        this.ability = ability;
    }

    private void Reset()
    {
        hit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerHealth" && !hit)
        {
            hit = true;
            ability.OnHit(other.transform.parent.gameObject);
        }
    }
}
