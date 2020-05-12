using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyAttackFollow : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private bool rechoseAbility;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;

    private bool startedFollowing;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<RangedEnemyManager>();
            //manager.direction = EnemyInfo.AbilityRandomizer.Next() % 2;
            //if (manager.direction == 0)   
            //    manager.direction = -1;
        }

        checkTimer = checkDuration;
        exiting = false;
        rechoseAbility = false;
        lastDistanceToPlayer = DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        startedFollowing = false;

        manager.TurnOnAgent();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = DistanceToPlayer();

            EnemyInfo.RangedArranger.GetValidIndex(
                manager.transform.position,
                manager.direction,
                manager.ignoreIndex,
                ref manager.index);
            if (checkTimer > checkDuration)
            {
                MoveTowardsPlayer();
            }

            Vector2 destination = EnemyInfo.RangedArranger.GetPosition(manager.index);
            Vector3 destinationNav = GameInfo.CurrentLevel.NavCast(destination);
            PlayerInfo.Manager.test = destinationNav;
            
            ClampToGround();

            if (manager.ScreenForWaiting())
                WaitingExit();

            if (!exiting) 
                DefensiveTransition();
            if (!exiting) 
                StopTransition();
            
            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void ClampToGround()
    {
        RaycastHit raycast;

        Vector3 agentCenter = manager.Agent.nextPosition + (-manager.Agent.baseOffset + manager.Agent.height / 2) * Vector3.up;

        bool hit = UnityEngine.Physics.SphereCast(
            agentCenter,
            manager.Capsule.radius,
            Vector3.down,
            out raycast,
            (manager.Capsule.height / 2) + manager.Capsule.radius,
            LayerConstants.GroundCollision);

        if (hit)
        {
            float verticalOffset = 1f - (raycast.distance - (manager.Capsule.height / 2 - manager.Capsule.radius));
            manager.Agent.baseOffset = verticalOffset;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 destination = EnemyInfo.RangedArranger.GetPosition(manager.index);
        Vector3 destinationNav = GameInfo.CurrentLevel.NavCast(destination);
        NavMeshPath path = new NavMeshPath();
        if (manager.Agent.CalculatePath(destinationNav, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            manager.Agent.path = path;
            manager.path = new List<Vector3>(path.corners);
            manager.path.Insert(0, GameInfo.CurrentLevel.NavCast(Matho.StandardProjection2D(manager.transform.position)));
            manager.Agent.stoppingDistance = 0;
            startedFollowing = true;
        }
    }

    private void DefensiveTransition()
    {
        if (manager.IsInDefensiveRange())
        {
            DefensiveExit();
        }
    }

    private void StopTransition()
    {
        if (manager.Agent.remainingDistance < 0.5f + 0.05f && startedFollowing)
        {
            if (manager.HasClearPlacement())
            {
                StationaryExit();
            }
            else
            {
                SearchExit();
            }

            manager.ignoreIndex = -1;
        }
    }

    private void DefensiveExit()
    {
        manager.TurnOffAgent();

        manager.AbilityManager.CancelQueue();
        
        manager.NextAttack = manager.Slow;
        manager.Slow.Queue(EnemyAbilityType.First);
        manager.Slow.Queue(EnemyAbilityType.Middle);
        manager.Slow.Queue(EnemyAbilityType.Last);
        manager.AbilityManager.StartQueue();

        manager.Animator.SetTrigger("defensiveStart");
        manager.Animator.SetBool("defensive", true);
        manager.Animator.ResetTrigger("runAbility");

        exiting = true;
    }

    private void SearchExit()
    {
        manager.Animator.SetTrigger("toSearch");
        exiting = true;
    }

    private void StationaryExit()
    {
        manager.Animator.SetTrigger("toStationary");
        exiting = true;
    }

    private void WaitingExit()
    {
        manager.Animator.SetBool("waiting", true);
        manager.TurnOffAgent();
        exiting = true;
    }

    private float DistanceToPlayer()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(manager.transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);
        return horizontalDistanceToPlayer;
    }
}
