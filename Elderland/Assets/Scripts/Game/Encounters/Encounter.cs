using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
Bounds setup instructions:
Make the trigger bounds (start encounter bounds) be solid in the middle, inside the borders.
This way enemies will spawn if inside the encounter and they enemmies recycled themselves already.
*/
public class Encounter : MonoBehaviour
{
    public static readonly float RecycleDistance = 10f;
    public static readonly float RecycleDuration = 3.5f;
    public static readonly float EngageStartDistance = 3f;
    public static readonly float EngageEnemyDistance = 6f;
    public static readonly float BoundsWaitDistance = 0.1f;
    public static readonly float BoundsWaitRotSpeed = 3f;

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

    /*
    Sends an event to each enemy in the encounter to return to their spawn (if still alive and 
    the enemy is movable)

    Inputs:
    None

    Outputs:
    None
    */
    public void BoundsReturn()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.isActiveAndEnabled)
            {
                enemy.TryBoundsReturn();
            }
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
