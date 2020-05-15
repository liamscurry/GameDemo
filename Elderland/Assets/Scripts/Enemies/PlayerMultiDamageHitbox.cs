using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMultiDamageHitbox : MonoBehaviour
{
    [SerializeField]
    private GameObject display;

    private List<Collider> enemiesHit;
    private PlayerAbility ability;

    public GameObject Display { get { return display; } }

    private void Awake()
    {
        enemiesHit = new List<Collider>();
    }

    public void Activate(PlayerAbility ability)
    {
        Reset();
        this.ability = ability;
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
        if (other.tag == "EnemyHealth" && !enemiesHit.Contains(other) && gameObject.activeSelf)
        {
            if (ability.OnHit(other.transform.parent.gameObject))
                enemiesHit.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemyHealth" && enemiesHit.Contains(other))
        {
            enemiesHit.Remove(other);
            ability.OnLeave(other.transform.parent.gameObject);
        }
    }
}