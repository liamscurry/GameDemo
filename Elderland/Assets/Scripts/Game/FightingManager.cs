using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FightingManager : MonoBehaviour
{
    [SerializeField]
    private float centerDistance;
    [SerializeField]
    private float arrangementDistance;
    [SerializeField]
    private EnemyManager enemy1;
    [SerializeField]
    private EnemyManager enemy2;
    [SerializeField]
    private EnemyManager enemy3;

    private Vector2 currentCenter;

    private float timer;
    private const float duration = 0.5f;

    private void Start()
    {
        TurnOn();
    }

    public void SetLevel(EnemyLevel level)
    {
        GameInfo.CurrentLevel = level;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        Vector2 center = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float distance = Vector2.Distance(currentCenter, center);

        UpdateCenter(center);
        UpdateValidity();

        if (timer > duration)
            timer = 0;
    }

    private void LateUpdate()
    {
        EnemyInfo.RangedArranger.LateUpdateArranger();
    }

    private void UpdateCenter(Vector2 center)
    {
        EnemyInfo.MeleeArranger.Center = center;
        EnemyInfo.RangedArranger.Center = center;
    }

    private void UpdateValidity()
    {
        for (int index = 0; index < EnemyInfo.MeleeArranger.n; index++)
        {
            if (EnemyInfo.MeleeArranger.nodes[index] != null)
            {
                if (!EnemyInfo.MeleeArranger.CheckValidity(index))
                {
                    EnemyManager enemy = EnemyInfo.MeleeArranger.nodes[index];
                    EnemyInfo.MeleeArranger.ClearNode(index);
                    EnemyInfo.MeleeArranger.ClaimNode(enemy);
                    if (enemy.ArrangementNode != -1)
                    {
                        enemy.CalculateAgentPath();
                    }
                    else
                    {
                        enemy.CancelAgentPath();
                    }
                }
            }
        }
    }

    private void Clear()
    {
        EnemyInfo.MeleeArranger.ClearNodes();
    }

    public void TurnOn()
    {
        currentCenter = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        UpdateCenter(currentCenter);
        Clear();
        gameObject.SetActive(true);
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (EnemyInfo.MeleeArranger != null && GameInfo.CurrentLevel != null)
        {
            for (int index = 0; index < EnemyInfo.MeleeArranger.n; index++)
            {
                if (EnemyInfo.MeleeArranger.nodes[index] != null)
                {
                    EnemyManager enemy = EnemyInfo.MeleeArranger.nodes[index];
                    float radius = enemy.ArrangmentRadius;
                    Vector3 position = EnemyInfo.MeleeArranger.GetPosition(index);
                    position.z = position.y;
                    position.y = PlayerInfo.Player.transform.position.y;
        
                    Gizmos.color = (!EnemyInfo.MeleeArranger.CheckValidity(index)) ? Color.red : Color.green;
                    Gizmos.DrawLine(enemy.transform.position, position);
                    Gizmos.DrawCube(position + Vector3.up * 5, Vector3.one * 0.5f);
                }
                else
                {
                    Vector3 position = EnemyInfo.MeleeArranger.GetPosition(index);
                    position.z = position.y;
                    position.y = PlayerInfo.Player.transform.position.y;
        
                    Gizmos.color = (!EnemyInfo.MeleeArranger.CheckValidity(index)) ? Color.red : Color.green;
                    Gizmos.DrawLine(PlayerInfo.Player.transform.position, position);
                    Gizmos.DrawCube(position + Vector3.up * 5, Vector3.one * 0.5f);
                }
            }

            for (int index = 0; index < EnemyInfo.RangedArranger.n; index++)
            {
                Vector3 position = EnemyInfo.RangedArranger.GetPosition(index);
                position.z = position.y;
                position.y = PlayerInfo.Player.transform.position.y;

                Gizmos.color = (!EnemyInfo.RangedArranger.GetValidity(index)) ? Color.red : Color.green;
                Gizmos.DrawCube(position + Vector3.up * 5, Vector3.one * 0.5f);
            }
        }
    }
}
