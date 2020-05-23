using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealthPickup : Pickup
{
    [SerializeField]
    private float healthGain;
    [SerializeField]
    private float minHealthSeek;

    private static float compositeHealthSeek;

    public override bool IsSeekValid()
    {
        float healthDifference = PlayerInfo.Manager.MaxHealth - PlayerInfo.Manager.Health;
        bool seek = 
               base.IsSeekValid() &&
               healthDifference >= minHealthSeek &&
               compositeHealthSeek < healthDifference;
        if (seek)
        {
            compositeHealthSeek += healthGain;
        }
        return seek;
    }

    protected override void OnReachPlayer()
    {
        compositeHealthSeek -= healthGain;
        PlayerInfo.Manager.ChangeHealth(healthGain);
    }

    protected override void Recycle()
    {
        GameInfo.PickupPool.Add<HealthPickup>(gameObject);
    }

    public override void OnForceRecycle()
    {
        compositeHealthSeek -= healthGain;
    }
}