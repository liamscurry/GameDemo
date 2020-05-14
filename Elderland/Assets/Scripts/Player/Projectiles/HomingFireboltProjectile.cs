using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Standard projectile with no unique properties.

public sealed class HomingFireboltProjectile : ParticleProjectile 
{
    [SerializeField]
    private HomingFireballSensor sensor;

    private bool seekingEnemy; 
    private EnemyManager targetEnemy;
    private float maxSpeed;

    private List<HomingFireboltProjectile> group;
    private float groupID;

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
        group = new List<HomingFireboltProjectile>();
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
        group.Clear();
    }

    protected override void OnDeath(GameObject target)
    {
        if (alive)
        {
            //Trail particle
            trailParticle.Stop();
            //Death particle
            deathParticle.gameObject.SetActive(true);
            deathParticle.Clear();
            deathParticle.Play();

            alive = false;
            this.enabled = false;
            body.velocity = Vector3.zero;
            if (hitTarget != null)
                hitTarget(target);

            targetEnemy = null;
            groupID = -1;
        }
	}

    public void SetGroupInformation(List<HomingFireboltProjectile> group, int groupID)
    {
        foreach (HomingFireboltProjectile projectile in group)
        {
            if (projectile != this)
            {
                this.group.Add(projectile);
            }
        }

        this.groupID = groupID;
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
            TurnTowardsEnemy();

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
    
    private void TurnTowardsEnemy()
    {
        float distanceBetween =
            (targetEnemy.transform.position - transform.position).magnitude;

        Vector3 direction = velocity.normalized;
        Vector3 targetDirection =
            (targetEnemy.transform.position - transform.position).normalized;
        Vector3 interpolatedDirection = 
            Matho.RotateTowards(direction,
                                targetDirection,
                                350f * Time.deltaTime);

        float percentageBetween =
            Mathf.Clamp01(Matho.AngleBetween(direction, targetDirection) / 45f);
        float turnSpeed = (1f - 0.90f * percentageBetween) * maxSpeed;

        velocity = interpolatedDirection * turnSpeed;
    }

    private bool EnemiesLOSCheck()
    {
        foreach (EnemyManager enemy in sensor.EnemiesInRange)
        {
            if (enemy.Health == 0)
                continue;

            bool groupAlreadyHoming = false;
            foreach (HomingFireboltProjectile projectile in group)
            {
                if (projectile.targetEnemy == enemy &&
                    groupID == projectile.groupID)
                {
                    groupAlreadyHoming = true;
                    break;
                }
            }
            if (groupAlreadyHoming)
                continue;

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