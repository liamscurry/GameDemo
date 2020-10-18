using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDashTier2 : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 18f;

    private ParticleSystem dashParticles;

    private AbilitySegment act;
    private AbilityProcess actProcess;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip dashClip = Resources.Load<AnimationClip>("Player/Abilities/Dash/Dash");

        actProcess = new AbilityProcess(ActBegin, DuringAct, ActEnd, 1);
        act = new AbilitySegment(dashClip, actProcess);
        act.Type = AbilitySegmentType.Physics;
        act.LoopFactor = 2;

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

        coolDownDuration = 1f;

        staminaCost = 1f;
        GenerateCoolDownIcon(
            staminaCost,
            Resources.Load<Sprite>(ResourceConstants.Player.UI.Abilities.DashTier1Icon),
            "II");
    }

    protected override bool WaitCondition()
    {
        return GameInfo.Settings.LeftDirectionalInput.magnitude >= 0.25f &&
               PlayerInfo.AbilityManager.Stamina >= staminaCost;
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
        PlayerInfo.BuffManager.Apply<PlayerDashTier2Buff>(
            new PlayerDashTier2Buff(2f, PlayerInfo.BuffManager, BuffType.Buff, 5f));

        dashParticles.Stop();
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