using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cannot use center field of colliders. Must edit location by its local transform.
// Only multi damage hitbox supports overlap check on deactivation as of right now.
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
        if (other.tag == "EnemyHealth" &&
            !hit &&
            (specificEnemy == null ||other == specificEnemy) &&
            gameObject.activeInHierarchy &&
            other.gameObject.activeInHierarchy &&
            !CheckForObstruction(other))
        {
            hit = true;
            ability.OnHit(other.transform.parent.gameObject);
        }
    }

    private bool CheckForObstruction(Collider other)
    {
        return Physics.Linecast(transform.position, other.transform.position, LayerConstants.GroundCollision);
    }
}