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
    //private const float staminaCostPerSecond = 2f;
    private const float staminaCostPerBlock = 1f;

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
        PlayerInfo.Manager.OnBlock += OnBlock;
    }

    private void OnBreak(object sender, EventArgs args)
    {
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCostPerBlock);
        broke = true;
    }

    private void OnBlock(object sender, EventArgs args)
    {
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCostPerBlock);
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCostPerBlock;
    }

    protected override void GlobalStart()
    {
        GameInfo.CameraController.ZoomIn.ClaimLock(this, (true, -5, 0.32f));
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
        timer += Time.deltaTime;
        if (((PlayerAbilityManager) system).Stamina < staminaCostPerBlock ||
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
        GameInfo.CameraController.ZoomIn.TryReleaseLock(this, (false, 0, 0));
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