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
    private List<EnemyManager> spawnedEnemies;

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
        spawnedEnemies = new List<EnemyManager>();
    }

    public void Spawn()
    {
        foreach (EncounterSpawner spawner in spawners)
        {
            spawnedEnemies.AddRange(spawner.Spawn());
        }
    }

    public void Reset()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.isActiveAndEnabled)
            {
                enemy.Recycle();
            }
        }

        spawnedEnemies.Clear();

        foreach (var spawner in spawners)
        {
            spawner.Reset();
        }
    }
}
