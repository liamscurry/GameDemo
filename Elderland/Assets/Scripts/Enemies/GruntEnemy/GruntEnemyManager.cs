using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class GruntEnemyManager : EnemyManager, IEnemyGroup
{
    //public LightEnemySword Sword { get; private set; }
    [Header("Grunt Enemy: Dynamic Enemy Group")]
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
    private float shrinkRadius;
    [SerializeField]
    private GruntEnemyNearbySensor nearbySensor;
    [SerializeField]
    private GruntEnemyGroupSensor groupSensor;
    [Header("Grunt Enemy: Art")]
    [SerializeField]
    private Animator fragmentAnimator;

    public const float ExpandSpeed = 3.5f;
    public const float ShrinkSpeed = 1.5f;

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
    public bool PingedToAttack { get; set; }
    public bool PingedToGroup { get; set; }
    public float FightingAgentRadius { get { return fightingAgentRadius; } }
    public float FollowAgentRadius { get { return followAgentRadius; } }
    public float ShrinkRadius { get { return shrinkRadius; } }

    public EnemyGroup Group { get; set; }
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }
    public Vector3 Velocity { get; set; }
    public bool UpdatingRotation { get; set; }
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

    // Animation
    private Animator animator;
    private Vector2 positionAnalogDirection;
    private const float positionAnalogSpeed = 1.7f;
    private Vector3 lastPosition;
    private float currentPercSpeed;
    private const float minMoveDistance = 0.01f;
    private const float acceleration = 7f;

    private void LateUpdate()
    {
        if (Group != null)
        {
            if (Agent.hasPath)
            {
                Agent.ResetPath();
            }
            else
            {
                Group.ResetAdjust();
            }
        }

        UpdateAnimatorProperties();
    }

    protected override void OnHealthZero()
    {
        base.Die();
        fragmentAnimator.Play("Die");
    }

    private void OnDestroy()
    {
        EnemyGroup.RemoveAttacking(this);
    }

    protected override void Initialize() 
    {
        DeclareAbilities();
        DeclareType();
        animator = GetComponentInChildren<Animator>();
        lastPosition = transform.position;
        currentPercSpeed = 0;
    }

    protected override void DeclareAbilities()
    {
        Sword = GetComponent<GruntEnemySword>();
        AbilityManager.ApplyAbility(Sword);

        MaxHealth = 3f;
        Health = MaxHealth;
        MaxArmor = 0f;
        Armor = MaxArmor;
        FinisherHealth = 0.5f;
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
        return PlayerInfo.Player.transform.position - PlayerInfo.BottomSphereOffset;
    }

    public Vector3 PlayerNavMeshPosition(Vector3 offset)
    {
        return 
            PlayerInfo.Player.transform.position -
            PlayerInfo.BottomSphereOffset +
            Matho.StdProj3D(offset);
    }

    public void UpdateSpawnPath()
    {
        Agent.destination = EncounterSpawn.spawnPosition;
    }

    protected override void SpawnPickups()
    {
        // Will be a percent chance to drop one health pickup.
        /*
        Pickup.SpawnPickups<HealthPickup>(
            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
            transform.position,
            4,
            3f,
            90f);
            */
    }

    private void UpdateAnimatorProperties()
    {
        UpdateWalkProperties();

        lastPosition = transform.position;
    }

    private void UpdateWalkProperties()
    {
        Vector2 forward = Matho.StdProj2D(transform.forward).normalized;
        Vector2 right = Matho.Rotate(forward, 90);

        Vector2 scaledCurrentDir =
            Matho.StdProj2D(transform.position - lastPosition).normalized; 
        float deltaMag = Matho.StdProj2D(transform.position - lastPosition).magnitude;

        if (deltaMag > minMoveDistance * StatsManager.MovespeedMultiplier.Value)
        {
            currentPercSpeed += acceleration * Time.deltaTime;
            if (currentPercSpeed > 1)
                currentPercSpeed = 1;
        }
        else
        {
            currentPercSpeed -= acceleration * Time.deltaTime;
            if (currentPercSpeed < 0)
                currentPercSpeed = 0;
        }

        Vector2 analogDirection = 
            new Vector2(
                Matho.ProjectScalar(scaledCurrentDir, forward),
                Matho.ProjectScalar(scaledCurrentDir, right));

        positionAnalogDirection =
            Vector2.MoveTowards(positionAnalogDirection, analogDirection, positionAnalogSpeed * Time.deltaTime);

        animator.SetFloat(
            "speed",
            positionAnalogDirection.x);
        animator.SetFloat(
            "strafe",
            positionAnalogDirection.y);
        animator.SetFloat(
            "percentileSpeed",
            currentPercSpeed);
    }
}