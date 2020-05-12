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

    public static EnemyLevel CurrentLevel { get; set; }

    //States
    public static bool Panning { get; set; }
    public static bool Paused { get; set; }


    //Initializes references, called from GameInitializer.
    public static void Initialize(GameObject manager, ProjectilePool projectilePool)
    {
        //Systems
        Manager = manager.GetComponent<GameManager>();
        Menu = manager.GetComponent<MenuManager>();
        Settings = manager.GetComponent<GameSettings>();

        //References
        CameraController = Camera.main.GetComponent<CameraController>();
        ProjectilePool = projectilePool;

        //States
        Paused = false;
        Panning = false;
    }
}
