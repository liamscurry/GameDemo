using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ability structure that supports any amount of states and substates. Has an optional cooldown period.
public abstract class EnemyAbility : Ability 
{
    public float AttackDistance { get; protected set; }
    public float AttackDistanceMargin { get; protected set; }
    public float AttackAngleMargin { get; protected set; }

    protected EnemyAbilityType type;

    //Runs methods based on state of the skill.
    public virtual void Initialize(EnemyAbilityManager abilitySystem)
    {
        this.system = abilitySystem;
        state = AbilityState.Waiting;
    }

    public void UpdateAbility()
    {
        if (state == AbilityState.CoolingDown)
        {
            CoolDown();
        }
    }

    public void Queue(EnemyAbilityType type)
    {
        ((EnemyAbilityManager) system).QueuedAbilities.Enqueue(this);
        ((EnemyAbilityManager) system).QueuedTypes.Enqueue(type);
    }

    //Waits for valid input.
	public bool TryRun(EnemyAbilityType type = EnemyAbilityType.None, bool firstTimeCalling = true)
    {
        if (system.Ready() && system.CurrentAbility == null)
        {
            system.CurrentAbility = this;
            state = AbilityState.InProgress;

            GlobalStart();

            if (firstTimeCalling)
            {
                system.ResetSegmentIndex();
                system.Animator.SetTrigger("runAbility");
                system.Animator.SetBool("exitAbility", false);
            }

            ActiveSegment = segments.Start;
            system.SetNextSegmentClip(segments.Start.Clip);
            this.type = type;

            return true;
        }
        else
        {
            return false;
        }
    }

    protected sealed override void AdvanceSegment()
    {
        if (ActiveSegment.Next != null)
        {
            ActiveSegment = ActiveSegment.Next;
            system.SetNextSegmentClip(ActiveSegment.Clip);
        }
        else
        {
            ToWaiting();

            if (((EnemyAbilityManager) system).QueuedAbilities.Count != 0)
            {
                if (!system.Ready())
                {
                    ((EnemyAbilityManager) system).QueuedAbilities.Clear();
                    system.Animator.SetBool("exitAbility", true);
                }
                else
                {
                    EnemyAbility nextAbility = ((EnemyAbilityManager) system).QueuedAbilities.Dequeue();
                    EnemyAbilityType nextType = ((EnemyAbilityManager) system).QueuedTypes.Dequeue();
                    nextAbility.TryRun(nextType, false);
                }
            }
            else
            {
                system.Animator.SetBool("exitAbility", true);
            }
        }

        system.Animator.SetTrigger("proceedAbility");
    }

    public sealed override void ShortCircuit(bool forceNoReuse = false)
    {
        ShortCircuitLogic();

        StopCoroutine("SegmentCoroutine");

        if (ActiveProcess != null && ActiveProcess.End != null)
            ActiveProcess.End();

        ResetAnimatorSettings();
        ActiveSegment.Finished = true;

        ToWaiting();

        ((EnemyAbilityManager) system).QueuedAbilities.Clear();
        system.Animator.SetBool("exitAbility", false);
        system.Animator.ResetTrigger("proceedAbility");
        system.Animator.ResetTrigger("runAbility");
        
    }
}