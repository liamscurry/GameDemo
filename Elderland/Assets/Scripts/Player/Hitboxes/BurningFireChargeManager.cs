using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public sealed class BurningFireChargeManager : FireChargeManager
{
    [SerializeField]
    private ParticleSystem floorParticleSystem;
    
    private ParticleSystem.Particle[] floorParticles;

    private const float spawnDistance = 1f;

    private float distanceTraveled;
    private List<Vector3> travelPoints;

    private bool activated;
    private bool activatedBurners;

    private const float fireDuration = 5;

    private List<PlayerMultiDamageHitbox> currentDebuffHitboxes;
    private List<PlayerMultiDamageHitbox> asleepDebuffHitboxes;

    protected override void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitbox = GetComponentInChildren<PlayerMultiDamageHitbox>();
        particles = GetComponentsInChildren<ParticleSystem>();

        travelPoints = new List<Vector3>();
        currentDebuffHitboxes = new List<PlayerMultiDamageHitbox>();
        asleepDebuffHitboxes = new List<PlayerMultiDamageHitbox>();
        GameInfo.Manager.OnRespawn += OnRespawn;

        floorParticles = new Particle[500];
    }

    public override void Initialize(PlayerAbility ability, Vector2 velocity, float lifeDuration)
    {
        this.ability = ability;
        this.velocity = new Vector3(velocity.x, 0, velocity.y);
        this.lifeDuration = lifeDuration;
        lifeTimer = 0;

        distanceTraveled = 0;
        travelPoints.Clear();
        activated = true;
        DeactivateBurners();
        activatedBurners = true;

        floorParticleSystem.Stop();

        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }
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

        LocateBurner(transform.position);
        travelPoints.Add(transform.position);
    }

    protected override void Update()
    {
        //float speed =
        //    80 * Mathf.Pow(speedExponentialTerm * Mathf.Clamp01(lifeTimer / lifeDuration), maxSpeed);

        //Vector3 currentVelocity = velocity.normalized * speed;
        //Debug.Log(speed);
        lifeTimer += Time.deltaTime;

        if (activated)
        {
            Vector3 startPosition = transform.position;
            characterController.Move(velocity * Time.deltaTime); // calling deactivate on hit
            travelPoints.Add(transform.position);
            Vector3 endPosition = transform.position;

            if (activated)
            {
                SpawnBurnersWithDistance(startPosition, endPosition);
                GroundClamp();

                if (lifeTimer >= lifeDuration)
                {
                    Deactivate();
                }
            }
        }
        else
        {         
            if (lifeTimer >= lifeDuration + fireDuration)
            {   
                DeactivateBurners();
            }
        }
    }

    protected override void Deactivate()
    {
        foreach (ParticleSystem particle in particles)
        {
            //if (particle != floorParticleSystem)
                particle.Stop();
        }
        velocity = Vector3.zero;
        hitbox.gameObject.SetActive(false);
        hitbox.Deactivate();
        activated = false;
    }

    private void DeactivateBurners()
    {
        if (activatedBurners)
        {
            FastForwardGroundParticles();

            foreach (PlayerMultiDamageHitbox hitbox in currentDebuffHitboxes)
            {
                asleepDebuffHitboxes.Add(hitbox);
                hitbox.Deactivate();
                hitbox.gameObject.SetActive(false);
            }
            currentDebuffHitboxes.Clear(); 
            activatedBurners = false;
        }
    }

    private void FastForwardGroundParticles()
    {
        int numberOfParticles =
            floorParticleSystem.GetParticles(floorParticles);
        
        for (int i = 0; i < numberOfParticles; i++)
        {
            if (floorParticles[i].remainingLifetime > 0.5f)
                floorParticles[i].remainingLifetime = 0.5f;
        }

        floorParticleSystem.SetParticles(floorParticles, numberOfParticles);
    }

    private void OnRespawn(object sender, EventArgs e)
    {
        Deactivate();
        DeactivateBurners();
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

                LocateBurner(spawnPosition);

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

    private void LocateBurner(Vector3 raycastStart)
    {
        Quaternion spawnRotation =
            Quaternion.LookRotation(Matho.StandardProjection3D(velocity).normalized, Vector3.up);

        RaycastHit hit;
        if (Physics.Raycast(raycastStart,
                            Vector3.down,
                            out hit,
                            characterController.height,
                            LayerConstants.GroundCollision))
        {
            GenerateBurner(hit.point, spawnRotation);
        }
    }

    private void GenerateBurner(Vector3 position, Quaternion rotation)
    {
        if (asleepDebuffHitboxes.Count == 0)
        {
            GameObject debuffHitbox =
                    Instantiate(Resources.Load<GameObject>(
                                    ResourceConstants.Player.Hitboxes.BurningFireChargeDebuffHitbox),
                                    position,
                                    rotation);
                
            debuffHitbox.transform.parent = PlayerInfo.MeleeObjects.transform;
            var hitbox = debuffHitbox.GetComponent<PlayerMultiDamageHitbox>();
            currentDebuffHitboxes.Add(hitbox);
            hitbox.Activate(ability, false, true);
        }
        else
        {
            var debuffHitbox = asleepDebuffHitboxes[asleepDebuffHitboxes.Count - 1];
            asleepDebuffHitboxes.RemoveAt(asleepDebuffHitboxes.Count - 1);
            debuffHitbox.transform.position = position;
            debuffHitbox.transform.rotation = rotation;
            currentDebuffHitboxes.Add(debuffHitbox);
            debuffHitbox.Activate(ability, false, true);
            debuffHitbox.gameObject.SetActive(true);
        }
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
