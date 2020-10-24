using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unlike other buffs, should only be issued by PlayerDashTier3 ability.
public sealed class PlayerDashTier3Buff : Buff<PlayerManager>
{
    private float damageMultiplier;

    public PlayerDashTier3Buff(BuffManager<PlayerManager> manager, BuffType type, float duration)
        : base(manager, type, duration) {}

    public override void ApplyBuff()
    {
        PlayerInfo.StatsManager.AttackSpeedMultiplier.AddModifier(2);
        PlayerInfo.StatsManager.DamageMultiplier.AddModifier(2f);
    }

    public override void ReverseBuff()
    {
        PlayerInfo.StatsManager.AttackSpeedMultiplier.RemoveModifier(2);
        PlayerInfo.StatsManager.DamageMultiplier.RemoveModifier(2f);
    }
}