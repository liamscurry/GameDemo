using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GruntEnemyManager : EnemyManager, IEnemyGroup
{
    //public LightEnemySword Sword { get; private set; }

    [SerializeField]
    private float groupFollowRadius;
    [SerializeField]
    private float groupFollowRadiusMargin;
    [SerializeField]
    private GruntEnemyNearbySensor nearbySensor;
    [SerializeField]
    private GruntEnemyGroupSensor groupSensor;

    public float GroupFollowRadius { get { return groupFollowRadius; } }
    public float GroupFollowRadiusMargin { get { return groupFollowRadiusMargin; } }
    public GruntEnemyNearbySensor NearbySensor { get { return nearbySensor; } }
    public GruntEnemyGroupSensor GroupSensor { get { return groupSensor; } }
    public bool GroupMovement { get; set; }

    public EnemyGroup Group { get; set; }
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }
    public Vector3 Velocity { get; set; }
    public List<IEnemyGroup> NearbyEnemies
    {
        get
        {
            return nearbySensor.NearbyGrunts;
        }

        set
        {
            Debug.Log("GruntEnemyManager nearby grunts cannot be set. This message should not be printed");
        }
    }

    public bool InGroupState { get; set; }

    public int CompareTo(IEnemyGroup e)
    {
        if (((IEnemyGroup) this) == e)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    private void LateUpdate()
    {
        if (Group != null)
        {
            if (Agent.hasPath)
            {
                GroupMovement = true;
                Agent.ResetPath();
            }
            else
            {
                Group.ResetAdjust();
            }
        }
    }

    protected override void DeclareAbilities()
    {
        //Sword = GetComponent<LightEnemySword>();
        //AbilityManager.ApplyAbility(Sword);

        MaxHealth = 1;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Melee;
    }

    public override void ChooseNextAbility()
    {
        //NextAttack = Sword;
    }

    public void UpdateAgentPath()
    {
        Agent.destination = PlayerNavMeshPosition();
    }

    public Vector3 PlayerNavMeshPosition()
    {
        Vector2 projectedPlayerPosition = 
            Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        Vector3 targetPosition =
            GameInfo.CurrentLevel.NavCast(projectedPlayerPosition);
        return targetPosition;
    }

    public Vector3 PlayerNavMeshPosition(Vector3 offset)
    {
        Vector2 projectedPlayerPosition = 
            Matho.StandardProjection2D(PlayerInfo.Player.transform.position + offset);
        Vector3 targetPosition =
            GameInfo.CurrentLevel.NavCast(projectedPlayerPosition);
        return targetPosition;
    }

    protected override void SpawnPickups()
    {
        // Will be a percent chance to drop one health pickup.
        /*
        Pickup.SpawnPickups<HealthPickup>(
            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
            transform.position,
            1,
            3f,
            90f);
            */
    }
}