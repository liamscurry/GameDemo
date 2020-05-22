using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsManager
{
    private EnemyManager manager;

    public StatMultiplier MovespeedMultiplier { get; }

    public StatMultiplier DamageTakenMultiplier { get; }

    // Indicator that a health debuff is currently being applied to
    // the enemy. Used to color health bar to indicate health debuff.
    public StatMultiplier HealthDebuffMultiplier { get; }

    public EnemyStatsManager(EnemyManager manager)
    {   
        this.manager = manager;

        MovespeedMultiplier = new StatMultiplier(1);
        DamageTakenMultiplier = new StatMultiplier(1);
        HealthDebuffMultiplier = new StatMultiplier(1);
    }
}
