using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealthPickup : Pickup
{
    [SerializeField]
    private float healthGain;
    [SerializeField]
    private float minHealthSeek;

    public override bool IsSeekValid()
    {
        return base.IsSeekValid() &&
               PlayerInfo.Manager.MaxHealth - PlayerInfo.Manager.Health >= minHealthSeek;
    }

    protected override void OnReachPlayer()
    {
        PlayerInfo.Manager.ChangeHealth(healthGain);
    }

    protected override void Recycle()
    {
        GameInfo.PickupPool.Add<HealthPickup>(gameObject);
    }
}