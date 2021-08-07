using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyInfo
{
    public static readonly int MaxMeleeAttackers = 3;
    public static readonly int MaxRangedAttackers = 3;
    public static readonly int MaxOverallAttackers = 6;

    public static List<EnemyManager> MeleeAttackers { get; private set; }
    public static List<EnemyManager> RangedAttackers { get; private set; }

    public static List<EnemyManager> MeleeWatchers { get; private set; }
    public static List<EnemyManager> RangedWatchers { get; private set; }
    
    public const float OverrideMargin = 0.5f;

    public static readonly float AttackWidth = 5;
    public static readonly float AttackWatchMargin = 1;

    public static MeleeArranger MeleeArranger { get; private set; }
    public static RangedArranger RangedArranger { get; private set; }

    public static System.Random AbilityRandomizer { get; private set; }

    public static Color ArmorColor { get; private set; }
    public static Color HealthColor { get; private set; }
    public static Color FinisherHealthColor { get; private set; }
    public static float ShadowColorDim { get; private set; }
    
    private const float obstructionCheckMargin = 0.25f;

    public static void Initialize(
        Color armorColor,
        Color healthColor,
        Color finisherHealthColor,
        float shadowColorDim)
    {
        MeleeAttackers = new List<EnemyManager>(MaxMeleeAttackers);
        RangedAttackers = new List<EnemyManager>(MaxRangedAttackers);

        MeleeWatchers = new List<EnemyManager>();
        RangedWatchers = new List<EnemyManager>();

        Vector2 playerPosition = Matho.StdProj2D(PlayerInfo.Player.transform.position);
        MeleeArranger = new MeleeArranger(playerPosition, 1.75f + 1.5f, 8, 0);
        RangedArranger = new RangedArranger(playerPosition, 12, 16, 0);//7

        AbilityRandomizer = new System.Random();

        ArmorColor = armorColor;
        HealthColor = healthColor;
        FinisherHealthColor = finisherHealthColor;
        ShadowColorDim = shadowColorDim;
    }

    public static bool IsEnemyObstructed(Collider other)
    {
        NavMeshHit navMeshHit;
        Vector3 endPosition = 
            Vector3.MoveTowards(
                PlayerInfo.Player.transform.position,
                other.transform.parent.position,
                obstructionCheckMargin);

        return other.GetComponentInParent<NavMeshAgent>().Raycast(
                endPosition,
                out navMeshHit);
    }
}