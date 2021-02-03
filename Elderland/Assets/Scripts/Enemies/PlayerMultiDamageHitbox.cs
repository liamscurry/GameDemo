using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cannot use center field of colliders. Must edit location by its local transform.
// Only multi damage hitbox supports overlap check on deactivation as of right now.
public class PlayerMultiDamageHitbox : MonoBehaviour
{
    [SerializeField]
    private GameObject display;
    [SerializeField]
    private bool checkBoxOnDisable;

    private List<Collider> enemiesHit;
    private PlayerAbility ability;

    private bool callOnHit;
    private bool callOnStay;
    //private bool disableNextFrame;

    public GameObject Display { get { return display; } }

    private void Awake()
    {
        enemiesHit = new List<Collider>();
    }

    /*private void FixedUpdate()
    {
        if (disableNextFrame)
        {
            Deactivate();
            gameObject.SetActive(false);
            disableNextFrame = false;
        }
    }*/

    public void Invoke(PlayerAbility ability, bool callOnHit = true, bool callOnStay = false)
    {
        Reset();
        this.ability = ability;
        this.callOnHit = callOnHit;
        this.callOnStay = callOnStay;
    }

    /*public void DisableNextFrame()
    {
        disableNextFrame = true;
    }*/

    private void Reset()
    {
        enemiesHit.Clear();
    }

    public void Deactivate()
    {
        // Check for overlap
        if (checkBoxOnDisable)
        {
            BoxCollider boxCollider = 
                GetComponent<BoxCollider>();

            Collider[] overlappingColliders =
                Physics.OverlapBox(
                    boxCollider.transform.position + boxCollider.center,
                    boxCollider.size,
                    boxCollider.transform.rotation);
            foreach (Collider overlappingCollider in overlappingColliders)
            {
                TestCollider(overlappingCollider);
            }
        }

        foreach (Collider enemy in enemiesHit)
        {
            if (enemy != null)
            {
                ability.OnLeave(enemy.transform.parent.gameObject);
            }
        }
        enemiesHit.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        TestCollider(other);
    }

    private void TestCollider(Collider other)
    {
        if (other.tag == "EnemyHealth" &&
            (gameObject.activeInHierarchy) &&
            other.gameObject.activeInHierarchy)
        {
            if (!enemiesHit.Contains(other))
            {
                if ((callOnHit && !CheckForObstruction(other) && ability.OnHit(other.transform.parent.gameObject)) || (!callOnHit))
                    enemiesHit.Add(other);
            }
            else if (callOnStay && !CheckForObstruction(other))
            {
                ability.OnStay(other.transform.parent.gameObject);
            }
        }
    }

    private bool CheckForObstruction(Collider other)
    {
        return Physics.Linecast(transform.position, other.transform.position, LayerConstants.GroundCollision);
    }

    /*
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemyHealth" && enemiesHit.Contains(other))
        {
            enemiesHit.Remove(other);
            ability.OnLeave(other.transform.parent.gameObject);
        }
    }*/ //may need to have this for firecharge abilities, will have to look into supporting both.
}