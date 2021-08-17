using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When spawning the turrets, use the turret enemy wall model prefab for back hitbox access.
// Make sure the back of the gizmos cube on the spawner is flush with the back of the doorway arch.
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
    private GameObject cannonGameObject;
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
    public GameObject CannonGameObject { get { return cannonGameObject; } }
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
        StatsManager.Interuptable = false; // Needed to ignore iterrupt calls as turrets don't flinch when damaged.
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

    protected override void SpawnPickups() {}
}