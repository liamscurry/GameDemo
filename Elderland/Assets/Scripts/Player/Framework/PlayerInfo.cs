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

    //Components on player children
    public static PlayerSensor Sensor { get; private set; }

    //System subparts
    public static AdvancedMovementSystem MovementSystem { get; private set; }
    public static PhysicsSystem PhysicsSystem { get; private set; }

    //Manager subparts
    public static PlayerAbilityManager AbilityManager { get; private set; }
    public static PlayerAnimationManager AnimationManager { get; private set; }
    public static BuffManager<PlayerManager> BuffManager { get; private set; }
    public static PlayerInteractionManager InteractionManager { get; private set; }
    public static PlayerMovementManager MovementManager { get; private set; }
    public static PlayerSkillManager SkillManager { get; private set; }
    public static PlayerStatsManager StatsManager { get; private set; }

    //Informational Properties
    public static Vector3 BottomSphereOffset { get; private set; } 

    public static bool TeleportingThisFrame;

    //Initializes references, called from GameInitializer.
    public static void Initialize(
        GameObject player,
        GameObject sensor,
        GameObject objects,
        GameObject meleeObjects)
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

        //Components on player children assignments
        Sensor = sensor.GetComponent<PlayerSensor>();

        //System subpart initializations
        PhysicsSystem = new PhysicsSystem(Player, Capsule, Body, 1);
        MovementSystem = new AdvancedMovementSystem(Player, Capsule, PhysicsSystem);

        //Manager subpart initializations
        AbilityManager = new PlayerAbilityManager(Animator, PhysicsSystem, MovementSystem, Player);
        AnimationManager = new PlayerAnimationManager();
        BuffManager = new BuffManager<PlayerManager>(Manager);
        InteractionManager = new PlayerInteractionManager();
        MovementManager = new PlayerMovementManager();
        StatsManager = new PlayerStatsManager();
        SkillManager = new PlayerSkillManager();

        //Informational Properties
        BottomSphereOffset = Capsule.BottomSphereOffset();

        PlayerInfo.Manager.MaxOutStamina();
    }
}