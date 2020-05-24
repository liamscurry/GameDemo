using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerDashTier3 : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 30f;

    private AbilitySegment act;
    private AbilityProcess actProcess;
    private const float staminaCost = 2f;

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

        continous = true;
    }

    protected override bool WaitCondition()
    {
        return GameInfo.Settings.LeftDirectionalInput.magnitude >= 0.25f &&
               PlayerInfo.AbilityManager.Stamina >=
                staminaCost * PlayerInfo.StatsManager.DashCostMultiplier.Value;
    }

    private void ActBegin()
    {
        direction = GameInfo.CameraController.StandardToCameraDirection(GameInfo.Settings.LeftDirectionalInput);
        PlayerInfo.MovementManager.TargetDirection = direction;
        PlayerInfo.MovementManager.SnapDirection();
        system.Physics.GravityStrength = 0;
        system.Movement.ExitEnabled = false;
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost * PlayerInfo.StatsManager.DashCostMultiplier.Value);
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
        
        bool alreadyHasDashBuff = false;
        for (int i = PlayerInfo.BuffManager.Buffs.Count - 1;
             i >= 0; i--)
        {
            Buff<PlayerManager> buff = PlayerInfo.BuffManager.Buffs[i];
            if (buff is PlayerDashTier3Buff)
            {
                alreadyHasDashBuff = true;
                PlayerInfo.BuffManager.Clear(buff);
                break;
            }
        }

        PlayerInfo.BuffManager.Apply<PlayerDashTier2Buff>(
            new PlayerDashTier2Buff(2f, PlayerInfo.BuffManager, BuffType.Buff, 5f));

        if (!alreadyHasDashBuff)
        {
            PlayerInfo.BuffManager.Apply<PlayerDashTier3Buff>(
                new PlayerDashTier3Buff(PlayerInfo.BuffManager, BuffType.Buff, 5f));
        }
    }

    public override bool OnHit(GameObject character)
    {
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}