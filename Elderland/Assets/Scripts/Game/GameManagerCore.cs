using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper core class similar to PlayerManager core. See class for details.
public class GameManagerCore : MonoBehaviour
{
    public void ResetAllEncounters()
    {
        Encounter[] encounters = Object.FindObjectsOfType<Encounter>();
        foreach (var encounter in encounters)
        {
            encounter.Reset();
        }
    }
}
