using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDash : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 15f;

    private ParticleSystem dashParticles;

    private AbilitySegment act;
    private AbilityProcess actProcess;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(null, actProcess);
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
        Vector2 playerInput =
            PlayerInfo.MovementManager.DirectionToPlayerCoord(GameInfo.Settings.LeftDirectionalInput);
        AnimationClip dashClip =
            GetDirAnim(ResourceConstants.Player.Art.Dash, playerInput);
        act.Clip = dashClip;
    }

    public override void GlobalUpdate()
    {
        PlayerInfo.AnimationManager.UpdateRotation(true);
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);

        dashParticles.Play();

        GameInfo.CameraController.Speed = 0.5f;
        GameInfo.CameraController.TargetSpeed = 0;
        GameInfo.CameraController.SpeedGradation = .15f;

        PlayerInfo.StatsManager.Invulnerable.ClaimLock(this, true);
    }

    private void DuringAct()
    {
        //if (GameInfo.Settings.LeftDirectionalInput.magnitude > 0.25f)
        //{
        //    direction = Matho.RotateTowards(direction, GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput.normalized), 150 * Time.deltaTime);
        //}
        
        if (system.Physics.TouchingFloor)
        {
            actVelocity = system.Movement.Move(direction, speed);
        }
        else
        {
            system.Physics.AnimationVelocity += system.Movement.ExitVelocity;
            actVelocity = system.Movement.ExitVelocity;
        }  
        
        //Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
        //system.Parent.transform.rotation = rotation;
    }

    private void ActEnd()
    {  
        //PlayerInfo.MovementManager.TargetDirection = direction;
        //PlayerInfo.MovementManager.SnapDirection();
        PlayerInfo.MovementManager.TargetPercentileSpeed = 1;
        PlayerInfo.MovementManager.SnapSpeed();
        system.Physics.GravityStrength = PhysicsSystem.GravitationalConstant;
        system.Movement.ExitEnabled = true;

        dashParticles.Stop();

        PlayerInfo.StatsManager.Invulnerable.TryReleaseLock(this, false);
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