using UnityEngine;

//Weapon ability with a grunt melee attack.

public sealed class TurretEnemyShoot : EnemyAbility 
{
    [SerializeField]
    private AnimationClip pauseClip;
    [SerializeField]
    private AnimationClip attackClip;
    [SerializeField]
    private float lifeTime;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float shootOffset;

    //Fields
    private float damage = 0.5f;
    
    private AbilityProcess pauseProcess;
    private AbilityProcess attackProcess;
    private AbilitySegment pause;
    private AbilitySegment attack;
    private TurretEnemyManager manager;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        //Segment setup
        pauseProcess = new AbilityProcess(PauseBegin, null, PauseEnd, 1);
        attackProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.25f);

        pause = new AbilitySegment(pauseClip, pauseProcess);
        attack = new AbilitySegment(attackClip, attackProcess);
        segments = new AbilitySegmentList();

        segments.AddSegment(pause);
        segments.AddSegment(attack);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;

        AttackDistance = 1.75f;
        AttackDistanceMargin = 0.5f;
        AttackAngleMargin = 5;

        manager = GetComponent<TurretEnemyManager>();
    }

    private void PauseBegin()
    {
        
    }

    private void PauseEnd()
    {
        
    }

    public void ActBegin()
    {

    }

    public void ActEnd()
    {
        Vector3 start = manager.MeshParent.transform.position + shootOffset * manager.MeshParent.transform.forward;
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

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}