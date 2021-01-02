using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerBlock : PlayerAbility 
{
    private AbilitySegment segment;
    private AbilityProcess process;

    private ParticleSystem blockParticles;

    private float timer;
    private const float minDuration = 0.5f;
    private const float staminaCostPerSecond = 2f;

    private bool broke;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip clip = Resources.Load<AnimationClip>("Player/Abilities/Block/Block");

        process = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1, true);
        segment = new AbilitySegment(clip, process);
        segment.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(segment);
        segments.NormalizeSegments();

        coolDownDuration = 0.5f;

        GameObject blockParticlesObject =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.BlockParticles),
                transform.position,
                Quaternion.identity);
        blockParticlesObject.transform.parent = PlayerInfo.Player.transform;
        blockParticles = blockParticlesObject.GetComponent<ParticleSystem>();

        PlayerInfo.Manager.OnBreak += OnBreak;
    }

    private void OnBreak(object sender, EventArgs args)
    {
        broke = true;
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCostPerSecond * minDuration;
    }

    private void ActBegin()
    {
        PlayerInfo.StatsManager.Blocking = true;
        blockParticles.Play();
        timer = 0;
        broke = false;
    }

    private void DuringAct()
    {
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCostPerSecond * Time.deltaTime);

        timer += Time.deltaTime;
        if (((PlayerAbilityManager) system).Stamina == 0 ||
            (!Input.GetKey(GameInfo.Settings.BlockAbilityKey) && timer > minDuration) ||
            broke)
        {
            ActiveProcess.IndefiniteFinished = true;
        }
    }

    private void ActEnd()
    {  
        PlayerInfo.StatsManager.Blocking = false;
        blockParticles.Stop();
    }

    public override bool OnHit(GameObject character)
    {
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }

    public override void DeleteResources()
    {
        PlayerInfo.Manager.OnBreak -= OnBreak;
    }
}