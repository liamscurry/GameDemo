﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The Dash skill allows the player to travel long distances in a short amount of time.

public sealed class PlayerFireChargeTier2 : PlayerAbility 
{
    //Fields
    private Vector2 direction;
    private float speed = 30f;
    private const float lifeDurationPercentage = 0.25f * (2f / 3f);
    private const float damage = 2f;

    private AbilitySegment act;
    private AbilityProcess actProcess;

    private List<FireChargeManager> charges;
    private List<PlayerMultiDamageHitbox> hitboxes; 

    private int invokeID;
    private List<EnemyHit> enemyHits;

    public override void Initialize(PlayerAbilityManager abilityManager)
    {
        this.system = abilityManager;

        AnimationClip actClip = Resources.Load<AnimationClip>("Player/Abilities/FireCharge/FireChargeTier1");

        actProcess = new AbilityProcess(ActBegin, null, null, 1);
        act = new AbilitySegment(actClip, actProcess);
        act.Type = AbilitySegmentType.Normal;

        segments = new AbilitySegmentList();
        segments.AddSegment(act);
        segments.NormalizeSegments();

        coolDownDuration = 2f;

        charges = new List<FireChargeManager>();
        hitboxes = new List<PlayerMultiDamageHitbox>();

        for (int i = 0; i < 4; i++)
        {
            GameObject charge = Instantiate(Resources.Load<GameObject>(ResourceConstants.Player.Hitboxes.FireChargeSegment), transform.position, Quaternion.identity);
            charge.transform.parent = PlayerInfo.MeleeObjects.transform;

            charges.Add(charge.GetComponent<FireChargeManager>());
            hitboxes.Add(charge.GetComponentInChildren<PlayerMultiDamageHitbox>());

            hitboxes[i].gameObject.SetActive(false);
        }
       
        invokeID = 0;
        enemyHits = new List<EnemyHit>();

        staminaCost = 1.5f;
        GenerateCoolDownIcon(
            staminaCost,
            Resources.Load<Sprite>(ResourceConstants.Player.Abilities.FirechargeTier2Icon),
            "II");
    }

    protected override bool WaitCondition()
    {
        return PlayerInfo.AbilityManager.Stamina >= staminaCost;
    }

    public override void GlobalConstantUpdate()
    {
        Vector2 projectedCameraDirection = Matho.StdProj2D(GameInfo.CameraController.Direction).normalized;
        Vector2 forwardDirection = (GameInfo.Settings.LeftDirectionalInput.y * projectedCameraDirection);
        Vector2 sidewaysDirection = (GameInfo.Settings.LeftDirectionalInput.x * Matho.Rotate(projectedCameraDirection, 90));
        Vector2 movementDirection = forwardDirection + sidewaysDirection;

        //Direction and speed targets
        if (GameInfo.Settings.LeftDirectionalInput.magnitude <= 0.5f)
        {
            PlayerInfo.MovementManager.LockDirection();
            PlayerInfo.MovementManager.TargetPercentileSpeed = 0;
        }
        else
        {
            Vector3 targetRotation = Matho.StandardProjection3D(GameInfo.CameraController.Direction).normalized;
            Vector3 currentRotation = Matho.StandardProjection3D(PlayerInfo.Player.transform.forward).normalized;
            Vector3 incrementedRotation = Vector3.RotateTowards(currentRotation, targetRotation, 10 * Time.deltaTime, 0f);
            Quaternion rotation = Quaternion.LookRotation(incrementedRotation, Vector3.up);
            PlayerInfo.Player.transform.rotation = rotation;

            PlayerInfo.MovementManager.TargetDirection = movementDirection;

            float forwardsAngle = Matho.AngleBetween(Matho.StdProj2D(targetRotation), movementDirection);
            float forwardsModifier = Mathf.Cos(forwardsAngle * 0.4f * Mathf.Deg2Rad);
        
            PlayerInfo.MovementManager.TargetPercentileSpeed = GameInfo.Settings.LeftDirectionalInput.magnitude * forwardsModifier;
        }

        PlayerInfo.CharMoveSystem.GroundMove(
            PlayerInfo.MovementManager.CurrentDirection *
            PlayerInfo.MovementManager.CurrentPercentileSpeed *
            PlayerInfo.StatsManager.Movespeed);

        PlayerInfo.Animator.SetFloat("speed", PlayerInfo.MovementManager.CurrentPercentileSpeed * PlayerInfo.StatsManager.MovespeedMultiplier.Value);
    }

    private void ActBegin()
    {
        invokeID = (invokeID + 1) % 10000;
        enemyHits.Clear();

        direction =
            Matho.StdProj2D(GameInfo.CameraController.transform.forward).normalized;

        for (int i = 0; i < 4; i++)
        {
            charges[i].gameObject.transform.position =
                transform.position + GameInfo.CameraController.transform.right * (i - 2f + 0.5f);
            charges[i].Initialize(this, direction * speed, lifeDurationPercentage * coolDownDuration);
            hitboxes[i].Invoke(this);
            hitboxes[i].gameObject.SetActive(true);
            charges[i].PostInitialization();
        }
        
        PlayerInfo.AbilityManager.ChangeStamina(-staminaCost);
    }

    public override bool OnHit(GameObject character)
    {
        EnemyManager enemy = character.GetComponent<EnemyManager>();

        // Check to see if enemy has already been hit by current charge.
        foreach (EnemyHit hit in enemyHits)
        {
            if (hit.enemy == enemy && hit.id == invokeID)
                return true;
        }

        enemy.Push((new Vector3(direction.x, 0, direction.y)).normalized * 7.75f);
        
        enemy.ChangeHealth(
            -damage * PlayerInfo.StatsManager.DamageMultiplier.Value);
        enemyHits.Add(new EnemyHit(invokeID, enemy));
        return true;
    }

    public override void ShortCircuitLogic()
    {
        
    }

    public override void DeleteResources()
    {
        DeleteAbilityIcon();

        for (int i = charges.Count - 1; i >= 0; i--)
        {
            charges[i].DeleteResource();
            Destroy(charges[i].gameObject);
        }
    }

    private struct EnemyHit
    {
        public readonly EnemyManager enemy;
        public readonly int id;
        public EnemyHit(int id, EnemyManager enemy)
        {
            this.id = id;
            this.enemy = enemy;
        }
    }
}