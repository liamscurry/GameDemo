using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyManager : MonoBehaviour
{
    public enum EnemyType { Melee, Ranged }
    public enum EnemyState { Watching, Attacking, None }

    [SerializeField]
    private GameObject healthbarPivot;
    [SerializeField]
    private MeshRenderer healthbarDisplay;
    [SerializeField]
    private Collider hitbox;

    private float baseAgentSpeed;

    public EnemyLevel Level { get; set; }

    public Animator Animator { get; protected set; }
    public CapsuleCollider Capsule { get; protected set; }
    public Rigidbody Body { get; protected set; }
    public NavMeshAgent Agent { get; protected set; }
    public PhysicsSystem PhysicsSystem { get; protected set; }
    public AdvancedMovementSystem MovementSystem { get; protected set; }
    public EnemyAbilityManager AbilityManager { get; protected set; }
    public EnemyBuffManager BuffManager { get; protected set; }
    public EnemyStatsManager StatsManager { get; protected set; }
    public EnemyType Type { get; protected set; }
    public EnemyState State;

    public EnemyAbility NextAttack { get; set; }

    public bool towardsPlayer { get; private set; }
    public List<Vector2> waypoints { get; private set; }
    public Vector2 currentWaypoint { get; private set; }
    public bool movingTowardsWaypoint { get; private set; }
    public bool startedWaypoints { get; set; }
    public bool completedWaypoints { get; private set; }
    public int waypointsLength { get; set; }
    
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

    public Collider Hitbox { get { return hitbox; } }

    private bool isAgentOn;
    public bool Alive { get; private set;}

    private void Start()
    {
        Animator = GetComponent<Animator>();
        Capsule = GetComponent<CapsuleCollider>();
        Body = GetComponent<Rigidbody>();
        Agent = GetComponent<NavMeshAgent>();
        PhysicsSystem = new PhysicsSystem(gameObject, Capsule, Body, 1);
        MovementSystem = new AdvancedMovementSystem(gameObject, Capsule, PhysicsSystem);
        AbilityManager = new EnemyAbilityManager(Animator, PhysicsSystem, MovementSystem, gameObject);
        BuffManager = new EnemyBuffManager(this);
        StatsManager = new EnemyStatsManager(this);
        State = EnemyState.None;
        Initialize();

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        PhysicsSystem.Animating = false;
        waypoints = new List<Vector2>();

        ArrangementNode = -1;
        Alive = true;

        baseAgentSpeed = Agent.speed;
    }

    private void Update()
    {
        PhysicsSystem.UpdateSystem();
        BuffManager.UpdateBuffs();
        MovementSystem.UpdateSystem();
        AbilityManager.UpdateAbilities();
        ColorHealth();
        Agent.speed = baseAgentSpeed * StatsManager.MovespeedMultiplier.Value;
    }
    
    private void LateUpdate()
    {
        MovementSystem.LateUpdateSystem();
        PhysicsSystem.LateUpdateSystem();
    }

    private void FixedUpdate()
    {
        if (!PhysicsSystem.Animating)
        {
            Body.velocity = PhysicsSystem.CalculatedVelocity;
        } 
        else
        {
            Body.velocity = PhysicsSystem.AnimationVelocity;
            //Debug.Log(PhysicsSystem.AnimationVelocity);
        }       

        //Friction
        if (PhysicsSystem.TouchingFloor)
        {
            PhysicsSystem.DynamicDrag(12f);
        }

        MovementSystem.FixedUpdateSystem();
        PhysicsSystem.FixedUpdateSystem();
        AbilityManager.FixedUpdateAbilities();
    }

    //Collision
    private void OnCollisionEnter(Collision other)
    {
        VelocityCollision(other);
        OverlapCollision(other);
        GroundCollision(other);
    }

    private void OnCollisionStay(Collision other)
    {
        VelocityCollision(other);
        OverlapCollision(other);
    }

    //Handle dynamic velocity alteration when player gets pushed or moved into ground collision.
    private void VelocityCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleVelocityCollisions(PhysicsSystem, Capsule, transform.position, other);
    }

    private void GroundCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleGroundCollisions(PhysicsSystem, Capsule, transform.position, other);
    }

    private void OverlapCollision(Collision other)
    {
        if (1 << other.collider.gameObject.layer == LayerConstants.GroundCollision.value)
            PhysicsSystem.HandleOverlapCollisions(PhysicsSystem, Capsule, transform.position, other);
    }

    public void ChangeHealth(float value)
    {
        float preHealth = Health;

        Health = Mathf.Clamp(Health + (value * StatsManager.DamageTakenMultiplier.Value), 0, MaxHealth);

        Vector3 currentScale = healthbarPivot.transform.localScale;
        healthbarPivot.transform.localScale = new Vector3(Health / MaxHealth, currentScale.y, currentScale.z);
        if (preHealth != 0 && Health == 0)
        {
            healthbarPivot.SetActive(false);
            Die();
        }
        else if (preHealth == 0 && Health != 0)
        {
            healthbarPivot.SetActive(true);
        }
    }

    private void ColorHealth()
    {
        Color currentColor = healthbarDisplay.material.color;
        healthbarDisplay.material.color =
            new Color(currentColor.r,
                      currentColor.g,
                      1f - StatsManager.HealthDebuffMultiplier.Value);
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

        PhysicsSystem.Animating = true;

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        Level.RemoveFromWave();
    }

    public virtual void Freeze()
    {
        TurnOffAgent();

        PhysicsSystem.Animating = true;

        if (AbilityManager.CurrentAbility != null)
        {
            AbilityManager.CurrentAbility.ShortCircuit();
        }

        Animator.SetBool("stationary", false);
    }

    protected IEnumerator DieTimer()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    public void TurnOnAgent()
    {
        if (!isAgentOn)
        {
            isAgentOn = true;
            Agent.ResetPath();
            Agent.Warp(transform.position);
            Agent.updatePosition = true;
            Agent.updateRotation = true;
            PhysicsSystem.Animating = true;
        }
    }

    public void TurnOffAgent()
    {
        if (isAgentOn)
        {
            isAgentOn = false;
            PhysicsSystem.TotalZero(true, true, true);
            Agent.updatePosition = false;
            Agent.updateRotation = false;
            PhysicsSystem.Animating = false;
            PhysicsSystem.ForceTouchingFloor();
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