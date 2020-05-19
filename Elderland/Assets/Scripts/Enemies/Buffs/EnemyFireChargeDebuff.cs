using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unlike other buffs, should only be issued by FireCharge ability.
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
        manager.StatsManager.MovespeedMultiplier.AddModifier(0.25f);
        manager.StatsManager.HealthDebuffMultiplier.AddModifier(0);
    }

    public override void ReverseBuff()
    {
        manager.StatsManager.DamageTakenMultiplier.RemoveModifier(damageTakenPercentage);
        manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.25f);
        manager.StatsManager.HealthDebuffMultiplier.RemoveModifier(0);
    }
}
