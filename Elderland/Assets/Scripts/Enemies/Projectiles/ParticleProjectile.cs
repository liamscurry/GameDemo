using System;
using UnityEngine;

//Standard projectile with lights and particles.

public abstract class ParticleProjectile : Projectile 
{
    [SerializeField]
    protected ParticleSystem trailParticle;
    [SerializeField]
    protected ParticleSystem deathParticle;

    public override void Initialize(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        //Base
        transform.position = position;
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;
        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        //Unique
        trailParticle.Clear();
        trailParticle.Play();
    }

    public override void Reset(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        //Base
        transform.position = position;
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;
        this.enabled = true;

        //Unique
        trailParticle.Clear();
        trailParticle.Play();
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
        }
	}
}
