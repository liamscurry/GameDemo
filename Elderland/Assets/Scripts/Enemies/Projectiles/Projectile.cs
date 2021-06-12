using System;
using UnityEngine;

//Bases for all player projectiles
//Max speed is 30 velocity, anything above that needs to be hitscan

public abstract class Projectile : MonoBehaviour 
{
    [SerializeField]
    protected float damage;

    //Transform
    protected Vector3 velocity;

    //Timers
    protected float lifeTimer;
    protected float lifeDuration;

    //States
    protected bool alive;
    protected Vector3 lastPosition;

    //Components
    protected Rigidbody body;
    protected SphereCollider sphereCollider;

    //Information
    protected Vector3 normal;

    protected string targetTag;
    protected Func<GameObject, bool> hitTarget;

    //Life timer update
    protected virtual void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration && alive)
        { 
            normal = Vector2.zero;
            OnDeath(null); 
        }
        
        lastPosition = transform.position;
    }

    //Movement with velocity
    protected virtual void FixedUpdate()
    { 
        if (alive)
        {
            body.velocity = velocity;
        }
        else
        {
            body.velocity = Vector3.zero;
        }
    }

    //Called on projectile creation
    public virtual void Initialize(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        transform.position = position;
        if (velocity.magnitude != 0 && Matho.AngleBetween(Vector3.up, velocity) != 0)
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;

        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    //Called on projectile reuse
    public virtual void Reset(
        Vector3 position,
        Vector3 velocity,
        float lifeTime,
        string targetTag,
        Func<GameObject, bool> hitTarget,
        ProjectileArgs info = null)
    {
        transform.position = position;
        if (velocity.magnitude != 0 && Matho.AngleBetween(Vector3.up, velocity) != 0)
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        this.velocity = velocity;
        this.lifeTimer = 0;
        this.lifeDuration = lifeTime;
        this.targetTag = targetTag;
        this.hitTarget = hitTarget;
        alive = true;

        this.enabled = true;
    }

    //Adds projectile to pool
    public abstract void Recycle();

    //Disables components, invokes custom death and recycles the projectile.
    protected virtual void OnDeath(GameObject target) 
    {
        alive = false;
        this.enabled = false;
        body.velocity = Vector3.zero;
        if (hitTarget != null)
            hitTarget(target);
        Recycle();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (alive)
        {
            if (other.tag == TagConstants.GroundCollision)
            {
                // Check for hitbox trigger
                Collider[] overlappingColliders =  
                    Physics.OverlapSphere(transform.position, sphereCollider.radius * 1.1f, LayerConstants.Hitbox);

                foreach (Collider overlapCollider in overlappingColliders)
                {
                    if (overlapCollider.tag == targetTag)
                    {
                        AdjustDeathPosition(LayerConstants.Hitbox);    
                        OnDeath(overlapCollider.transform.parent.gameObject);  
                        return;
                    }
                }

                AdjustDeathPosition(LayerConstants.GroundCollision);               
                OnDeath(null);  
            }

            if (other.tag == targetTag)
            {
                AdjustDeathPosition(LayerConstants.Hitbox);    
                OnDeath(other.transform.parent.gameObject);  
            }
        }
    }

    protected virtual void AdjustDeathPosition(int layerMask)
    {
        RaycastHit deathHit;
        bool deathHitSuccessful = Physics.SphereCast(
            lastPosition,
            sphereCollider.radius,
            (transform.position - lastPosition).normalized,
            out deathHit,
            (transform.position - lastPosition).magnitude,
            layerMask);

        //Debug.Log((transform.position - lastPosition).magnitude);
        Debug.DrawLine(lastPosition, transform.position, Color.red, 5);
        Debug.DrawLine(lastPosition, lastPosition + (transform.position - lastPosition).normalized * 0.5f, Color.red, 5);

        if (deathHitSuccessful )
        {
            transform.position = lastPosition + deathHit.distance * (transform.position - lastPosition).normalized;
        } 
        else if (Physics.OverlapSphere(lastPosition, sphereCollider.radius, layerMask).Length > 0)
        {
            transform.position = lastPosition;
        }
    }
}