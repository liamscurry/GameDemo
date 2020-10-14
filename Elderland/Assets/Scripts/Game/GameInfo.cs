using UnityEngine;

//Contains references to game required objects. Ex: Pathfinder 

public static class GameInfo 
{
    //System References
    public static GameManager Manager { get; private set; }
    public static MenuManager Menu { get; private set; }
    public static GameSettings Settings { get; private set; }

    //References
    public static CameraController CameraController { get; private set; }

    public static ProjectilePool ProjectilePool { get; private set; }
    public static PickupPool PickupPool { get; private set; }

    public static EnemyLevel CurrentLevel { get; set; }
    public static Transform RespawnTransformNoLevel { get; set; }

    //States
    public static bool Panning { get; set; }
    public static bool Paused { get; set; }


    //Initializes references, called from GameInitializer.
    public static void Initialize(
        GameObject menuManager,
        GameObject manager,
        ProjectilePool projectilePool,
        PickupPool pickupPool)
    {
        //Systems
        Manager = manager.GetComponent<GameManager>();
        Menu = menuManager.GetComponent<MenuManager>();
        Settings = manager.GetComponent<GameSettings>();

        //References
        CameraController = Camera.main.GetComponent<CameraController>();
        ProjectilePool = projectilePool;
        PickupPool = pickupPool;

        //States
        Paused = false;
        Panning = false;
    }
}
