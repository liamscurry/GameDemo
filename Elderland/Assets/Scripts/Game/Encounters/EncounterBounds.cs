using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Helper class that checks if the player has reached the encounter's boundaries, recycling
// the enemies if so.
public class EncounterBounds : MonoBehaviour
{
    private Encounter encounter;

    private void Awake()
    {
        encounter = GetComponentInParent<Encounter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerHealth")
        {
            encounter.Reset();
        }
    }
}