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
    }

    protected virtual void Update()
    {
        //float speed =
        //    80 * Mathf.Pow(speedExponentialTerm * Mathf.Clamp01(lifeTimer / lifeDuration), maxSpeed);

        //Vector3 currentVelocity = velocity.normalized * speed;
        //Debug.Log(speed);
        characterController.Move(velocity * Time.deltaTime);
        GroundClamp();
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration)
        {
            Deactivate();
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
            Deactivate();
        }
    }

    protected virtual void Deactivate()
    {
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
        }
        velocity = Vector3.zero;
        hitbox.gameObject.SetActive(false);
        hitbox.Deactivate();
    }
}
