using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDash : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 15f;
    private const float maxSpeedOnExit = 5f;

    private ParticleSystem dashParticles;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private const float rotationSpeed = 20f;
    private Vector3 targetPlayerDir;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip actClip = 
            PlayerInfo.AnimationManager.GetAnim(ResourceConstants.Player.Art.Dash);

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(actClip, actProcess);
        act.Type = AbilitySegmentType.Physics;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        GameObject dashParticlesObject =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.DashParticles),
                transform.position,
                Quaternion.identity);
        dashParticlesObject.transform.parent = PlayerInfo.Player.transform;
        dashParticles = dashParticlesObject.GetComponent<ParticleSystem>();

        coolDownDuration = 0.75f;

        staminaCost = 0f;

        GenerateCoolDownIcon(
            staminaCost,
            Resources.Load<Sprite>(ResourceConstants.Player.Abilities.DashTier1Icon),
            "I");
    }

    protected override bool WaitCondition()
    {
        return GameInfo.Settings.LeftDirectionalInput.magnitude >= 0.25f &&
               PlayerInfo.AbilityManager.Stamina >= staminaCost;
    }

    protected override void GlobalStart()
    {
        Vector2 targetPlayerDir2D = 
            GameInfo.CameraController.StdToCameraDir(GameInfo.Settings.LeftDirectionalInput);
        targetPlayerDir = new Vector3(targetPlayerDir2D.x, 0, targetPlayerDir2D.y).normalized;
    }

    public override void GlobalUpdate()
    {
        Vector3 incrementedRotation = Vector3.zero;
        incrementedRotation =
            Vector3.RotateTowards(
                PlayerInfo.Player.transform.forward,
                targetPlayerDir,
                rotationSpeed * Time.deltaTime,
                Mathf.Infinity);
        PlayerInfo.Player.transform.rotation =
            Quaternion.LookRotation(incrementedRotation, Vector3.up);
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StdToCameraDir(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();

        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);

        dashParticles.Play();

        GameInfo.CameraController.Speed = 0.25f;
        GameInfo.CameraController.TargetSpeed = 0;
        GameInfo.CameraController.SpeedGradation = 0.15f;

        PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
        PlayerInfo.CharMoveSystem.MaxConstantOnExit.ClaimLock(this, maxSpeedOnExit);
    }

    private void DuringAct()
    {
        system.CharMoveSystem.GroundMove(direction * speed);
    }

    private void ActEnd()
    {  
        PlayerInfo.MovementManager.TargetPercentileSpeed = PlayerInfo.MovementManager.SprintModifier;
        PlayerInfo.MovementManager.SnapSpeed();
        PlayerInfo.MovementManager.TurnOnSprint();

        dashParticles.Stop();

        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(this, false);
        PlayerInfo.CharMoveSystem.MaxConstantOnExit.TryReleaseLock(this, float.MaxValue);
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
        DeleteAbilityIcon();
        
        GameObject.Destroy(dashParticles);
    }
}