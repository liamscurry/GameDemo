using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealthPickup : Pickup
{
    [SerializeField]
    private float healthGain;
    [SerializeField]
    private float minHealthSeek;
    [SerializeField]
    private ParticleSystem centralParticle;
    [SerializeField]
    private ParticleSystem trailParticles;

    private static float compositeHealthSeek;

    private MeshRenderer meshRenderer;
    
    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void Initialize(Vector3 position)
    {
        base.Initialize(position);
        meshRenderer.enabled = true;
        centralParticle.Play();
        ParticleSystem.MainModule newMain = centralParticle.main;
        newMain.simulationSpeed = 1;
        trailParticles.Play();
        ParticleSystem.MainModule newMainTrail = trailParticles.main;
        newMainTrail.simulationSpeed = 1;
    }

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
        meshRenderer.enabled = false;
        ParticleSystem.MainModule newMain = centralParticle.main;
        newMain.simulationSpeed = 2f;
        centralParticle.Stop();
        ParticleSystem.MainModule newMainTrail = trailParticles.main;
        newMainTrail.simulationSpeed = 2;
        trailParticles.Stop();
        compositeHealthSeek -= healthGain;
        PlayerInfo.Manager.ChangeHealth(healthGain);
    }

    public override void OnForceRecycle()
    {
        compositeHealthSeek -= healthGain;
    }
}