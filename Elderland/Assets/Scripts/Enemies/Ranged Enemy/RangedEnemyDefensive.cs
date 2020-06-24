using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyDefensive : StateMachineBehaviour
{
    private RangedEnemyManager manager;
    private bool exiting;

    private float checkTimer;
    private const float checkDuration = 0.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (manager == null)
        {
            manager = animator.GetComponent<RangedEnemyManager>();
        }

        checkTimer = checkDuration;
        exiting = false;

        manager.TurnOnAgent();

        if (manager.DefensiveAttackSuccessful)
        {
            manager.Agent.speed = RangedEnemyManager.RunAwaySpeed;
        }
        else
        {
            manager.Agent.speed = RangedEnemyManager.LimpAwaySpeed;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!exiting)
        {
            checkTimer += Time.deltaTime;

            EnemyInfo.RangedArranger.GetValidIndex(
                manager.transform.position,
                manager.direction,
                manager.ignoreIndex,
                ref manager.index);

            if (manager.index != -1)
            {
                if (checkTimer > checkDuration)
                {
                    MoveAwayFromPlayer();
                }
            }

            ClampToGround();

            if (!exiting)
                RunAwayExit();

            if (checkTimer >= checkDuration)
                checkTimer = 0;
        }
    }

    private void MoveAwayFromPlayer()
    {
        Vector2 destination = EnemyInfo.RangedArranger.GetPosition(manager.index);
        Vector3 destinationNav = GameInfo.CurrentLevel.NavCast(destination);
        NavMeshPath path = new NavMeshPath();
        if (manager.Agent.CalculatePath(destinationNav, path))
        {
            manager.Agent.path = path;
            manager.path = new List<Vector3>(path.corners);
            manager.path.Insert(0, GameInfo.CurrentLevel.NavCast(Matho.StandardProjection2D(manager.transform.position)));
        }
        manager.Agent.stoppingDistance = 0;
    }

    private void RunAwayExit()
    {
        if (manager.IsOutOfDefensiveRange())
        {
            manager.Animator.SetTrigger("defensiveExit");
            manager.Animator.SetBool("defensive", false);
            manager.Agent.speed = RangedEnemyManager.WalkSpeed;
            exiting = true;
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
            float verticalOffset = 1.0f - (raycast.distance - (manager.Capsule.height / 2 - manager.Capsule.radius));
            manager.Agent.baseOffset = verticalOffset;
        }
    }
}