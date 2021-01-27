using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour
{
    [Header("Existing enemies should be childs of this game object (ex Turrets).")]
    [SerializeField]
    private UnityEvent completionEvent;

    private bool[] existingActive;
    private EnemyManager[] existingEnemies;
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

            count += existingEnemies.Length;

            return count;
        }
    }

    public void Awake()
    {
        spawners = GetComponentsInChildren<EnemyWaveSpawner>();
        level = transform.parent.GetComponent<EnemyLevel>();
        existingEnemies = GetComponentsInChildren<EnemyManager>(true);
        foreach (EnemyManager enemy in existingEnemies)
        {
            enemy.Level = level;
        }
        existingActive = new bool[existingEnemies.Length];
    }

    public List<EnemyManager> Spawn()
    {
        List<EnemyManager> enemyManagers = new List<EnemyManager>();
        foreach (EnemyWaveSpawner spawner in spawners)
        {
            enemyManagers.AddRange(spawner.Spawn());
        }

        // Set level of enemy. Tiers not currently supported.
        for (int i = 0; i < existingActive.Length; i++)
        {
            EnemyManager enemy = existingEnemies[i];
            existingActive[i] = enemy.gameObject.activeSelf;
            enemy.gameObject.SetActive(false);
            var clonedEnemy = Object.Instantiate<GameObject>(
                enemy.gameObject,
                enemy.transform.position,
                enemy.transform.rotation,
                enemy.transform.parent);
            clonedEnemy.gameObject.SetActive(true);
            EnemyManager clonedEnemyManger = 
                clonedEnemy.GetComponent<EnemyManager>();
            clonedEnemyManger.Level = level;
            enemyManagers.Add(clonedEnemyManger);
        }

        return enemyManagers;
    }

    public void RespawnExistingEnemies()
    {
        for (int i = 0; i < existingActive.Length; i++)
        {
            existingEnemies[i].gameObject.SetActive(existingActive[i]);
        }
    }
}
