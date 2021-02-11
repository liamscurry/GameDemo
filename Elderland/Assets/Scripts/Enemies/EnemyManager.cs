﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyManager : MonoBehaviour, ICharacterManager
{
    public enum EnemyType { Melee, Ranged }
    public enum EnemyState { Watching, Attacking, None }

    [Header("Healthbar")]
    [SerializeField]
    protected GameObject healthbarPivot;
    [SerializeField]
    private MeshRenderer healthbarDisplay;
    [SerializeField]
    private GameObject healthbarBackground;
    [SerializeField]
    private Color healthBarColor;
    [SerializeField]
    private Color finisherColor;
    [SerializeField]
    private Color armorColor;
    [SerializeField]
    private float healthbarAppearRadius;
    [SerializeField]
    private float healthbarAppearBlend;
    [Header("Resolvebar")]
    [SerializeField]
    private GameObject resolvebarPivot;
    [SerializeField]
    private MeshRenderer resolvebarDisplay;
    [SerializeField]
    private HealthbarShadow resolvebarShadowPivot;
    [SerializeField]
    private Collider hitbox;
    [SerializeField]
    private int maxResolve;
    [SerializeField]
    private float resolveZeroDuration;
    [SerializeField]
    private GameObject leftWeakDirectionIndicator;
    [SerializeField]
    private GameObject rightWeakDirectionIndicator;
    [SerializeField]
    private GameObject glitchRenderersParent;
    [Header("Particles")]
    [SerializeField]
    private ParticleSystem[] spawnParticles;
    [SerializeField]
    private ParticleSystem[] deathParticles;
    [SerializeField]
    private ParticleSystem[] recycleParticles;
    [SerializeField]
    private Light[] lights;

    private float[] lightsIntensity;

    private float currentResolve;
    private float resolveTimer;
    private Vector3 healthbarPivotScale;
    private Vector3 resolvebarPivotScale;
    private GameObject finisherIndicator;
    private const float healthArmorMargin = 0.1f;
    private bool inFinisherState;
    private float currentFresnel;
    private const float fresnelSpeed = 2f;

    private float baseAgentSpeed;

    public StateMachineBehaviour BehaviourLock { get; set; }

    public EnemyLevel Level { get; set; }
    public EncounterSpawner.Spawner EncounterSpawn { get; set; }

    public Animator Animator { get; protected set; }
    public CapsuleCollider Capsule { get; protected set; }
    public Rigidbody Body { get; protected set; }
    public NavMeshAgent Agent { get; protected set; }
    public EnemyAbilityManager AbilityManager { get; protected set; }
    public BuffManager<EnemyManager> BuffManager { get; protected set; }
    public EnemyStatsManager StatsManager { get; protected set; }
    public EnemyType Type { get; protected set; }
    public EnemyState State;
    public int WeakDirection { get; private set; }

    public EnemyAbility NextAttack { get; set; }

    public bool towardsPlayer { get; private set; }
    public List<Vector2> waypoints { get; private set; }
    public Vector2 currentWaypoint { get; private set; }
    public bool movingTowardsWaypoint { get; private set; }
    public bool startedWaypoints { get; set; }
    public bool completedWaypoints { get; private set; }
    public int waypointsLength { get; set; }

    // Particles
    public ParticleSystem[] SpawnParticles { get { return spawnParticles; } }
    public ParticleSystem[] DeathParticles { get { return deathParticles; } }
    public ParticleSystem[] RecycleParticles { get { return recycleParticles; } }
    
    private SkinnedMeshRenderer[] glitchRenderers;

    public float NextAttackMax
    {
        get { return NextAttack.AttackDistance + NextAttack.AttackDistanceMargin; }
    }

    public float NextAttackCenter
    {
        get { return NextAttack.AttackDistance; }
    }

    public float ArrangementAngle { get; set; }
    public float ArrangmentRadius { get; set; }
    public int ArrangementNode;
    // { get; set; }

    public float Health { get; protected set; }
    public float MaxHealth { get; protected set; }
    public float Armor { get; protected set; }
    public float MaxArmor { get; protected set; }
    public float FinisherHealth { get; protected set; }
    public Color HealthBarColor { get; set; }
    
    public Vector3 BottomSphereOffset { get; private set; }

    public Collider Hitbox { get { return hitbox; } }

    public bool IsAgentOn { get { return isAgentOn; } }

    private bool isAgentOn;
    public bool Alive { get; private set; }

    private Vector3 dynamicAgentVelocity;
    private bool moveViaMovementManagerDuringAnimating;

    private void Start()
    {
        HealthBarColor = healthBarColor;

        Animator = GetComponentInChildren<Animator>();
        Capsule = GetComponent<CapsuleCollider>();
        Body = GetComponent<Rigidbody>();
        Agent = GetComponent<NavMeshAgent>();
        AbilityManager = new EnemyAbilityManager(Animator, null, null, gameObject);
        BuffManager = new BuffManager<EnemyManager>(this);
        StatsManager = new EnemyStatsManager(this);
        State = EnemyState.None;
        Initialize();

        Agent.updateRotation = false;
        //PhysicsSystem.Animating = false;
        waypoints = new List<Vector2>();

        ArrangementNode = -1;

        baseAgentSpeed = Agent.speed;

        BottomSphereOffset = Capsule.BottomSphereOffset();

        GenerateHealthbar();

        ScrambleWeakDirection();

        lightsIntensity = new float[lights.Length];

        for (int i = 0; i < lightsIntensity.Length; i++)
        {
            lightsIntensity[i] = lights[i].intensity;
        }
        glitchRenderers = glitchRenderersParent.GetComponentsInChildren<SkinnedMeshRenderer>();

        StartCoroutine(SpawnTimer());
    }

    private void Update()
    {
        BuffManager.UpdateBuffs();
        AbilityManager.UpdateAbilities();
        UpdateMaterialSettings();
        TimeResolve();
        Agent.speed = baseAgentSpeed * StatsManager.MovespeedMultiplier.Value;

        //if (!PhysicsSystem.Animating || moveViaMovementManagerDuringAnimating)
        {
            //MovementSystem.Move(Matho.StandardProjection2D(dynamicAgentVelocity), dynamicAgentVelocity.magnitude);
            //Debug.Log(Matho.StandardProjection2D(dynamicAgentVelocity));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeHealth(0.5f);
        }
    }

    protected virtual void FixedUpdate()
    {
        // Temporarily disable dynamic velocity
        Agent.Move(dynamicAgentVelocity * Time.deltaTime);
    
        DynamicDrag(12f);

        AbilityManager.FixedUpdateAbilities();
    }

    protected virtual void GenerateHealthbar()
    {
        ZeroResolveBar();
        resolvebarShadowPivot.Zero();
        healthbarPivotScale = healthbarPivot.transform.parent.localScale;
        resolvebarPivotScale = resolvebarPivot.transform.parent.localScale;

        finisherIndicator =
            Instantiate(
                Resources.Load<GameObject>(ResourceConstants.Enemy.UI.FinisherIndicator),
                healthbarBackground.transform);

        float width =
            healthbarBackground.transform.localScale.x;
        float finisherPercentage = 
            FinisherHealth / MaxHealth;

        finisherIndicator.transform.localPosition = 
            new Vector3(
                width / 2 - finisherPercentage * width,
                finisherIndicator.transform.localPosition.y,
                finisherIndicator.transform.localPosition.z);

        inFinisherState = false;
        currentFresnel = 0;

        ColorHealth();
    }

    public void Push(Vector3 velocity)
    {
        dynamicAgentVelocity += velocity;
    }

    public void Zero()
    {
        dynamicAgentVelocity = Vector3.zero;
    }

    public Vector3 GetGroundNormal()
    {
        RaycastHit raycast;
        bool hit = UnityEngine.Physics.SphereCast(
            transform.position + BottomSphereOffset + Capsule.radius * Vector3.up,
            Capsule.radius,
            Vector3.down,
            out raycast,
            Capsule.radius + Capsule.height / 2f,
            LayerConstants.GroundCollision);

        if (hit)
        {
            return raycast.normal;
        }
        else
        {
            return Vector3.up;
        }
    }

    public void ClampToGround()
    {
        RaycastHit raycast;

        Vector3 agentCenter = Agent.nextPosition + (-Agent.baseOffset + Agent.height / 2) * Vector3.up;

        bool hit = UnityEngine.Physics.SphereCast(
            agentCenter,
            Capsule.radius,
            Vector3.down,
            out raycast,
            (Capsule.height / 2) + Capsule.radius,
            LayerConstants.GroundCollision);

        if (hit)
        {
            float verticalOffset = 1 - (raycast.distance - (Capsule.height / 2 - Capsule.radius));
            Agent.baseOffset = verticalOffset;
        }
    }

    public void DynamicDrag(float strength)
    {
        float magnitude = dynamicAgentVelocity.magnitude;
        magnitude -= strength * Time.deltaTime;
        if (magnitude < 0)
            magnitude = 0;
        
        dynamicAgentVelocity = magnitude * dynamicAgentVelocity.normalized;
    }

    public void IncreaseResolve(float amount)
    {
        currentResolve += amount;
        if (currentResolve > maxResolve)
            currentResolve = maxResolve;

        if (!resolvebarPivot.activeSelf)
            resolvebarPivot.gameObject.SetActive(true);

        float resolvePercentage = ((float) currentResolve) / maxResolve;

        resolvebarPivot.transform.localScale =
            new Vector3(resolvePercentage, resolvebarPivot.transform.localScale.y, resolvebarPivot.transform.localScale.z);

        resolveTimer = 0;
    }

    public void ScrambleWeakDirection()
    {
        WeakDirection = (UnityEngine.Random.value > 0.5) ? 1 : 0;
        if (WeakDirection == 0)
        {
            leftWeakDirectionIndicator.SetActive(true);
            rightWeakDirectionIndicator.SetActive(false);
        }
        else
        {
            leftWeakDirectionIndicator.SetActive(false);
            rightWeakDirectionIndicator.SetActive(true);
        }
    }

    public bool CheckResolve()
    {
        return currentResolve == maxResolve;
    }

    public void ConsumeResolve()
    {
        if (currentResolve != 0)
        {
            currentResolve = 0;
            ZeroResolveBar();
        }
    }

    private void TimeResolve()
    {
        resolveTimer += Time.deltaTime;
        if (resolveTimer >= resolveZeroDuration)
        {
            resolveTimer = 0;
            ConsumeResolve();
        }
    }

    public void ZeroResolveBar()
    {
        resolvebarPivot.transform.localScale =
            new Vector3(0, resolvebarPivot.transform.localScale.y, resolvebarPivot.transform.localScale.z);

        resolvebarPivot.SetActive(false);
    }

    public void ChangeHealth(float value, bool effectsArmor = true)
    {
        if (Armor > healthArmorMargin)
        {
            // Still armor left
            if (value < 0 && effectsArmor)
            {
                float armor = Armor;
                ChangeHealthbarValue(
                    ref armor,
                    MaxArmor,
                    value,
                    OnArmorZero,
                    OnArmorNonZero);
                    Armor = armor;
            }
        }
        else
        {
            float health = Health;
            ChangeHealthbarValue(
                ref health,
                MaxHealth,
                value,
                OnHealthZero,
                OnHealthNonZero);
            Health = health;
        }

        ColorHealth();
    }

    private void ChangeHealthbarValue(
        ref float current,
        float max,
        float delta,
        Action onZero,
        Action onNonzero)
    {
        float preHealth = current;

        current = Mathf.Clamp(current + (delta * StatsManager.DamageTakenMultiplier.Value), 0, max);

        Vector3 currentScale = healthbarPivot.transform.localScale;
        healthbarPivot.transform.localScale = new Vector3(current / max, currentScale.y, currentScale.z);
        if (preHealth != 0 && current == 0)
        {
            if (onZero != null)
                onZero.Invoke();
        }
        else if (preHealth == 0 && current != 0)
        {
            if (onNonzero != null)
                onNonzero.Invoke();
        }

        if (delta < 0)
        {
            StopCoroutine("GlitchMaterial");
            StartCoroutine("GlitchMaterial", 0.75f);
        }
    }

    private void OnHealthZero()
    {
        //healthbarPivot.SetActive(false);
        Die();
    }

    private void OnHealthNonZero()
    {
        //healthbarPivot.SetActive(true);
    }

    private void OnArmorZero()
    {
        Vector3 currentScale = healthbarPivot.transform.localScale;
        healthbarPivot.transform.localScale = new Vector3(1, currentScale.y, currentScale.z);
    }

    private void OnArmorNonZero()
    {
        
    }

    private void ColorHealth()
    {
        Color dimColor =
            new Color(
                EnemyInfo.ShadowColorDim,
                EnemyInfo.ShadowColorDim,
                EnemyInfo.ShadowColorDim,
                1);
        // Health Color
        if (Armor < healthArmorMargin)
        {
            if (Health < FinisherHealth ||
                Matho.IsInRange(Health, FinisherHealth, healthArmorMargin))
            {
                healthbarDisplay.material.SetColor("_Color", EnemyInfo.FinisherHealthColor);
                resolvebarDisplay.material.SetColor("_Color", EnemyInfo.FinisherHealthColor * dimColor);
                inFinisherState = true;
            }
            else
            {
                healthbarDisplay.material.SetColor("_Color", EnemyInfo.HealthColor);
                resolvebarDisplay.material.SetColor("_Color", EnemyInfo.HealthColor * dimColor);
                inFinisherState = false;
            }

            if (!finisherIndicator.activeSelf)
                finisherIndicator.SetActive(true);
        }
        else
        {
            // In armor state
            healthbarDisplay.material.SetColor("_Color", EnemyInfo.ArmorColor);
            resolvebarDisplay.material.SetColor("_Color", EnemyInfo.ArmorColor * dimColor);
            if (finisherIndicator.activeSelf)
                finisherIndicator.SetActive(false);
        }
    }

    private IEnumerator GlitchMaterial(float duration)
    {
        float timer = 0;
        
        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            glitch.material.SetFloat("_Glitch", 1);
        }

        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();

            timer += Time.deltaTime;
            foreach (SkinnedMeshRenderer glitch in glitchRenderers)
            {
                glitch.material.SetFloat("_Glitch", 1 - timer / duration);
            }
        }

        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            glitch.material.SetFloat("_Glitch", 0);
        }
    }

    public void MaxOutHealth()
    {
        Health = MaxHealth;
    }

    public void SetTierMaxHealth(float percentage)
    {
        MaxHealth *= percentage;
    }

    private void UpdateMaterialSettings()
    {
        ColorFresnel();

        if (Alive)
        {
            float distanceToPlayer = DistanceToPlayer();
            if (distanceToPlayer > healthbarAppearRadius)
            {
                float scaleModifier = distanceToPlayer - healthbarAppearRadius;
                scaleModifier = 1 - Mathf.Clamp01(scaleModifier / healthbarAppearBlend);
                
                if (scaleModifier == 0)
                {
                    if (healthbarPivot.transform.parent.gameObject.activeSelf)
                    {
                        healthbarPivot.transform.parent.gameObject.SetActive(false);
                        resolvebarPivot.transform.parent.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (!healthbarPivot.transform.parent.gameObject.activeSelf)
                    {
                        healthbarPivot.transform.parent.gameObject.SetActive(true);
                        resolvebarPivot.transform.parent.gameObject.SetActive(true);
                    }
                    healthbarPivot.transform.parent.localScale = healthbarPivotScale * scaleModifier;
                    resolvebarPivot.transform.parent.localScale = resolvebarPivotScale * scaleModifier;
                }
            } 
            else
            {
                if (!healthbarPivot.transform.parent.gameObject.activeSelf)
                {
                    healthbarPivot.transform.parent.gameObject.SetActive(true);
                    resolvebarPivot.transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }

    private void ColorFresnel()
    {
        int fresnelTarget = (inFinisherState) ? 1 : 0;
        currentFresnel = Mathf.MoveTowards(currentFresnel, fresnelTarget, fresnelSpeed * Time.deltaTime);
        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            glitch.material.SetFloat("_FresnelStrength", currentFresnel);
        }
    }

    public void Die()
    {
        Alive = false;
        SpawnPickups();
        DieLogic();

        StartCoroutine(DieTimer());
    }

    protected virtual void SpawnPickups() {}

    public void DieInstant()
    {
        Alive = false;
        DieLogic();

        Destroy(gameObject);
    }

    private void DieLogic()
    {
        Animator.SetTrigger("die");

        if (Type == EnemyType.Melee && ArrangementNode != -1)
        {
            EnemyInfo.MeleeArranger.ClearNode(ArrangementNode);
        }

        if (State == EnemyState.Attacking)
        {
            UnsubscribeFromAttack();
        }
        else if (State == EnemyState.Watching)
        {
            UnsubscribeFromWatch();
        }

        TurnOffAgent();

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        if (Level != null)
        {
            Level.RemoveFromWave();
        }
        else if (EncounterSpawn != null)
        {
            EncounterSpawn.state = EncounterSpawner.SpawnState.Dead;
        }
    }

    public void Recycle()
    {
        Alive = false;
        RecycleLogic();

        StartCoroutine(RecycleTimer());
    }

    public void RecycleLogic()
    {
        Animator.SetTrigger("recycle");

        /*
        if (Type == EnemyType.Melee && ArrangementNode != -1)
        {
            EnemyInfo.MeleeArranger.ClearNode(ArrangementNode);
        }*/

        /*
        if (State == EnemyState.Attacking)
        {
            UnsubscribeFromAttack();
        }
        else if (State == EnemyState.Watching)
        {
            UnsubscribeFromWatch();
        }*/

        TurnOffAgent();

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        EncounterSpawn.state = EncounterSpawner.SpawnState.Ready;
    }

    public virtual void Freeze()
    {
        TurnOffAgent();

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        Animator.SetBool("stationary", false);
    }

    public virtual void TryFlinch()
    {
        if (StatsManager.Interuptable)
        {
            if (AbilityManager.CurrentAbility != null)
            {
                AbilityManager.CurrentAbility.ShortCircuit();
            }

            Animator.SetTrigger("toFlinch");
        }
    }

    protected IEnumerator SpawnTimer()
    {
        healthbarPivot.transform.parent.gameObject.SetActive(false);
        resolvebarPivot.transform.parent.gameObject.SetActive(false);

        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            foreach (var material in glitch.materials)
            {
                material.SetFloat("_ClipThreshold", 0);
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = 0;
        }

        yield return new WaitForSeconds(0.3f);
        yield return MeshTransitionTimer(-1, 0.7f, 4);
        Alive = true;
    }

    protected IEnumerator DieTimer()
    {
        Vector3 healthBarScale =
            healthbarPivot.transform.parent.localScale;
        Vector3 resolveBarScale =
            resolvebarPivot.transform.parent.localScale;

        StartCoroutine(MeshTransitionTimer(1, 0.6f, 16));
        yield return ParticleTransitionTimer(0.6f, deathParticles);

        healthbarPivot.transform.parent.gameObject.SetActive(false);
        resolvebarPivot.transform.parent.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);
    }

    protected IEnumerator MeshTransitionTimer(int sign, float duration, float healthBarSpeed)
    {
        float scaledSign = sign * 0.5f + 0.5f;

        float timer = 0;

        float distanceToPlayer = DistanceToPlayer();
        float scaleModifier;
        if (distanceToPlayer > healthbarAppearRadius)
        {
            scaleModifier = distanceToPlayer - healthbarAppearRadius;
            scaleModifier = 1 - Mathf.Clamp01(scaleModifier / healthbarAppearBlend);
        }
        else
        {
            scaleModifier = 1;
        }
            
        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            foreach (var material in glitch.materials)
            {
                material.SetFloat("_ClipThreshold", scaledSign);
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = scaledSign * lightsIntensity[i];
        }

        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();

            timer += Time.deltaTime;
            float percentage = 1 - timer / duration;
            float alteredPercentage = Mathf.Clamp01(1 - healthBarSpeed * timer / duration);
            float clipThreshold = percentage;
            if (scaledSign == 0)
            {
                clipThreshold = 1 - percentage;
                alteredPercentage = 1 - alteredPercentage;
            }

            foreach (SkinnedMeshRenderer glitch in glitchRenderers)
            {
                foreach (var material in glitch.materials)
                {
                    material.SetFloat("_ClipThreshold", clipThreshold);
                }
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = clipThreshold * lightsIntensity[i];
            }
            
            if (alteredPercentage != 0)
            {
                if (!healthbarPivot.transform.parent.gameObject.activeSelf)
                {
                    healthbarPivot.transform.parent.gameObject.SetActive(true);
                    resolvebarPivot.transform.parent.gameObject.SetActive(true);
                }

                healthbarPivot.transform.parent.localScale = 
                    new Vector3(healthbarPivotScale.x, alteredPercentage * healthbarPivotScale.y, healthbarPivotScale.z) * scaleModifier;
                
                resolvebarPivot.transform.parent.localScale = 
                    new Vector3(resolvebarPivotScale.x, alteredPercentage * resolvebarPivotScale.y, resolvebarPivotScale.z) * scaleModifier;
            }
            else
            {
                healthbarPivot.transform.parent.gameObject.SetActive(false);
                resolvebarPivot.transform.parent.gameObject.SetActive(false);
            }
        }

        foreach (SkinnedMeshRenderer glitch in glitchRenderers)
        {
            foreach (var material in glitch.materials)
            {
                material.SetFloat("_ClipThreshold", 1 - scaledSign);
            }
        }   

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = (1 - scaledSign) * lightsIntensity[i];
        }
    }

    protected IEnumerator ParticleTransitionTimer(float duration, ParticleSystem[] particleSystemParent)
    {
        float timer = 0;
        
        var deathParticleRenderers
            = new List<ParticleSystemRenderer>();
        foreach (ParticleSystem particleSystem in particleSystemParent)
        {
            deathParticleRenderers.Add(
                particleSystem.GetComponent<ParticleSystemRenderer>());
        }

        foreach (var deathParticle in deathParticleRenderers)
        {
            deathParticle.material.SetFloat("_ClipThreshold", 0);
            deathParticle.material.SetVector("_WorldCenter", glitchRenderersParent.transform.position + Vector3.up * 0.2f);
        }

        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();

            foreach (var deathParticle in deathParticleRenderers)
            {
                deathParticle.material.SetFloat("_ClipThreshold", 0);
                deathParticle.material.SetVector("_WorldCenter", glitchRenderersParent.transform.position + Vector3.up * 0.2f);
            }

            timer += Time.deltaTime;
            float percentage = 1 - timer / duration;

            foreach (var deathParticle in deathParticleRenderers)
            {
                deathParticle.material.SetFloat("_ClipThreshold", 1 - percentage);
            }
        }
    }

    protected IEnumerator RecycleTimer()
    {
        Vector3 healthBarScale =
            healthbarPivot.transform.parent.localScale;
        Vector3 resolveBarScale =
            resolvebarPivot.transform.parent.localScale;

        yield return (MeshTransitionTimer(1, 0.6f, 16));
        //yield return ParticleTransitionTimer(0.6f, recycleParticles);

        healthbarPivot.transform.parent.gameObject.SetActive(false);
        resolvebarPivot.transform.parent.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);
    }

    public void TurnOnAgent()
    {
        if (!isAgentOn)
        {
            isAgentOn = true;
            Agent.ResetPath();
            Agent.updateRotation = true;
        }
    }

    public void TurnOffAgent()
    {
        if (isAgentOn)
        {
            isAgentOn = false;
            Agent.ResetPath();
            Agent.updateRotation = false;
        }
    }

    public bool IsInNextAttackMax()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        return horizontalDistanceToPlayer < NextAttack.AttackDistance + NextAttack.AttackDistanceMargin;
    } 

    public bool IsInNextAttackMin()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        return horizontalDistanceToPlayer < NextAttack.AttackDistance - NextAttack.AttackDistanceMargin;
    }

    public bool IsInNextAttackCenter()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        return horizontalDistanceToPlayer < NextAttack.AttackDistance;
    } 

    public void SubscribeToAttack()
    {
        switch (Type)
        {
            case EnemyType.Melee:
                EnemyInfo.MeleeAttackers.Add(this);
                break;
            case EnemyType.Ranged:
                EnemyInfo.RangedAttackers.Add(this);
                break;
        }
        
        State = EnemyState.Attacking;

        Animator.SetTrigger("toAttack");
    }

    public void SubscribeToWatch()
    {
        switch (Type)
        {
            case EnemyType.Melee:
                EnemyInfo.MeleeWatchers.Add(this);
                break;
            case EnemyType.Ranged:
                EnemyInfo.RangedWatchers.Add(this);
                break;
        }
        
        State = EnemyState.Watching;

        Animator.SetTrigger("toWatch");
    }
    
    public void UnsubscribeFromAttack()
    {
        switch (Type)
        {
            case EnemyType.Melee:
                EnemyInfo.MeleeAttackers.Remove(this);
                break;
            case EnemyType.Ranged:
                EnemyInfo.RangedAttackers.Remove(this);
                break;
        }

        State = EnemyState.None;
    }

    public void UnsubscribeFromWatch()
    {
        switch (Type)
        {
            case EnemyType.Melee:
                EnemyInfo.MeleeWatchers.Remove(this);
                break;
            case EnemyType.Ranged:
                EnemyInfo.RangedWatchers.Add(this);
                break;
        }

        State = EnemyState.None;
    }

    public void OverrideAttacker(EnemyManager other)
    {
        UnsubscribeFromWatch();
        SubscribeToAttack();
        other.Animator.ResetTrigger("toWatch");

        other.UnsubscribeFromAttack();
        other.SubscribeToWatch();
        other.Animator.ResetTrigger("toAttack");
        other.TurnOffAgent();
    }

    public bool IsSubscribeToAttackValid()
    {
        if (OverallHasRoom())
        {
            bool correspondingHasRoom = false;
            switch (Type)
            {
                case EnemyType.Melee:
                    correspondingHasRoom = MeleeHasRoom();
                    break;
                case EnemyType.Ranged:
                    correspondingHasRoom = RangedHasRoom();
                    break;
            }

            return correspondingHasRoom;
        }
        else
        {
            return false;
        }
    }

    private bool OverallHasRoom()
    {
        return (EnemyInfo.MeleeAttackers.Count + EnemyInfo.RangedAttackers.Count) < EnemyInfo.MaxOverallAttackers;
    }

    private bool MeleeHasRoom()
    {
        return EnemyInfo.MeleeAttackers.Count < EnemyInfo.MaxMeleeAttackers;
    }

    private bool RangedHasRoom()
    {
        return EnemyInfo.RangedAttackers.Count < EnemyInfo.MaxRangedAttackers;
    }

    public EnemyManager FindOverrideAttacker()
    {
        List<EnemyManager> closerAttackers = new List<EnemyManager>();

        FindFartherAttackers(closerAttackers);

        if (closerAttackers.Count > 0)
        {
            return FindFarthestAttacker(closerAttackers);
        }
        else
        {
            return null;
        }
    }

    private void FindFartherAttackers(List<EnemyManager> closerAttackers)
    {
        float thisDistance = Vector3.Distance(transform.position, PlayerInfo.Player.transform.position);

        foreach (EnemyManager enemy in EnemyInfo.MeleeAttackers)
        {
            float enemyDistance = Vector3.Distance(enemy.transform.position, PlayerInfo.Player.transform.position);
            if (enemyDistance > thisDistance + EnemyInfo.OverrideMargin)
                closerAttackers.Add(enemy);
        }

        foreach (EnemyManager enemy in EnemyInfo.RangedAttackers)
        {
            float enemyDistance = Vector3.Distance(enemy.transform.position, PlayerInfo.Player.transform.position);
            if (enemyDistance > thisDistance + EnemyInfo.OverrideMargin)
                closerAttackers.Add(enemy);
        }
    }

    private EnemyManager FindFarthestAttacker(List<EnemyManager> closerAttackers)
    {
        EnemyManager farthestCloserAttacker = closerAttackers[0];
        float farthestDistance = Vector3.Distance(farthestCloserAttacker.transform.position, PlayerInfo.Player.transform.position);

        for (int index = 1; index < closerAttackers.Count; index++)
        {
            float enemyDistance = Vector3.Distance(closerAttackers[index].transform.position, PlayerInfo.Player.transform.position);
            if (enemyDistance > farthestDistance)
            {
                farthestCloserAttacker = closerAttackers[index];
                farthestDistance = enemyDistance;
            }
        }

        return farthestCloserAttacker;
    }

    public float DistanceToPlayer()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);
        return horizontalDistanceToPlayer;
    }

    //Below arrangement code should really be in a melee enemy derivative
    public void PrimeAgentPath()
    {
        if (ArrangementNode != -1)
        {
            EnemyInfo.MeleeArranger.ClearNode(ArrangementNode);
        }
        EnemyInfo.MeleeArranger.ClaimNode(this);
        startedWaypoints = false;
        waypoints.Clear();
        movingTowardsWaypoint = false;
        completedWaypoints = false;
    }

    public void CancelAgentPath()
    {
        waypoints.Clear();
        Agent.ResetPath();
        startedWaypoints = false;
        movingTowardsWaypoint = false;
        completedWaypoints = false;
    }

    public void CalculateAgentPath()
    {
        float distanceToPlayer = DistanceToPlayer();
        Vector2 nodePosition = EnemyInfo.MeleeArranger.GetPosition(ArrangementNode);
        
        float nodePlayerDistance = (Matho.StandardProjection2D(PlayerInfo.Player.transform.position) - nodePosition).magnitude;
        towardsPlayer = distanceToPlayer >= nodePlayerDistance;

        float positionIndex = EnemyInfo.MeleeArranger.GetExactIndex(Matho.StandardProjection2D(transform.position));

        Vector2 nodeDirection = EnemyInfo.MeleeArranger.Center - nodePosition;
        Vector2 exactDirection = EnemyInfo.MeleeArranger.Center - EnemyInfo.MeleeArranger.GetPosition(positionIndex);

        if (!towardsPlayer && Matho.AngleBetween(nodeDirection, exactDirection) < 30f)
        {
            //waypoints.Clear();
            //Vector2 fightingPosition = Vector2.MoveTowards(nodePosition, EnemyInfo.MeleeArranger.Center, 1.5f);
            //waypoints.Add(fightingPosition);
            //waypointsLength = 1;
            //FollowAgentPoint();

            Vector2 fightingPosition = Vector2.MoveTowards(nodePosition, EnemyInfo.MeleeArranger.Center, 1.5f);
            NavMeshPath path = new NavMeshPath();
            bool validPath = Agent.CalculatePath(GameInfo.CurrentLevel.NavCast(fightingPosition), path);
            if (validPath && path.status == NavMeshPathStatus.PathComplete)
            {
                waypoints.Clear();
                waypoints.Add(fightingPosition);
                waypointsLength = 1;
                FollowAgentPoint();
            }
        }
        else
        {
            Vector3 nodePositionNav = GameInfo.CurrentLevel.NavCast(nodePosition);
            NavMeshPath path = new NavMeshPath();
            bool validPath = Agent.CalculatePath(nodePositionNav, path);
            if (validPath && path.status == NavMeshPathStatus.PathComplete)
            {
                Vector3 end = nodePositionNav;
                int endIndex = ArrangementNode;

                Vector3 farPoint;
                if (path.corners.Length > 1)
                    farPoint = path.corners[path.corners.Length - 2];
                else
                    farPoint = transform.position;
                Vector2 projectedFarPoint = Matho.StandardProjection2D(farPoint);
                int farIndex = EnemyInfo.MeleeArranger.GetValidIndex(projectedFarPoint);
                float exactIndex = EnemyInfo.MeleeArranger.GetExactIndex(projectedFarPoint);

                //Left path
                bool leftValid = true;
                int leftCount = 0;
                var leftWaypoints = new List<Vector2>();
                LeftPath(endIndex, farIndex, exactIndex, projectedFarPoint, ref leftValid, ref leftCount, ref leftWaypoints);

                //Right path
                bool rightValid = true;
                int rightCount = 0;
                var rightWaypoints = new List<Vector2>();
                RightPath(endIndex, farIndex, exactIndex, projectedFarPoint, ref rightValid, ref rightCount, ref rightWaypoints);

                waypoints.Clear();

                if (!rightValid && !leftValid)
                {
                    if (ArrangementNode != -1)
                    {
                        EnemyInfo.MeleeArranger.ClearNode(ArrangementNode);
                        ArrangementNode = -1;
                    }
                }
                else
                {
                    if (rightValid && !leftValid)
                    {
                        //right path
                        waypoints.AddRange(rightWaypoints);
                    }
                    else if (!rightValid && leftValid)
                    {
                        //left path
                        waypoints.AddRange(leftWaypoints);
                    }
                    else
                    {
                        if (rightCount < leftCount)
                        {
                            //rightPath
                            waypoints.AddRange(rightWaypoints);
                        }
                        else
                        {
                            //leftPath
                            waypoints.AddRange(leftWaypoints);
                        }
                    }

                    Vector2 fightingPosition = Vector2.MoveTowards(waypoints[0], EnemyInfo.MeleeArranger.Center, 1.5f);
                    waypoints.Insert(0, fightingPosition);
                    waypointsLength = waypoints.Count;
                    FollowAgentPoint();
                }
            }
        }
    }
    
    public void FollowAgentPath()
    {
        if (startedWaypoints)
        {
            if ((waypoints.Count != 0 && Agent.remainingDistance < 0.75f) || 
                (waypoints.Count == 0 && Agent.remainingDistance < 0.10f)) 
            {             
                if (waypoints.Count == 0)
                {
                    completedWaypoints = true;       
                }
                else
                {
                    FollowAgentPoint();
                }
            }
        }
    }

    private void FollowAgentPoint()
    {
        if (isAgentOn)
        {
            Vector3 destination = waypoints[waypoints.Count - 1];
            currentWaypoint = destination;
            Vector3 desinationNav = GameInfo.CurrentLevel.NavCast(destination);

            Agent.SetDestination(desinationNav);
            waypoints.RemoveAt(waypoints.Count - 1);
            Agent.stoppingDistance = 0;
            startedWaypoints = true;

            Vector2 centerDirection = EnemyInfo.MeleeArranger.Center - Matho.StandardProjection2D(transform.position);
            Vector2 nodeDirection = Matho.StandardProjection2D(destination - transform.position);
            Agent.autoBraking = (Matho.AngleBetween(nodeDirection, centerDirection) > 45);
        }
    }

    private void RightPath(int index, int endIndex, float exactIndex, Vector2 endPosition, ref bool valid, ref int count, ref List<Vector2> waypoints)
    {
        while (true)
        {
            if (!EnemyInfo.MeleeArranger.CheckValidity(index))
            {
                valid = false;
                break;
            }
            else
            {
                Vector2 nodePosition = EnemyInfo.MeleeArranger.GetPosition(index);
                Vector2 nodeDirection = EnemyInfo.MeleeArranger.Center - nodePosition;
                Vector2 endDirection = endPosition - nodePosition;
                Vector2 exactDirection = EnemyInfo.MeleeArranger.Center - EnemyInfo.MeleeArranger.GetPosition(exactIndex);
                if (index == endIndex ||
                    Matho.AngleBetween(nodeDirection, endDirection) >= 90 ||
                    Matho.AngleBetween(exactDirection, nodeDirection) < EnemyInfo.MeleeArranger.nodeSpacing + 10f)
                {
                    waypoints.Add(nodePosition);
                    break;
                }
                else
                {
                    waypoints.Add(nodePosition);
                    count++;
                    index = (index + 1) % EnemyInfo.MeleeArranger.n;
                }
            }
        }
    }

    private void LeftPath(int index, int endIndex, float exactIndex, Vector2 endPosition, ref bool valid, ref int count, ref List<Vector2> waypoints)
    {
        while (true)
        {
            if (!EnemyInfo.MeleeArranger.CheckValidity(index))
            {
                valid = false;
                break;
            }
            else
            {
                Vector2 nodePosition = EnemyInfo.MeleeArranger.GetPosition(index);
                Vector2 nodeDirection = EnemyInfo.MeleeArranger.Center - nodePosition;
                Vector2 endDirection = endPosition - nodePosition;
                Vector2 exactDirection = EnemyInfo.MeleeArranger.Center - EnemyInfo.MeleeArranger.GetPosition(exactIndex);
                if (index == endIndex ||
                    Matho.AngleBetween(nodeDirection, endDirection) >= 90 ||
                    Matho.AngleBetween(exactDirection, nodeDirection) < EnemyInfo.MeleeArranger.nodeSpacing + 10f)
                {
                    waypoints.Add(nodePosition);
                    break;
                }
                else
                {
                    waypoints.Add(nodePosition);
                    count++;
                    index = (index - 1);
                    if (index < 0)
                        index += EnemyInfo.MeleeArranger.n;
                }
            }
        }
    }

    protected virtual void Initialize() 
    {
        DeclareAbilities();
        DeclareType();
    }

    protected abstract void DeclareAbilities();
    protected abstract void DeclareType();
    public abstract void ChooseNextAbility();

	protected virtual void OnDrawGizmos()
	{
        if (waypoints == null)
            return;

        Gizmos.color = Color.magenta;
        if (waypoints.Count >= 2)
        {
            for (int index = 0; index < waypoints.Count - 1; index++)
            {
                Vector3 current = new Vector3(waypoints[index].x, transform.position.y, waypoints[index].y);
                Vector3 next = new Vector3(waypoints[index + 1].x, transform.position.y, waypoints[index + 1].y);
                Gizmos.DrawCube(current, Vector3.one * 0.3f);
                Gizmos.DrawLine(current, next);
            }
            Vector3 last = new Vector3(waypoints[waypoints.Count - 1].x, transform.position.y, waypoints[waypoints.Count - 1].y);
            Gizmos.DrawCube(last, Vector3.one * 0.3f);
            
            Vector3 first = new Vector3(waypoints[0].x, transform.position.y, waypoints[0].y);
            Vector3 c = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.y);
            Gizmos.DrawCube(last, Vector3.one * 0.3f);
            Gizmos.DrawLine(last, c);
            Gizmos.DrawCube(c, Vector3.one * 0.3f);
            Gizmos.DrawLine(c, transform.position);
        }
        else if (waypoints.Count == 1)
        {
            Vector3 last = new Vector3(waypoints[waypoints.Count - 1].x, transform.position.y, waypoints[waypoints.Count - 1].y);
            Vector3 c = new Vector3(currentWaypoint.x, transform.position.y, currentWaypoint.y);
            Gizmos.DrawCube(last, Vector3.one * 0.3f);
            Gizmos.DrawLine(last, c);
            Gizmos.DrawCube(c, Vector3.one * 0.3f);
            Gizmos.DrawLine(c, transform.position);
        }
	}
}