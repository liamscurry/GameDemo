using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour
{
    [SerializeField]
    private UnityEvent completionEvent;

    private EnemyWaveSpawner[] spawners;

    public UnityEvent CompletionEvent { get { return completionEvent; } }

    public int Count
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
    }

    public void Awake()
    {
        spawners = GetComponentsInChildren<EnemyWaveSpawner>();
    }

    public List<EnemyManager> Spawn()
    {
        List<EnemyManager> enemyManagers = new List<EnemyManager>();
        foreach (EnemyWaveSpawner spawner in spawners)
        {
            enemyManagers.AddRange(spawner.Spawn());
        }
        return enemyManagers;
    }
}
