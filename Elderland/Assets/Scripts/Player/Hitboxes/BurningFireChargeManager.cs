using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public sealed class BurningFireChargeManager : FireChargeManager
{
    private const float spawnDistance = 1f;

    private float distanceTraveled;
    private List<Vector3> travelPoints;

    private bool moving;

    protected override void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitbox = GetComponentInChildren<PlayerMultiDamageHitbox>();
        particles = GetComponentsInChildren<ParticleSystem>();

        travelPoints = new List<Vector3>();
    }

    public override void Initialize(PlayerAbility ability, Vector2 velocity, float lifeDuration)
    {
        this.ability = ability;
        this.velocity = new Vector3(velocity.x, 0, velocity.y);
        this.lifeDuration = lifeDuration;
        lifeTimer = 0;

        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }

        distanceTraveled = 0;
        travelPoints.Clear();
        moving = true;
    }

    public override void PostInitialization()
    {
        // Wall check
        if (Physics.OverlapSphere(transform.position,
                                  characterController.radius * 0.9f,
                                  LayerConstants.GroundCollision).Length != 0)
        {
            Deactivate();
        }

        travelPoints.Add(transform.position);
    }

    protected override void Update()
    {
        //float speed =
        //    80 * Mathf.Pow(speedExponentialTerm * Mathf.Clamp01(lifeTimer / lifeDuration), maxSpeed);

        //Vector3 currentVelocity = velocity.normalized * speed;
        //Debug.Log(speed);
        if (moving)
        {
            Vector3 startPosition = transform.position;
            characterController.Move(velocity * Time.deltaTime);
            travelPoints.Add(transform.position);
            Vector3 endPosition = transform.position;
            SpawnBurnersWithDistance(startPosition, endPosition);
            GroundClamp();
        }
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration)
        {
            Deactivate();
        }
    }

    protected override void Deactivate()
    {
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
        }
        velocity = Vector3.zero;
        hitbox.gameObject.SetActive(false);
        hitbox.Deactivate();
        moving = false;
    }

    private void SpawnBurnersWithDistance(Vector3 startPosition, Vector3 endPosition)
    {
        float horizontalDistance =
            Matho.StandardProjection2D(endPosition - startPosition).magnitude;

        distanceTraveled += horizontalDistance;
        
        int spawnCount = (int) (distanceTraveled / spawnDistance); 

        //Debug.Log(spawnCount);
        for (int i = 0; i < spawnCount; i++) // Working up to here
        {
            float percentageAlongPath = (spawnDistance) / distanceTraveled;
            LookUpPercentageAlongPath(percentageAlongPath);
        }
    }

    private void LookUpPercentageAlongPath(float percentageAlongPath)
    {   
        float currentDistanceAlongPath = 0;
        List<float> travelDistances = new List<float>();
        travelDistances.Add(0);

        for (int i = 1; i < travelPoints.Count; i++)
        {
            float deltaDistance = (travelPoints[i] - travelPoints[i - 1]).magnitude;
            currentDistanceAlongPath += deltaDistance;
            travelDistances.Add(currentDistanceAlongPath);
        } // working

        List<float> travelPercentages = new List<float>();
        travelPercentages.Add(0);
        for (int i = 1; i < travelPoints.Count; i++)
        {
            travelPercentages.Add(travelDistances[i] / currentDistanceAlongPath);
        } // working

        int pointsToRemove = 0;
        Vector3 pointToAdd = Vector3.zero;
        for (int i = 0; i < travelPoints.Count - 1; i++)
        {
            if (travelPercentages[i] <= percentageAlongPath &&
                percentageAlongPath <= travelPercentages[i + 1])
            {
                float spawnPercentage =
                    (percentageAlongPath - travelPercentages[i]) /
                    (travelPercentages[i + 1] - travelPercentages[i]);
                Vector3 spawnPosition =
                    travelPoints[i] * (1 - spawnPercentage) +
                    travelPoints[i + 1] * spawnPercentage;
                Debug.DrawLine(spawnPosition, spawnPosition + Vector3.up, Color.magenta, 10f);
                pointToAdd = spawnPosition;
                pointsToRemove++;
                break;
            }
            else
            {
                pointsToRemove++;
            }
        } // working without removing

        distanceTraveled -= percentageAlongPath * distanceTraveled;

        for (int i = 0; i < pointsToRemove; i++)
        {
            travelPoints.RemoveAt(0);
        }

        travelPoints.Insert(0, pointToAdd);
    }

    private void SpawnBurner()
    {

    }

    private void DeactivateBurners()
    {

    }

    private void OnDrawGizmos()
    {
        if (travelPoints != null && travelPoints.Count > 0)
        {
            Gizmos.color = Color.black;

            for (int i = 0; i < travelPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(travelPoints[i], travelPoints[i + 1]);
            }

            foreach (Vector3 v in travelPoints)
            {
                Gizmos.DrawCube(v, Vector3.one * 0.025f);
            }
        }
    }
}
