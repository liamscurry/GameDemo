using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsManager
{
    private EnemyManager manager;

    public EnemyStatMultiplier MovespeedMultiplier { get; }

    public EnemyStatMultiplier DamageTakenMultiplier { get; }

    public EnemyStatsManager(EnemyManager manager)
    {   
        this.manager = manager;

        MovespeedMultiplier = new EnemyStatMultiplier(1);
        DamageTakenMultiplier = new EnemyStatMultiplier(1);
    }
}
