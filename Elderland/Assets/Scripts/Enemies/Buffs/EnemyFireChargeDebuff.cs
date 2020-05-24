using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unlike other buffs, should only be issued by FireCharge ability.
public sealed class EnemyFireChargeDebuff : Buff<EnemyManager>
{
    private float damageTakenPercentage;

    public EnemyFireChargeDebuff(float damageTakenPercentage, BuffManager<EnemyManager> manager, BuffType type, float duration)
        : base(manager, type, duration)
    {
        this.damageTakenPercentage = damageTakenPercentage;
    }

    public override void ApplyBuff()
    {
        manager.Manager.StatsManager.DamageTakenMultiplier.AddModifier(damageTakenPercentage);
        manager.Manager.StatsManager.MovespeedMultiplier.AddModifier(0.25f);
        manager.Manager.StatsManager.HealthDebuffMultiplier.AddModifier(0);
    }

    public override void ReverseBuff()
    {
        manager.Manager.StatsManager.DamageTakenMultiplier.RemoveModifier(damageTakenPercentage);
        manager.Manager.StatsManager.MovespeedMultiplier.RemoveModifier(0.25f);
        manager.Manager.StatsManager.HealthDebuffMultiplier.RemoveModifier(0);
    }
}
