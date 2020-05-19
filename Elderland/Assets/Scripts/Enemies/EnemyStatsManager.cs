using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsManager
{
    private EnemyManager manager;

    public EnemyStatMultiplier MovespeedMultiplier { get; }

    public EnemyStatMultiplier DamageTakenMultiplier { get; }

    // Indicator that a health debuff is currently being applied to
    // the enemy. Used to color health bar to indicate health debuff.
    public EnemyStatMultiplier HealthDebuffMultiplier { get; }

    public EnemyStatsManager(EnemyManager manager)
    {   
        this.manager = manager;

        MovespeedMultiplier = new EnemyStatMultiplier(1);
        DamageTakenMultiplier = new EnemyStatMultiplier(1);
        HealthDebuffMultiplier = new EnemyStatMultiplier(1);
    }
}
