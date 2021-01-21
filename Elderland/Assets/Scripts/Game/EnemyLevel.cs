using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyLevel : MonoBehaviour
{
    [SerializeField]
    private GameObject mechanicsParent;
    [SerializeField]
    private Transform respawnTransform;
    [SerializeField]
    private float top;
    [SerializeField]
    private float bottom;
    [SerializeField]
    private Vector2 size;
    [SerializeField]
    private Vector3 center;
    [SerializeField]
    private UnityEvent startEvent;
    [SerializeField]
    private UnityEvent endEvent;
    [SerializeField]
    private bool keepWaypointsOnFight;

    private float topPosition;
    private float bottomPosition;
    private float height;

    private EnemyWave[] waves;
    private int currentWave;
    private int currentWaveCount;

    private List<EnemyManager> currentEnemyManagers;
    private bool completed;
    private bool started;

    private LevelMechanic[] mechanics;

    public Transform RespawnTransform { get { return respawnTransform; } }

    public void Start()
    {
        topPosition = transform.position.y + center.y + top;
        bottomPosition = transform.position.y + center.y + bottom;
        height = topPosition - bottomPosition;

        waves = GetComponentsInChildren<EnemyWave>();
        mechanics = mechanicsParent.GetComponentsInChildren<LevelMechanic>();
    }

    public void Reset()
    {
        CancelLevel(true);
        foreach (LevelMechanic mechanic in mechanics)
        {
            mechanic.ResetEvent.Invoke();
            mechanic.ResetSelf();
        } 
    }

    public void StartLevel()
    {
        startEvent.Invoke();

        foreach (LevelMechanic mechanic in mechanics)
        {
            mechanic.InvokeSelf();
        } 

        StartWaves();
        PlayerInfo.Manager.SaveHealth();
        PlayerInfo.Manager.SaveStamina();
        if (!keepWaypointsOnFight)
            GameInfo.Menu.ObjectiveManager.DisableWaypoints(this);

        //GameInfo.Menu.ObjectiveManager.ObjectiveWaypoints.gameObject.SetActive(false);
    }

    private void StartWaves()
    {
        currentWave = -1;
        started = true;
        completed = false;
        AdvanceWave();
    }

    public void RemoveFromWave()
    {
        currentWaveCount--;
    }

    public void CancelLevel(bool instant = false)
    {
        if (started && !completed)
        {
            GameInfo.Menu.ObjectiveManager.EnableWaypoints(this);
            StopCoroutine("WatchWave");
            
            if (!instant)
            {
                endEvent.Invoke();
            }

            for (int index = 0; index < currentEnemyManagers.Count; index++)
            {
                if (!instant)
                {
                    if (currentEnemyManagers[index] != null)
                        currentEnemyManagers[index].Die();
                }
                else
                {
                    if (currentEnemyManagers[index] != null)
                        currentEnemyManagers[index].DieInstant();
                }
            }

            if (instant)
            {
                GameInfo.ProjectilePool.ClearProjectilePool();
            }

            completed = true;
        }
    }

    public void SetRespawnTransform(Transform respawnTransform)
    {
        this.respawnTransform = respawnTransform;
    }

    private void AdvanceWave()
    {
        if (currentWave >= 0 && waves[currentWave].CompletionEvent != null)
            waves[currentWave].CompletionEvent.Invoke();

        currentWave++;

        if (currentWave <= waves.Length - 1)
        {
            currentWaveCount = waves[currentWave].Count;
            currentEnemyManagers = waves[currentWave].Spawn();
            StartCoroutine("WatchWave");
        }
        else
        {
            completed = true;
            endEvent.Invoke();
            GameInfo.Menu.ObjectiveManager.EnableWaypoints(this);
        }
    }

    private IEnumerator WatchWave()
    {
        yield return new WaitUntil(() => currentWaveCount == 0);
        AdvanceWave();
    }

    public Vector3 NavCast(Vector2 location, bool inspector = false)
    {   
        if (!inspector)
        {
            Vector3 start = new Vector3(location.x, topPosition, location.y);
            RaycastHit hit;
            if (Physics.Raycast(start, Vector3.down, out hit, height, LayerConstants.GroundCollision))
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }
        else
        {
            float topPosition = transform.position.y + center.y + top;
            float bottomPosition = transform.position.y + center.y + bottom;
            float height = topPosition - bottomPosition;

            Vector3 start = new Vector3(location.x, topPosition, location.y);
            RaycastHit hit;
            if (Physics.Raycast(start, Vector3.down, out hit, height, LayerConstants.GroundCollision))
            {
                return hit.point;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        Vector3 drawSize = new Vector3(size.x, 0.1f, size.y);
        Gizmos.DrawCube(transform.position + center + top * Vector3.up, drawSize);
        Gizmos.DrawCube(transform.position + center + bottom * Vector3.up, drawSize);
    }
}
