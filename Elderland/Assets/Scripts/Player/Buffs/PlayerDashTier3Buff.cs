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
        PlayerInfo.StatsManager.DashCostMultiplier.AddModifier(0);
    }

    public override void ReverseBuff()
    {
        PlayerInfo.StatsManager.DashCostMultiplier.RemoveModifier(0);
    }
}