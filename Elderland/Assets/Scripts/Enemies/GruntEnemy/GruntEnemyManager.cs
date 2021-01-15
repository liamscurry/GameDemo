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
    private float centralStopRadius;
    [SerializeField]
    private float centralStopRadiusMargin;
    [SerializeField]
    private float attackFollowRadius;
    [SerializeField]
    private float attackFollowRadiusMargin;
    [SerializeField]
    private float attackPingRadius;
    [SerializeField]
    private float attackFollowSpeed;
    [SerializeField]
    private float fightingAgentRadius;
    [SerializeField]
    private float followAgentRadius;
    [SerializeField]
    private GruntEnemyNearbySensor nearbySensor;
    [SerializeField]
    private GruntEnemyGroupSensor groupSensor;

    public const float ExpandSpeed = 0.5f;

    public float GroupFollowRadius { get { return groupFollowRadius; } }
    public float GroupFollowRadiusMargin { get { return groupFollowRadiusMargin; } }
    public float CentralStopRadius { get { return centralStopRadius; } }
    public float CentralStopRadiusMargin { get { return centralStopRadiusMargin; } }
    public float AttackFollowRadius { get { return attackFollowRadius; } }
    public float AttackFollowRadiusMargin { get { return attackFollowRadiusMargin; } }
    public float AttackPingRadius { get { return attackPingRadius; } }
    public float AttackFollowSpeed { get { return attackFollowSpeed; } }
    public GruntEnemyNearbySensor NearbySensor { get { return nearbySensor; } }
    public GruntEnemyGroupSensor GroupSensor { get { return groupSensor; } }
    public bool GroupMovement { get; set; }
    public bool PingedToAttack { get; set; }
    public bool PingedToGroup { get; set; }
    public float FightingAgentRadius { get { return fightingAgentRadius; } }
    public float FollowAgentRadius { get { return followAgentRadius; } }

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

    public GruntEnemySword Sword { get; private set; }

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

    private void OnDestroy()
    {
        if (EnemyGroup.AttackingEnemies.Contains(this))
        {
            EnemyGroup.AttackingEnemies.Remove(this);
        }
    }

    protected override void DeclareAbilities()
    {
        Sword = GetComponent<GruntEnemySword>();
        AbilityManager.ApplyAbility(Sword);

        MaxHealth = 0.25f;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Melee;
    }

    public override void ChooseNextAbility()
    {
        NextAttack = Sword;
    }

    public void UpdateAgentPath()
    {
        Agent.destination = PlayerNavMeshPosition();
    }

    public void UpdateAgentPathOffset(Vector3 offset)
    {
        Agent.destination = PlayerNavMeshPosition(offset);
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

    public void RotateTowardsPlayer()
    {
        if (!GroupMovement)
        {
            if (!Agent.updateRotation)
                Agent.updateRotation = true;
        }
        else
        {
            if (Agent.updateRotation)
            {
                Agent.updateRotation = false;
            }
            else
            {
                Vector3 targetForward =
                    Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
                Vector3 forward =
                    Vector3.RotateTowards(transform.forward, targetForward, 1f * Time.deltaTime, 0f);
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }

    public void RotateLocallyTowardsPlayer()
    {
        Vector3 targetForward =
            Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward =
            Vector3.RotateTowards(transform.forward, targetForward, 1f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}