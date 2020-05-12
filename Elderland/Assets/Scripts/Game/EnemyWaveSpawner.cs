using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    private enum EnemyType { Light, Heavy, Ranged }

    [SerializeField]
    private EnemySpawn[] enemies;

    private EnemyLevel level;

    public int Count { get { return enemies.Length; } }

    public List<EnemyManager> Spawn()
    {
        List<EnemyManager> enemyManagers = new List<EnemyManager>();
        foreach (EnemySpawn spawn in enemies)
        {
            GameObject enemy = null;
            switch (spawn.type)
            { 
                case EnemyType.Light:
                    enemy = Resources.Load<GameObject>(ResourceConstants.Enemy.Enemies.LightEnemy);
                    break;
                case EnemyType.Heavy:
                    enemy = Resources.Load<GameObject>(ResourceConstants.Enemy.Enemies.HeavyEnemy);
                    break;
                case EnemyType.Ranged:
                    enemy = Resources.Load<GameObject>(ResourceConstants.Enemy.Enemies.RangedEnemy);
                    break;
                default:
                    throw new System.Exception("Not implemented to spawn yet");
            }

            Vector3 position = CalculatePosition(spawn) + enemy.GetComponent<CapsuleCollider>().height / 2 * Vector3.up + spawn.heightOffset * Vector3.up;
            Quaternion rotation = CalculateRotation(spawn);
            enemy = Instantiate(enemy, position, rotation) as GameObject;
            EnemyManager enemyManager = enemy.GetComponent<EnemyManager>();
            enemyManager.Level = level;
            enemyManagers.Add(enemyManager);
        }
        return enemyManagers;
    }

    private Vector3 CalculatePosition(EnemySpawn spawn)
    {
        if (level == null)
        {
            level = transform.parent.parent.GetComponent<EnemyLevel>();
        }
        return level.NavCast(Matho.StandardProjection2D(transform.position) + spawn.location, true);
    }

    private Quaternion CalculateRotation(EnemySpawn spawn)
    {
        Vector3 forward = 
            new Vector3(Mathf.Cos(spawn.direction * Mathf.Deg2Rad),
            0,
            Mathf.Sin(spawn.direction * Mathf.Deg2Rad));

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    [System.Serializable]
    private class EnemySpawn
    {
        [SerializeField]
        public Vector2 location;
        [SerializeField]
        public float heightOffset;
        [SerializeField]
        public float direction;
        [SerializeField]
        public EnemyType type;

        public EnemySpawn(Vector2 location, float direction, EnemyType type)
        {
            this.location = location;
            this.type = type;
            this.direction = direction;
        }
    }

    public void OnDrawGizmosSelected()
    {
        foreach (EnemySpawn spawn in enemies)
        {
            switch (spawn.type)
            { 
                case EnemyType.Light:
                    Gizmos.color = Color.blue;
                    break;
                case EnemyType.Heavy:
                    Gizmos.color = Color.red;
                    break;
                case EnemyType.Ranged:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.black;
                    break;
            }

            Vector3 position = CalculatePosition(spawn) + spawn.heightOffset * Vector3.up;
            Vector3 forward = 
                new Vector3(Mathf.Cos(spawn.direction * Mathf.Deg2Rad),
                0,
                Mathf.Sin(spawn.direction * Mathf.Deg2Rad));

            Gizmos.DrawLine(position, position + forward);
            Gizmos.DrawCube(position, Vector3.one);
            Gizmos.DrawCube(position + forward, Vector3.one * 0.3f);
        }
    }
}
