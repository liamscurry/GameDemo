using System;
using UnityEngine;

// Standard projectile with no unique properties.

public sealed class HomingFireboltProjectile : ParticleProjectile 
{
    [SerializeField]
    private HomingFireballSensor sensor;

    private bool seekingEnemy; 
    private EnemyManager targetEnemy;
    private float maxSpeed;

    public override void Initialize(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        // Base
        transform.position = position;
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;
        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        // Unique
        trailParticle.Clear();
        trailParticle.Play();

        // Unique
        seekingEnemy = false;
        targetEnemy = null;

        maxSpeed = velocity.magnitude;
    }

    public override void Reset(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        // Base
        transform.position = position;
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;
        this.enabled = true;

        // Unique
        trailParticle.Clear();
        trailParticle.Play();

        // Unique
        sensor.Reset();
        seekingEnemy = false;
        targetEnemy = null;

        maxSpeed = velocity.magnitude;
    }

    protected override void Update()
    {
        if (alive)
        {
            HomingUpdate();
        }

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration && alive)
        { 
            normal = Vector2.zero;
            OnDeath(null); 
        }
        
        lastPosition = transform.position;
    }

    private void HomingUpdate()
    {
        if (seekingEnemy)
        {
            // Edit velocity so the projectile can move towards enemy.
            Vector3 direction = velocity.normalized;
            Vector3 targetDirection =
                (targetEnemy.transform.position - transform.position).normalized;
            Vector3 interpolatedDirection = 
                Matho.RotateTowards(direction, targetDirection, 300f * Time.deltaTime);

            float percentageBetween =
                Mathf.Clamp01(Matho.AngleBetween(direction, targetDirection) / 90f);
            float turnSpeed = (1f - 0.90f * percentageBetween) * maxSpeed;

            velocity = interpolatedDirection * turnSpeed;

            float distanceToEnemy = 
                Vector3.Distance(targetEnemy.transform.position, transform.position);

            if (distanceToEnemy > sensor.Range)
            {
                seekingEnemy = false;
                targetEnemy = null;
            }
        }
        else
        {
            // Want to see if enemy is visible in range.
            if (sensor.EnemyInRange)
            {
                seekingEnemy = EnemiesLOSCheck();
            }
        }
    }

    private bool EnemiesLOSCheck()
    {
        foreach (EnemyManager enemy in sensor.EnemiesInRange)
        {
            if (!Physics.Linecast(transform.position,
                                enemy.transform.position,
                                LayerConstants.GroundCollision))
            {
                targetEnemy = enemy;
                return true;
            }
        }

        return false;
    }

    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<HomingFireboltProjectile>(gameObject);
    }
}