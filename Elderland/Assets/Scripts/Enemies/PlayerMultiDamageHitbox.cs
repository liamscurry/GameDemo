using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMultiDamageHitbox : MonoBehaviour
{
    [SerializeField]
    private GameObject display;

    private List<Collider> enemiesHit;
    private PlayerAbility ability;

    private bool callOnHit;
    private bool callOnStay;

    public GameObject Display { get { return display; } }

    private void Awake()
    {
        enemiesHit = new List<Collider>();
    }

    public void Invoke(PlayerAbility ability, bool callOnHit = true, bool callOnStay = false)
    {
        Reset();
        this.ability = ability;
        this.callOnHit = callOnHit;
        this.callOnStay = callOnStay;
    }

    private void Reset()
    {
        enemiesHit.Clear();
    }

    public void Deactivate()
    {
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
        if (other.tag == "EnemyHealth" && gameObject.activeSelf)
        {
            if (!enemiesHit.Contains(other))
            {
                if ((callOnHit && ability.OnHit(other.transform.parent.gameObject)) || (!callOnHit))
                    enemiesHit.Add(other);
            }
            else if (callOnStay)
            {
                ability.OnStay(other.transform.parent.gameObject);
            }
        }
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