using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Contains references to player required objects.

public static class PlayerInfo 
{
    //Player Object References
    public static GameObject Player { get; private set; }
    public static GameObject Objects { get; private set; }
    public static GameObject MeleeObjects { get; private set; }

    //Components on player
    public static Rigidbody Body { get; private set; }
    public static CapsuleCollider Capsule { get; private set; }
    public static PlayerManager Manager  { get; private set; }
    public static Animator Animator { get; private set; }
    public static PlayerAnimEventConnector AnimConnector { get; private set; }

    //Components on player children
    public static PlayerSensor Sensor { get; private set; }
    public static Cloth KiltPhysics { get; private set; }

    //System subparts
    public static AdvancedMovementSystem MovementSystem { get; private set; }
    public static PhysicsSystem PhysicsSystem { get; private set; }
    public static CharacterMovementSystem CharMoveSystem { get; private set; }

    //Manager subparts
    public static PlayerAbilityManager AbilityManager { get; private set; }
    public static PlayerAnimationManager AnimationManager { get; private set; }
    public static BuffManager<PlayerManager> BuffManager { get; private set; }
    public static PlayerInteractionManager InteractionManager { get; private set; }
    public static PlayerMovementManager MovementManager { get; private set; }
    public static PlayerSkillManager SkillManager { get; private set; }
    public static PlayerStatsManager StatsManager { get; private set; }

    public static AnimatorOverrideController Controller { get; private set; }

    //Informational Properties
    public static Vector3 BottomSphereOffset { get; private set; } 

    public static bool TeleportingThisFrame;

    //Initializes references, called from GameInitializer.
    public static void Initialize(
        GameObject player,
        GameObject sensor,
        GameObject objects,
        GameObject meleeObjects,
        Transform cooldownOriginTransform,
        Cloth kiltPhysics)
    {
        //Object References
        Player = player;
        Objects = objects;
        MeleeObjects = meleeObjects;

        //Components on player assignments
        Body = player.GetComponent<Rigidbody>();
        Capsule = player.GetComponent<CapsuleCollider>();
        Manager = player.GetComponent<PlayerManager>();
        Animator = player.GetComponent<Animator>();
        AnimConnector = player.GetComponent<PlayerAnimEventConnector>();
        
        Controller = new AnimatorOverrideController(Animator.runtimeAnimatorController);
		Animator.runtimeAnimatorController = Controller;

        //Components on player children assignments
        Sensor = sensor.GetComponent<PlayerSensor>();
        KiltPhysics = kiltPhysics;

        //System subpart initializations
        PhysicsSystem = new PhysicsSystem(Player, Capsule, Body, 1);
        MovementSystem = new AdvancedMovementSystem(Player, Capsule, PhysicsSystem);
        CharMoveSystem = Player.GetComponent<CharacterMovementSystem>();
        CharMoveSystem.Initialize(Player);

        //Manager subpart initializations
        AnimationManager = new PlayerAnimationManager();
        AbilityManager =
            new PlayerAbilityManager(
                Animator,
                PhysicsSystem,
                MovementSystem,
                CharMoveSystem, 
                Player, 
                cooldownOriginTransform,
                75);
        BuffManager = new BuffManager<PlayerManager>(Manager);
        InteractionManager = new PlayerInteractionManager();
        MovementManager = new PlayerMovementManager();
        CharMoveSystem.SetOnKinematicOff(MovementManager.ZeroSpeed);
        StatsManager = new PlayerStatsManager();
        SkillManager = new PlayerSkillManager();

        //Informational Properties
        BottomSphereOffset = Capsule.BottomSphereOffset();

        PlayerInfo.Manager.MaxOutStamina();
    }
}