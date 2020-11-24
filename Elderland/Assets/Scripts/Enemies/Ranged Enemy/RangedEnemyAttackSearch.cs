using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class RangedEnemyAttackSearch : StateMachineBehaviour
{
    private RangedEnemyManager manager;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    private bool exiting;
    private bool rechoseAbility;
    private float lastDistanceToPlayer;
    private float distanceToPlayer;

    private Vector2 losPosition;
    private int timesReversed;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponentInParent<RangedEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;
        rechoseAbility = false;
        lastDistanceToPlayer = DistanceToPlayer();
        distanceToPlayer = lastDistanceToPlayer;

        CalculateRotationSettings();
        IncrementNextSearchIndex();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
        if (!exiting)
        {
            checkTimer += Time.deltaTime;
            distanceToPlayer = DistanceToPlayer();

            MoveTowardsSearchPosition();

            manager.ClampToGround();

            if (!exiting) 
                DefensiveTransition();
            if (!exiting) 
                StopTransition();
            
            lastDistanceToPlayer = distanceToPlayer;
            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
	}

    private void CalculateRotationSettings()
    {
        timesReversed = 0;
        losPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        Vector2 projectedPosition = Matho.StandardProjection2D(manager.transform.position);

        Vector2 projectedForward;
        if (manager.path.Count == 0)
        {
            projectedForward = Vector2.right;
        }
        else
        {
            Vector2 secondToLastPoint = Matho.StandardProjection2D(manager.path[manager.path.Count - 2]);
            Vector2 lastPoint = Matho.StandardProjection2D(manager.path[manager.path.Count - 1]);
            PlayerInfo.Manager.test = GameInfo.CurrentLevel.NavCast(lastPoint);
            PlayerInfo.Manager.test2 = GameInfo.CurrentLevel.NavCast(secondToLastPoint);
            projectedForward = (lastPoint - secondToLastPoint).normalized;
        }

        Vector2 centerDirection = (losPosition - projectedPosition).normalized;
        Vector2 tangentDirection = Matho.Rotate(centerDirection, -90f);
        if (Matho.ProjectScalar(projectedForward, tangentDirection) >= 0)
        {
            manager.direction = -1;
        }
        else
        {
            manager.direction = 1;
        }
    }

    private void MoveTowardsSearchPosition()
    {
        if (manager.Agent.remainingDistance < 0.5f + 0.05f)
        {
            IncrementNextSearchIndex();
        }

        if (checkTimer > checkDuration)
        {
            CalculateIndexPath();
        }

        if (timesReversed == 1)
        {
            if (manager.ScreenForWaiting())
                WaitingExit();
        }
    }

    private void CalculateIndexPath()
    {
        if (manager.IsAgentOn)
        {
            Vector2 destination = EnemyInfo.RangedArranger.GetPosition(manager.index, losPosition);
            Vector3 destinationNav = GameInfo.CurrentLevel.NavCast(destination);
            NavMeshPath path = new NavMeshPath();
            if (manager.Agent.CalculatePath(destinationNav, path))
            {
                manager.Agent.path = path;
                manager.Agent.stoppingDistance = 0;
            }
        }
    }

    private void IncrementNextSearchIndex()
    {
        NavMeshHit hit;
        Vector3 indexNav = GameInfo.CurrentLevel.NavCast(EnemyInfo.RangedArranger.GetPosition(manager.index, losPosition));

        int nextIndex = GetNextSearchIndex(manager.direction);
        Vector3 nextIndexNav = GameInfo.CurrentLevel.NavCast(EnemyInfo.RangedArranger.GetPosition(nextIndex, losPosition));
        if (!NavMesh.Raycast(indexNav, nextIndexNav, out hit, NavMesh.AllAreas))
        {
            manager.index = nextIndex;
            CalculateIndexPath();
            return;
        }

        int reverseIndex = GetNextSearchIndex(-manager.direction);
        Vector3 reverseIndexNav = GameInfo.CurrentLevel.NavCast(EnemyInfo.RangedArranger.GetPosition(reverseIndex, losPosition));
        if (!NavMesh.Raycast(indexNav, reverseIndexNav, out hit, NavMesh.AllAreas))
        {
            manager.index = reverseIndex;
            manager.direction *= -1;
            CalculateIndexPath();
            timesReversed++;
            if (timesReversed == 2)
                WaitingExit();
               //IgnoreCurrentIndex();
            return;
        }

        //IgnoreCurrentIndex();
        WaitingExit();
    }

    private void IgnoreCurrentIndex()
    {
        /*
        EnemyInfo.RangedArranger.GetValidIndex(
                manager.transform.position,
                manager.direction,
                manager.ignoreIndex,
                ref manager.index);
        */ 
        //manager.ignoreIndex = manager.index;
        FollowExit();
    }

    private int GetNextSearchIndex(int direction)
    {
        int nextIndex = 0;
        if (direction == 1)
        {
            nextIndex = (manager.index + 1) % EnemyInfo.RangedArranger.n;
        }
        else
        {
            nextIndex = manager.index - 1;
            if (nextIndex < 0)
                nextIndex += EnemyInfo.RangedArranger.n;
        }
        return nextIndex;
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
        if (distanceToPlayer < 7f + 3f)
        {
            if (manager.HasClearPlacement())
            {
                StationaryExit();
            }
        }
        else
        {
            FollowExit();
        }
    }

    private void DefensiveExit()
    {
        manager.TurnOffAgent();

        manager.AbilityManager.CancelQueue();
        
        manager.NextAttack = manager.Slow;
        manager.Slow.Queue(EnemyAbilityType.First);
        //manager.Slow.Queue(EnemyAbilityType.Middle);
        manager.Slow.Queue(EnemyAbilityType.Last);
        manager.AbilityManager.StartQueue();

        manager.Animator.SetTrigger("defensiveStart");
        manager.Animator.SetBool("defensive", true);
        manager.Animator.ResetTrigger("runAbility");

        exiting = true;
    }

    private void WaitingExit()
    {
        manager.Animator.SetBool("waiting", true);
        manager.TurnOffAgent();
        exiting = true;
    }

    private void FollowExit()
    {
        manager.Animator.SetTrigger("toFollow");
        exiting = true;
    }

    private void StationaryExit()
    {
        manager.Animator.SetTrigger("toStationary");
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
