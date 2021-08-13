using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Combat mechanic that slows the player over time, reaching a cap slow speed. 
// Meant to be an optional area the player can walk on yet will have debuff.
public class CorruptionSludge : MonoBehaviour
{
    [SerializeField]
    private float timeBetweenSlows;
    [SerializeField]
    private float individualSlowAmount;
    [SerializeField]
    private float maximumNumberOfSlows;

    private float timer;
    private bool touchingPlayer;

    private float currentNumberOfSlows;

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
            if (timer > timeBetweenSlows)
            {
                timer = 0;
                ApplySlow();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.PlayerHitbox)
        {
            touchingPlayer = true;
            timer = 0;
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

    private void ApplySlow()
    {
        if (currentNumberOfSlows < maximumNumberOfSlows)
        {
            if (currentNumberOfSlows == 0)
            {
                PlayerInfo.MovementManager.SprintAvailable.ClaimLock(this, false);
            }

            currentNumberOfSlows++;
            PlayerInfo.StatsManager.MovespeedMultiplier.AddModifier(individualSlowAmount);
        }
    }

    private void RemoveSlows()
    {
        for (int i = 0; i < currentNumberOfSlows; i++)
        {
            PlayerInfo.StatsManager.MovespeedMultiplier.RemoveModifier(individualSlowAmount);
        }
        currentNumberOfSlows = 0;
        PlayerInfo.MovementManager.SprintAvailable.TryReleaseLock(this, true);
    }
}
