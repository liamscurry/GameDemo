using UnityEngine;

//Weapon ability with a light melee attack.

public sealed class RangedEnemySlow : EnemyAbility 
{
    [SerializeField]
    private EnemyDamageHitbox hitbox;
    [SerializeField]
    private BoxCollider hitboxTrigger;
    [SerializeField]
    private AnimationClip rotateClip;
    [SerializeField]
    private AnimationClip pauseClip;
    [SerializeField]
    private AnimationClip attackClip;

    private AbilityProcess rotateProcess;
    private AbilityProcess pauseProcess;
    private AbilityProcess attackProcess;
    private AbilitySegment rotate;
    private AbilitySegment pause;
    private AbilitySegment attack;

    private const float damage = 1f;

    private float processTimer;
    private const float processDuration = 0.5f;

    public override void Initialize(EnemyAbilityManager abilityManager)
    {
        //Segment setup
        rotateProcess = new AbilityProcess(RotateBegin, DuringRotate, null, 1, true);
        pauseProcess = new AbilityProcess(null, null, null, 1);
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

        AttackDistance = 3f;
        AttackDistanceMargin = 1f;
        AttackAngleMargin = 5;
    }

    protected override void GlobalStart()
    {
        RangedEnemyManager manager = (RangedEnemyManager) ((EnemyAbilityManager) system).Manager;
        manager.DefensiveAttackSuccessful = false;
    }

    public override void GlobalUpdate()
    {
        ((EnemyAbilityManager) system).Manager.ClampToGround();
    }

    public void RotateBegin()
    {
        processTimer = 0;
    }

    public void DuringRotate()
    {
        Vector3 targetForward = Matho.StandardProjection3D(PlayerInfo.Player.transform.position - transform.position).normalized;
        Vector3 forward = Vector3.RotateTowards(transform.forward, targetForward, 3f * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        if (!(type == EnemyAbilityType.First))
        {
            processTimer += Time.deltaTime;
            if (processTimer > processDuration)
            {
                ActiveProcess.IndefiniteFinished = true;
            }
        }
        else
        {
            float deltaAngle = Matho.AngleBetween(forward, targetForward);

            if (deltaAngle < 5f)
            {
                ActiveProcess.IndefiniteFinished = true;
            }
        }
    }

    public void ActBegin()
    {
        hitbox.gameObject.SetActive(true);
        hitbox.Invoke(this);

        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, ((EnemyAbilityManager) system).Manager.GetGroundNormal());
        hitbox.transform.rotation = normalRotation * transform.rotation;
    }

    public void ActEnd()
    {
        hitbox.gameObject.SetActive(false);
    }

    public override bool OnHit(GameObject character)
    {
        RangedEnemyManager manager = (RangedEnemyManager) ((EnemyAbilityManager) system).Manager;
        manager.AbilityManager.CancelQueue();
        manager.DefensiveAttackSuccessful = true;
        character.GetComponentInParent<PlayerManager>().ChangeHealth(-damage);
        return true;
    }

    public override void ShortCircuitLogic()
    {
        ActEnd();
    }
}