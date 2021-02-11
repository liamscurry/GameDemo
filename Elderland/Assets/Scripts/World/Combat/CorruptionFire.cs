using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Combat mechanic that damages the player over time. 
// Meant to be an optional area the player can walk on yet will take damage.
public class CorruptionFire : LevelMechanic
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

    public override void InvokeSelf()
    {
        
    }

    public override void ResetSelf()
    {
        timer = 0;
        RemoveSlows();
    }

    public override void DisableSelf()
    {
        ResetSelf();
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
            ApplySlow();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.Player)
        {
            touchingPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TagConstants.Player)
        {
            touchingPlayer = false;
            timer = 0;
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
