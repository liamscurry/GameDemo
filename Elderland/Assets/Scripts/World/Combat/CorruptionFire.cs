using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Combat mechanic that damages the player over time. 
// Meant to be an optional area the player can walk on yet will take damage.

// 8.11.21 Must use player health tag for collision detection as we are only concerned with the players
// hitbox on whether the player is touching the fire.
public class CorruptionFire : MonoBehaviour
{
    [SerializeField]
    private float timeBetweenDamage;
    [SerializeField]
    private float damageAmount;
    [SerializeField]
    private float slowAmount;

    private float timer;
    private bool touchingPlayer;
    private bool slowingPlayer;

    public void DisableSelf()
    {
        timer = 0;
        RemoveSlows();
    }

    private void Update()
    {
        if (touchingPlayer)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenDamage)
            {
                timer = 0;
                Damage();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.PlayerHitbox)
        {
            touchingPlayer = true;
            timer = 0;
            ApplySlow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TagConstants.PlayerHitbox)
        {
            touchingPlayer = false;
            RemoveSlows();
        }
    }

    private void Damage()
    {
        PlayerInfo.Manager.ChangeHealth(-damageAmount);
    }

    private void ApplySlow()
    {
        if (!slowingPlayer)
        {
            slowingPlayer = true;
            PlayerInfo.StatsManager.MovespeedMultiplier.AddModifier(slowAmount);
        }
    }

    private void RemoveSlows()
    {
        if (slowingPlayer)
        {
            slowingPlayer = false;
            PlayerInfo.StatsManager.MovespeedMultiplier.RemoveModifier(slowAmount);
        }
    }
}
