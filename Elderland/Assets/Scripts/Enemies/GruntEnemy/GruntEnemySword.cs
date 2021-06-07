using UnityEngine;

//Weapon ability with a grunt melee attack.

public sealed class GruntEnemySword : EnemyAbility 
{
    [SerializeField]
    private EnemyDamageHitbox hitbox;
    [SerializeField]
    private BoxCollider hitboxTrigger;
    [SerializeField]
    private GameObject hitboxPredictor;
    [Header("Clips")]
    [Header("1")]
    [SerializeField]
    private AnimationClip rotateClip;
    [SerializeField]
    private AnimationClip pauseClip;
    [SerializeField]
    private AnimationClip attackClip;
    [Header("2")]
    [SerializeField]
    private AnimationClip rotateClip2;
    [SerializeField]
    private AnimationClip pauseClip2;
    [SerializeField]
    private AnimationClip attackClip2;
    [Header("3")]
    [SerializeField]
    private AnimationClip rotateClip3;
    [SerializeField]
    private AnimationClip pauseClip3;
    [SerializeField]
    private AnimationClip attackClip3;

    //Fields
    private float damage = 0.5f;
    
    private AbilityProcess rotateProcess;
    private AbilityProcess pauseProcess;
    private AbilityProcess attackProcess;
    private AbilitySegment rotate;
    private AbilitySegment pause;
    private AbilitySegment attack;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        //Segment setup
        rotateProcess = new AbilityProcess(null, DuringRotate, null, 1);
        pauseProcess = new AbilityProcess(PauseBegin, null, PauseEnd, 1);
        attackProcess = new AbilityProcess(ActBegin, null, ActEnd, 0.25f);
        rotate = new AbilitySegment(rotateClip, rotateProcess);
        pause = new AbilitySegment(pauseClip, pauseProcess);
        attack = new AbilitySegment(attackClip, attackProcess);
        segments = new AbilitySegmentList();
        segments.AddSegment(rotate);
        segments.AddSegment(pause);
        segments.AddSegment(attack);
        segments.NormalizeSegments();

        //Specifications
        this.system = abilityManager;

        AttackDistance = 1.0f;
        AttackDistanceMargin = 0.5f;
        AttackAngleMargin = 5;
    }

    protected override void GlobalStart()
    {
        int swingType = ((new System.Random()).Next() % 3) + 1;
        switch (swingType)
        {
            case 1:
                rotate.Clip = rotateClip;
                pause.Clip = pauseClip;
                attack.Clip = attackClip;
                break;
            case 2:
                rotate.Clip = rotateClip2;
                pause.Clip = pauseClip2;
                attack.Clip = attackClip2;
                break;
            case 3:
                rotate.Clip = rotateClip3;
                pause.Clip = pauseClip3;
                attack.Clip = attackClip3;
                break;
            default:
                throw new System.Exception("Grunt enemy swing animation not implemented.");
        }
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    public void DuringRotate()
    {
        Vector3 targetForward = 
            Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void PauseBegin()
    {
        hitboxPredictor.SetActive(true);

        SetHitboxRotation(hitboxPredictor);
    }

    private void PauseEnd()
    {
        hitboxPredictor.SetActive(false);
    }

    public void ActBegin()
    {
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this);

        SetHitboxRotation(this.hitbox.gameObject);
        ((EnemyAbilityManager) system).Manager.StatsManager.Interuptable = false;
    }

    public void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
        ((EnemyAbilityManager) system).Manager.StatsManager.Interuptable = true;
    }

    public override bool OnHit(GameObject character)
    {
        character.GetComponentInParent<PlayerManager>().ChangeHealth(-damage);
        EnemyManager manager = ((EnemyAbilityManager) system).Manager;

        if (PlayerInfo.StatsManager.Blocking && manager.Health > manager.ZeroHealth)
        {
            ShortCircuit();
            manager.Animator.SetTrigger("toDeflected");
        }

        return true;
    }

    public override void ShortCircuitLogic()
    {
        hitboxPredictor.SetActive(false);

        ActEnd();

        // Assuming shortcut means from outside source,
        // thus removing from attack group.
        var manager = ((EnemyAbilityManager) system).Manager as GruntEnemyManager;
        EnemyGroup.AttackingEnemies.Remove(manager);
        manager.Agent.radius = manager.FollowAgentRadius;
        manager.Agent.stoppingDistance = 0;
        manager.Agent.ResetPath();
        EnemyGroup.RemoveAttacking(manager);
        manager.NearbySensor.transform.localScale = manager.NearbySensor.BaseRadius * Vector3.one;
    }

    private void SetHitboxRotation(GameObject hitbox)
    {
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, ((EnemyAbilityManager) system).Manager.GetGroundNormal());
        hitbox.transform.rotation = normalRotation * transform.rotation;
    }
}