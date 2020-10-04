using UnityEngine;

//Weapon ability that casts Fireball projectiles.

public sealed class RangedEnemyShoot : EnemyAbility
{
    [SerializeField]
    private AnimationClip shootClip;
    [SerializeField]
    [Range(0f, 30f)]
    private float speed;
    [SerializeField]
    private float lifeTime;

    private AbilitySegment shoot;
    private AbilityProcess shootProcess;
    private AbilityProcess checkProcess;

    private RangedEnemyManager manager;

    private bool exiting;

    private const float damage = 0.5f;

    public override void Initialize(EnemyAbilityManager abilityManger)
    {
        //Specifications
        this.system = abilityManger;

        shootProcess = new AbilityProcess(null, DuringShoot, ShootEnd, 0.25f);
        checkProcess = new AbilityProcess(null, null, CheckEnd, 0.75f);
        shoot = new AbilitySegment(shootClip, shootProcess, checkProcess);

        segments = new AbilitySegmentList();
        segments.AddSegment(shoot);
        segments.NormalizeSegments();

        AttackDistance = EnemyInfo.RangedArranger.radius;
        AttackDistanceMargin = 3.5f;
        AttackAngleMargin = 5;

        manager = ((RangedEnemyManager) abilityManger.Manager);
    }

    protected override void GlobalStart()
    {
        exiting = false;
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    public void DuringShoot()
    {
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

	public void ShootEnd()
    {
        Vector3 start = transform.position + manager.Capsule.height / 4f * Vector3.up;
        Vector3 end = PlayerInfo.Player.transform.position + PlayerInfo.Capsule.height / 4 * Vector3.up;
        Vector3 direction = (end - start).normalized;
        Vector3 velocity = speed * direction;

        GameInfo.ProjectilePool.Create<RangedEnemyProjectile>(
                Resources.Load<GameObject>(ResourceConstants.Enemy.Projectiles.RangedEnemyProjectile),
                start,
                velocity,
                lifeTime,
                TagConstants.PlayerHitbox,
                OnHit,
                null);
    }

    public void CheckEnd()
    {
        OffseniveCheck();
        if (!exiting) 
            DefensiveCheck();
    }

    public override bool OnHit(GameObject character)
    {
        if (character != null)
        {
            character.GetComponentInParent<PlayerManager>().ChangeHealth(-damage);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OffseniveCheck()
    {
        if (!manager.IsInNextAttackMax())
        {
            OffensiveExit();
        }
    }

    private void DefensiveCheck()
    {
        if (manager.IsInDefensiveRange())
        {
            DefensiveExit();
        }
    }

    private void OffensiveExit()
    {
        manager.AbilityManager.CancelQueue();
        exiting = true;
    }

    private void DefensiveExit()
    {
        manager.AbilityManager.CancelQueue();

        manager.NextAttack = manager.Slow;
        manager.Slow.Queue(EnemyAbilityType.First);
        //manager.Slow.Queue(EnemyAbilityType.Middle);
        manager.Slow.Queue(EnemyAbilityType.Last);
        manager.Animator.SetBool("defensive", true);
        manager.Animator.ResetTrigger("runAbility");
        exiting = true;
    }

    public override void ShortCircuitLogic()
    {
        CheckEnd();
    }
}