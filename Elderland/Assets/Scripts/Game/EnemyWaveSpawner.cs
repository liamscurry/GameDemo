using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    protected enum EnemyType { Light, Heavy, Ranged, Grunt, Turret }
    protected enum EnemyTier { One, Two, Three }

    [SerializeField]
    protected EnemySpawn[] enemies;

    protected EnemyLevel level;

    public int Count { get { return enemies.Length; } }

    protected Color tier2Color = new Color(255f / 255f, 221f / 255f, 0, 1);
    protected Color tier3Color = new Color(0f / 255f, 255f / 255f, 136f / 255f, 1);

    public virtual List<EnemyManager> Spawn()
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
                case EnemyType.Grunt:
                    enemy = Resources.Load<GameObject>(ResourceConstants.Enemy.Enemies.GruntEnemy);
                    break;  
                case EnemyType.Turret:
                    enemy = Resources.Load<GameObject>(ResourceConstants.Enemy.Enemies.TurretEnemy);
                    break;     
                default:
                    throw new System.Exception("Not implemented to spawn yet");
            }

            Vector3 position;
            if (spawn.useExplicitLocation)
            {
                position = spawn.explicitLocation + transform.position;
            }
            else
            {
                position = 
                    CalculatePosition(spawn) +
                    enemy.GetComponent<CapsuleCollider>().height / 2 * Vector3.up + spawn.heightOffset * Vector3.up;
            }
            
            Quaternion rotation = CalculateRotation(spawn);
            enemy = Instantiate(enemy, position, rotation) as GameObject;
            EnemyManager enemyManager = enemy.GetComponent<EnemyManager>();
            enemyManager.Level = level;
            StartCoroutine(ApplyTierBuff(enemyManager, spawn.tier));
            enemyManagers.Add(enemyManager);
        }
        return enemyManagers;
    }

    protected virtual Vector3 CalculatePosition(EnemySpawn spawn)
    {
        if (level == null)
        {
            level = transform.parent.parent.GetComponent<EnemyLevel>();
        }
        return level.NavCast(Matho.StdProj2D(transform.position) + spawn.location, true);
    }

    protected Quaternion CalculateRotation(EnemySpawn spawn)
    {
        Vector3 forward = 
            new Vector3(Mathf.Cos(spawn.direction * Mathf.Deg2Rad),
            0,
            Mathf.Sin(spawn.direction * Mathf.Deg2Rad));

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    [System.Serializable]
    protected class EnemySpawn
    {
        [SerializeField]
        public Vector2 location;
        [SerializeField]
        public float heightOffset;
        [SerializeField]
        public bool useExplicitLocation;
        [SerializeField]
        public Vector3 explicitLocation;
        [SerializeField]
        public float direction;
        [SerializeField]
        public EnemyType type;
        [SerializeField]
        public EnemyTier tier;

        public EnemySpawn(
            Vector2 location,
            float direction,
            EnemyType type,
            EnemyTier tier,
            bool useExplicitLocation,
            Vector3 explicitLocation)
        {
            this.location = location;
            this.type = type;
            this.direction = direction;
            this.tier = tier;
            this.useExplicitLocation = useExplicitLocation;
            this.explicitLocation = explicitLocation;
        }
    }

    protected IEnumerator ApplyTierBuff(EnemyManager enemyManager, EnemyTier tier)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (enemyManager != null)
        {
            switch (tier)
            {
                case EnemyTier.Two:
                    enemyManager.BuffManager.Apply(new EnemyTier2Buff(tier2Color, enemyManager.BuffManager));
                    break;
                case EnemyTier.Three:
                    enemyManager.BuffManager.Apply(new EnemyTier3Buff(tier3Color, enemyManager.BuffManager));
                    break;
                default:
                    break;
            }
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

            Vector3 position;
            if (spawn.useExplicitLocation)
            {
                position = spawn.explicitLocation + transform.position;
            }
            else
            {
                position =
                    CalculatePosition(spawn) + spawn.heightOffset * Vector3.up;
            }
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
