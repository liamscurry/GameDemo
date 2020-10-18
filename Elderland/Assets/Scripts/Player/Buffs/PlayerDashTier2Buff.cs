using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerDashTier2Buff : Buff<PlayerManager>
{
    public PlayerDashTier2Buff(BuffManager<PlayerManager> manager, BuffType type, float duration)
        : base(manager, type, duration)
    {}

    public override void ApplyBuff()
    {
        PlayerInfo.StatsManager.DamageMultiplier.AddModifier(1.5f);
    }

    public override void ReverseBuff()
    {
        PlayerInfo.StatsManager.DamageMultiplier.RemoveModifier(1.5f);
    }
}