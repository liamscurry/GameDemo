using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyFireChargeDebuff : EnemyBuff
{
    private float damageTakenPercentage;

    public EnemyFireChargeDebuff(float damageTakenPercentage, EnemyManager manager, BuffType type, float duration)
        : base(manager, type, duration)
    {
        this.damageTakenPercentage = damageTakenPercentage;
    }

    public override void ApplyBuff()
    {
        manager.StatsManager.DamageTakenMultiplier.AddModifier(damageTakenPercentage);
    }

    public override void ReverseBuff()
    {
        manager.StatsManager.DamageTakenMultiplier.RemoveModifier(damageTakenPercentage);
    }
}
