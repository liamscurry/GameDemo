using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public static void Initialize()
    {
        MeleeAttackers = new List<EnemyManager>(MaxMeleeAttackers);
        RangedAttackers = new List<EnemyManager>(MaxRangedAttackers);

        MeleeWatchers = new List<EnemyManager>();
        RangedWatchers = new List<EnemyManager>();

        Vector2 playerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        MeleeArranger = new MeleeArranger(playerPosition, 1.75f + 1.5f, 8, 0);
        RangedArranger = new RangedArranger(playerPosition, 12, 16, 0);//7

        AbilityRandomizer = new System.Random();
    }
}