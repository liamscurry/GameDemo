using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unlike other buffs, should only be issued by FireCharge ability.
public sealed class EnemyTier3Buff : Buff<EnemyManager>
{
    private Color healthBarColor;

    public EnemyTier3Buff(Color healthBarColor, BuffManager<EnemyManager> manager)
        : base(manager, BuffType.Buff, 1)
    {
        this.healthBarColor = healthBarColor;
    }

    public override void ApplyBuff()
    {
        manager.Manager.HealthBarColor = healthBarColor;
        manager.Manager.SetTierMaxHealth(2f);
        manager.Manager.MaxOutHealth();
    }

    public override void ReverseBuff()
    {

    }
}
