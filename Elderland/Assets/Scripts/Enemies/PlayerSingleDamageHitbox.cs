using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSingleDamageHitbox : MonoBehaviour
{
    [SerializeField]
    private GameObject display;

    private bool hit;
    private PlayerAbility ability;
    private Collider specificEnemy;

    public GameObject Display { get { return display; } }

    public void Invoke(PlayerAbility ability, Collider specificEnemy = null)
    {
        Reset();
        this.ability = ability;
        this.specificEnemy = specificEnemy;
    }

    private void Reset()
    {
        hit = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "EnemyHealth" && !hit && (specificEnemy == null || other == specificEnemy))
        {
            hit = true;
            ability.OnHit(other.transform.parent.gameObject);
        }
    }
}