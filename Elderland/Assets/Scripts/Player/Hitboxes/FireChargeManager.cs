using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FireChargeManager : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerMultiDamageHitbox hitbox;

    private Ability ability;
    private Vector3 velocity;
    private float lifeDuration;
    private float lifeTimer;

    private float maxSpeed = 5f;
    private float speedExponentialTerm = 1f;

    private ParticleSystem[] particles;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitbox = GetComponentInChildren<PlayerMultiDamageHitbox>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void Initialize(PlayerAbility ability, Vector2 velocity, float lifeDuration)
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

    private void Update()
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

    private void GroundClamp()
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Matho.AngleBetween(hit.normal, Vector3.up) > 45)
        {
            Deactivate();
        }
    }

    private void Deactivate()
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
