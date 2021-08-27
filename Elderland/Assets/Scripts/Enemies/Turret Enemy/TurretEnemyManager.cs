using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When spawning the turrets, use the turret enemy wall model prefab for back hitbox access.
// Make sure the back of the gizmos cube on the spawner is flush with the back of the doorway arch.
// Assumed to be on flat ground.
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
    [Header("The parent object that the cannon mesh is a child of.")]
    [SerializeField]
    private GameObject cannonGameObject;
    [Header("The armature parent transform from the FBX file.")]
    [SerializeField]
    private GameObject armatureParentObject; 
    [SerializeField]
    private GameObject[] otherBoneObjects;
    [SerializeField]
    private GameObject mainHitbox;
    [SerializeField]
    private GameObject backHitbox;
    [Header("Turret Enemy: UI")]
    [SerializeField]
    private GameObject healthbarLockIndicator;

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

    // Set in state machines, assigned in late update after internal animation update.
    public Quaternion CannonParentRotation { get; set; }
    public Vector3 CannonParentForward 
    { 
        get { return Matrix4x4.Rotate(CannonParentRotation).MultiplyPoint3x4(Vector3.forward); }
    }
    private Quaternion startArmatureParentRotation;

    public GameObject HealthbarLockIndicator { get { return healthbarLockIndicator; } }
    public const float DefensiveWallAngle = 110f; // angle for which the defensive mode should proc from wall normal.

    protected override void Initialize() 
    {
        DeclareAbilities();
        DeclareType();

        wallForward = transform.forward;
        wallRight = transform.right;
        StatsManager.Interuptable = false; // Needed to ignore iterrupt calls as turrets don't flinch when damaged.
        CannonParentRotation = cannonGameObject.transform.rotation;
    }

    /*
    Helper update needed to be called after animation update state behaviour to get correct rotation.

    Inputs:
    None

    Outputs:
    None
    */
    private void LateUpdate()
    {  
        armatureParentObject.transform.rotation = CannonParentRotation * Quaternion.Euler(0, 90, 0);
        //cannonGameObject.transform.rotation = CannonParentRotation;   
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

    /*
    See EnemyManager for original use. Turret enemy does not move and therefore does not have a return
    state.

    Inputs:
    None

    Outputs:
    None.
    */
    public override void TryBoundsReturn() {}

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