using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//In the initialization phase, it initializes Game, Player and Enemy info classes' fields from inspector assignment, as well as other static reference classes.

public class GameInitializer : MonoBehaviour 
{
    [Header("Player")]
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject sensor;
    [SerializeField]
    private GameObject playerObjects;
    [SerializeField]
    private GameObject meleeObjects;
    [SerializeField]
    private ProjectilePool projectilePool;
    [SerializeField]
    private PickupPool pickupPool;

    public void Initialize()
    {
        //Reference initialization
        GameInfo.Initialize(gameObject, projectilePool, pickupPool);
        PlayerInfo.Initialize(player, sensor, playerObjects, meleeObjects);
        EnemyInfo.Initialize();
        GetComponent<GameSettings>().Initialize();
    }

}
