using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//In the initialization phase, it initializes Game, Player and Enemy info classes' fields from inspector assignment, as well as other static reference classes.

public class GameInitializer : MonoBehaviour 
{
    [SerializeField]
    private SaveManager saveManager;
    [SerializeField]
    private GameObject menuManager;

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
    [SerializeField]
    private Transform cooldownOriginTransform;
    [Header("Universal Enemy Info")]
    [SerializeField]
    private Color ArmorColor;
    [SerializeField]
    private Color HealthColor;
    [SerializeField]
    private Color FinisherHealthColor;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float shadowColorDim;

    public void Initialize()
    {
        //Reference initialization
        GameInfo.Initialize(saveManager, menuManager, gameObject, projectilePool, pickupPool);
        PlayerInfo.Initialize(player, sensor, playerObjects, meleeObjects, cooldownOriginTransform);
        EnemyInfo.Initialize(ArmorColor, HealthColor, FinisherHealthColor, shadowColorDim);
        GetComponent<GameSettings>().Initialize();
    }

}
