using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerDashBuff : Buff<PlayerManager>
{
    private float damageMultiplier;

    public PlayerDashBuff(float damageMultiplier, BuffManager<PlayerManager> manager, BuffType type, float duration)
        : base(manager, type, duration)
    {
        this.damageMultiplier = damageMultiplier;
    }

    public override void ApplyBuff()
    {
        PlayerInfo.StatsManager.DamageMultiplier.AddModifier(damageMultiplier);
    }

    public override void ReverseBuff()
    {
        PlayerInfo.StatsManager.DamageMultiplier.RemoveModifier(damageMultiplier);
    }
}