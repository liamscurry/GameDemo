using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FireChargeManager : MonoBehaviour
{
    protected CharacterController characterController;
    protected PlayerMultiDamageHitbox hitbox;

    protected PlayerAbility ability;
    protected Vector3 velocity;
    protected float lifeDuration;
    protected float lifeTimer;

    protected float maxSpeed = 5f;
    protected float speedExponentialTerm = 1f;

    protected ParticleSystem[] particles;
    protected bool alive;

    protected virtual void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitbox = GetComponentInChildren<PlayerMultiDamageHitbox>();
        particles = GetComponentsInChildren<ParticleSystem>();
        GameInfo.Manager.OnRespawn += OnRespawn;
    }

    public virtual void Initialize(PlayerAbility ability, Vector2 velocity, float lifeDuration)
    {
        this.ability = ability;
        this.velocity = new Vector3(velocity.x, 0, velocity.y);
        this.lifeDuration = lifeDuration;
        lifeTimer = 0;

        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }

        alive = true;
    }

    protected virtual void Update()
    {
        //float speed =
        //    80 * Mathf.Pow(speedExponentialTerm * Mathf.Clamp01(lifeTimer / lifeDuration), maxSpeed);

        //Vector3 currentVelocity = velocity.normalized * speed;
        //Debug.Log(speed);
        if (alive)
        {
            characterController.Move(velocity * Time.deltaTime);
            GroundClamp();
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeDuration)
            {
                Deactivate();
            }
        }
    }

    public virtual void PostInitialization()
    {
        // Wall check
        if (Physics.OverlapSphere(transform.position,
                                  characterController.radius * 0.9f,
                                  LayerConstants.GroundCollision).Length != 0)
        {
            Deactivate();
        }
    }

    public virtual void DeleteResource()
    {
        GameInfo.Manager.OnRespawn -= OnRespawn;
        ForceDeactivate();
        Destroy(hitbox.gameObject);
    }

    private void OnRespawn(object sender, EventArgs e)
    {
        ForceDeactivate();
    }

    private void ForceDeactivate()
    {
        Deactivate();
        foreach (ParticleSystem particle in particles)
        {
            particle.Clear();
        }
    }

    protected void GroundClamp()
    {
        RaycastHit raycast;

        bool hit = UnityEngine.Physics.SphereCast(
            transform.position,
            characterController.radius,
            Vector3.down,
            out raycast,
            (characterController.height / 2) + characterController.radius,
            LayerConstants.GroundCollision);

        if (hit)
        {
            float verticalDisplacement = (raycast.distance - (characterController.height / 2 - characterController.radius));
            transform.position += verticalDisplacement * Vector3.down;
        }
        else
        {
            Deactivate();
        }
    }

    protected void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) > 45)
        {
            // Hit wall, need to add overlap check here to make sure enemies are hit that are touching walls.
            // of course not hitting enemies that have already been hit.
            Deactivate();
        }
    }

    protected virtual void Deactivate()
    {
        alive = false;
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
        }
        velocity = Vector3.zero;
        hitbox.Deactivate();
        hitbox.gameObject.SetActive(false);
    }
}
