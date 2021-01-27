using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour
{
    [Header("Existing enemies should be childs of this game object (ex Turrets).")]
    [SerializeField]
    private UnityEvent completionEvent;

    private EnemyWaveSpawner[] spawners;
    private EnemyLevel level;

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
        level = transform.parent.GetComponent<EnemyLevel>();
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
