using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Encounter : MonoBehaviour
{
    public static readonly float RecycleDistance = 10f;
    public static readonly float RecycleDuration = 3.5f;
    public static readonly float EngageStartDistance = 5f;
    public static readonly float EngageEnemyDistance = 6f;

    private EncounterSpawner[] spawners;

    /*public int Count
    {
        get
        {
            int count = 0;
            foreach (EnemyWaveSpawner spawner in spawners)
            {
                count += spawner.Count;
            }

            return count;
        }
    }*/

    public void Awake()
    {
        spawners = GetComponentsInChildren<EncounterSpawner>();
    }

    public void Spawn()
    {
        foreach (EncounterSpawner spawner in spawners)
        {
            spawner.Spawn();
        }
    }
}
