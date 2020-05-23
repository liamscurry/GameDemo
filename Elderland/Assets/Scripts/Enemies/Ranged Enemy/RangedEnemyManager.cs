using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class RangedEnemyManager : EnemyManager
{
    public RangedEnemyDash Dash { get; private set; }
    public RangedEnemyShoot Shoot { get; private set; }
    public RangedEnemySlow Slow { get; private set; }

    public bool DefensiveAttackSuccessful { get; set; }

    public int index;
    public int direction;
    public List<Vector3> path;
    public int ignoreIndex;
    public GameObject waitingParent;

    private const float defensiveRange = 3.5f;
    private const float defensiveRangeMargin = 2f;

    public const float WalkSpeed = 2.5f;
    public const float RunAwaySpeed = 3.5f;
    public const float LimpAwaySpeed = 0.75f;

    protected override void DeclareAbilities()
    {
        Dash = GetComponent<RangedEnemyDash>();
        AbilityManager.ApplyAbility(Dash);

        Shoot = GetComponent<RangedEnemyShoot>();
        AbilityManager.ApplyAbility(Shoot);

        Slow = GetComponent<RangedEnemySlow>();
        AbilityManager.ApplyAbility(Slow);

        ignoreIndex = -1;
        MaxHealth = 3;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Ranged;
    }

    protected override void SpawnPickups()
    {
        Pickup.SpawnPickups<HealthPickup>(
            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
            transform.position,
            3,
            3f,
            90f);
    }

    public override void Freeze()
    {
        TurnOffAgent();

        PhysicsSystem.Animating = true;

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        Animator.SetBool("stationary", false);
        Animator.SetBool("waiting", false);
        Animator.SetBool("defensive", false);
        Animator.ResetTrigger("defensiveStart");
        Animator.ResetTrigger("defensiveExit");
    }

    public override void ChooseNextAbility()
    {
        NextAttack = Shoot;
        Shoot.Queue(EnemyAbilityType.First);
        Shoot.Queue(EnemyAbilityType.Last);
        Dash.Queue(EnemyAbilityType.Last);
    }
    
    public bool IsInDefensiveRange()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        return horizontalDistanceToPlayer <= defensiveRange;
    } 

    public bool IsOutOfDefensiveRange()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        return horizontalDistanceToPlayer > defensiveRange + defensiveRangeMargin;
    } 

    public bool HasClearPlacement()
    {
        Vector2 castPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        castPosition = Vector2.MoveTowards(castPosition, Matho.StandardProjection2D(transform.position), 1f);
        Vector3 playerNav = GameInfo.CurrentLevel.NavCast(castPosition);
        NavMeshHit groundHit;
        
        Vector3 direction = PlayerInfo.Player.transform.position - transform.position;
        float distance = direction.magnitude;
        RaycastHit enemyHit;

        Vector2 rectangleCenter = Matho.StandardProjection2D(Vector3.MoveTowards(PlayerInfo.Player.transform.position, transform.position, 7f));
        rectangleCenter += Matho.StandardProjection2D(direction).normalized;
        Vector3 rectangleCenterNav = GameInfo.CurrentLevel.NavCast(rectangleCenter);
        Vector3 rectangleSize = new Vector3(2, 3, 1f - 0.1f);
        Quaternion rectangleRotation = Quaternion.LookRotation(Matho.StandardProjection3D(direction).normalized, Vector3.up);

        Collider[] enemies = Physics.OverlapBox(rectangleCenterNav, rectangleSize / 2f, rectangleRotation, LayerConstants.EnemyHitbox);

        return (!Agent.Raycast(playerNav, out groundHit) &&
        !Physics.SphereCast(transform.position, Capsule.radius - 0.1f, direction, out enemyHit, distance, LayerConstants.Enemy)
        && (enemies.Length == 0 || (enemies.Length == 1 && enemies[0].GetComponentInParent<EnemyManager>() == this)));
    }

    public bool ScreenForWaiting()
    {
        Vector3 direction = PlayerInfo.Player.transform.position - transform.position;
        Vector3 rectangleCenter = transform.position + transform.forward;
        Vector3 rectangleSize = new Vector3(2, 3, 2f - 0.1f);
        Quaternion rectangleRotation = Quaternion.LookRotation(Matho.StandardProjection3D(direction).normalized, Vector3.up);        
        Collider[] enemies = Physics.OverlapBox(rectangleCenter, rectangleSize / 2f, rectangleRotation, LayerConstants.EnemyHitbox);
        foreach (Collider enemy in enemies)
        {
            if (enemy.transform.parent.gameObject != gameObject && enemy.GetComponentInParent<RangedEnemyManager>() != null)
            {
                if (enemy.transform.parent.GetComponent<Animator>().GetBool("waiting"))
                {
                    waitingParent = enemy.transform.parent.gameObject;
                    return true;
                }
            }
        }
        return false;
    }

    protected override void OnDrawGizmos()
    {
        Vector3 direction = PlayerInfo.Player.transform.position - transform.position;
        Vector2 rectangleCenter = Matho.StandardProjection2D(Vector3.MoveTowards(PlayerInfo.Player.transform.position, transform.position, 7f));
        rectangleCenter += Matho.StandardProjection2D(direction).normalized;
        Vector3 rectangleCenterNav = GameInfo.CurrentLevel.NavCast(rectangleCenter);
        Vector3 rectangleSize = new Vector3(2, 3, 1f - 0.1f);
        Quaternion rectangleRotation = Quaternion.LookRotation(Matho.StandardProjection3D(direction).normalized, Vector3.up);
        Gizmos.matrix = Matrix4x4.TRS(rectangleCenterNav, rectangleRotation, Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(Vector3.zero, rectangleSize);
    }
}