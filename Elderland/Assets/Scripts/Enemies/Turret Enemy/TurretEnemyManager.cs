using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TurretEnemyManager : EnemyManager
{
    [SerializeField]
    private float defensiveRadius;
    [SerializeField]
    private float defensiveRadiusMargin;
    [SerializeField]
    private float searchRadius;
    [SerializeField]
    private float searchRadiusMargin;
    [SerializeField]
    private float passiveSearchSpeed;
    [SerializeField]
    [Range(0.0f, 90f)]
    private float passiveSearchConeAngle;
    [SerializeField]
    private float activeSearchSpeed;
    [SerializeField]
    private float defensiveRotateSpeed;
    [SerializeField]
    private GameObject meshParent;
    [SerializeField]
    private GameObject mainHitbox;
    [SerializeField]
    private GameObject backHitbox;

    private Vector3 wallForward;
    private Vector3 wallRight;

    public float DefensiveRadius { get { return defensiveRadius; } }
    public float DefensiveRadiusMargin { get { return defensiveRadiusMargin; } }
    public float SearchRadius { get { return searchRadius; } }
    public float SearchRadiusMargin { get { return searchRadiusMargin; } }
    public float PassiveSearchSpeed { get { return passiveSearchSpeed; } }
    public float PassiveSearchConeAngle { get { return passiveSearchConeAngle; } }
    public float ActiveSearchSpeed { get { return activeSearchSpeed; } }
    public float DefensiveRotateSpeed { get { return defensiveRotateSpeed; } }
    public GameObject MeshParent { get { return meshParent; } }
    public GameObject MainHitbox { get { return mainHitbox; } }
    public GameObject BackHitbox { get { return backHitbox; } }
    public Vector3 WallForward { get { return wallForward; } }
    public Vector3 WallRight { get { return wallRight; } }
    public bool InDefensive { get; set; }

    public TurretEnemyShoot Cannon { get; private set; }

    protected override void Initialize() 
    {
        DeclareAbilities();
        DeclareType();

        wallForward = transform.forward;
        wallRight = transform.right;
    }

    protected override void FixedUpdate()
    {
        AbilityManager.FixedUpdateAbilities();
    }

    protected override void DeclareAbilities()
    {
        Cannon = GetComponent<TurretEnemyShoot>();
        AbilityManager.ApplyAbility(Cannon);

        MaxHealth = 4f;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Ranged;
    }

    public override void ChooseNextAbility()
    {
        NextAttack = Cannon;
    }

    public Vector3 PlayerNavMeshPosition()
    {
        Vector2 projectedPlayerPosition = 
            Matho.StdProj2D(PlayerInfo.Player.transform.position);
        Vector3 targetPosition =
            GameInfo.CurrentLevel.NavCast(projectedPlayerPosition);
        return targetPosition;
    }

    public Vector3 PlayerNavMeshPosition(Vector3 offset)
    {
        Vector2 projectedPlayerPosition = 
            Matho.StdProj2D(PlayerInfo.Player.transform.position + offset);
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

    /*
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
    }*/

    /*
    public void RotateLocallyTowardsPlayer()
    {
        Vector3 targetForward =
            Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward =
            Vector3.RotateTowards(transform.forward, targetForward, 1f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }*/
}