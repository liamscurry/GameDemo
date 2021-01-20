using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ability structure that supports any amount of states and substates. Has an optional cooldown period.
//Works for player, enemies and any controller that needs has an ability system.

//Interfaces with animator system for automatic logic calling from StateBehaviours upon invokation.
//Achieves any number of states by using a triangle state loop. 
//Controllers should be modeled after the PlayerAnimationController.
public abstract class Ability : MonoBehaviour 
{
    public enum AbilityState { Waiting, InProgress, CoolingDown }

    //Fields//
    protected AbilityState state;
    protected float coolDownTimer;
    protected float fixedDuration;
    protected float fixedTimer;
    protected bool fixedStarted;
    protected bool fixedFinished;
    protected float abilitySpeed = 1;

    protected AbilitySystem system;
    protected AbilitySegmentList segments;
    protected Vector3 actVelocity;
    protected float coolDownDuration;

    //Properties//
    public AbilityProcess ActiveProcess { get; protected set; }
    public AbilitySegment ActiveSegment { get; protected set; }

    public virtual float AbilitySpeed { get { return abilitySpeed; } }

    public void FixedUpdateAbility()
    {
        if (state == AbilityState.InProgress && ActiveSegment.Type == AbilitySegmentType.Physics && fixedStarted && !fixedFinished)
        {
            fixedTimer += Time.fixedDeltaTime / abilitySpeed;
            if (fixedTimer >= fixedDuration)
            {
                if (system.Physics != null)
                    system.Physics.AnimationVelocity -= actVelocity;
                fixedFinished = true;
            }
        }
    }

    protected virtual void GlobalStart() {}
    public virtual void GlobalUpdate() {}

    protected virtual void CoolDown()
    {
        coolDownTimer += Time.deltaTime;
        if (coolDownTimer >= coolDownDuration)
		{			
            state = AbilityState.Waiting;
		}
    }

    public void ToCoolDown()
    {
        state = AbilityState.CoolingDown;
        coolDownTimer = 0;
        system.CurrentAbility = null;
    }

    public void ToWaiting()
    {
        state = AbilityState.Waiting;
        system.CurrentAbility = null;
    }

    public void StartSegmentCoroutine()
    {
        StartCoroutine("SegmentCoroutine");
    }
    
    //Runs processes of a segment at correct times. The timing method depends on update
    //specification such as with standard update or fixed update for movement.
    //Afterwards either goes to the next segment or finishes logic if there is none.
    public IEnumerator SegmentCoroutine()
    {
        SetAnimatorSettings();
        ActiveSegment.Finished = false;

        switch(ActiveSegment.Type)
        {
            case AbilitySegmentType.Normal:
                yield return ProcessSegment();
                break;
            case AbilitySegmentType.Physics:
                yield return ProcessSegmentPhysics();
                break;
            case AbilitySegmentType.RootMotion:
                yield return ProcessSegment();
                break;
        }

        ResetAnimatorSettings();
        ActiveSegment.Finished = true;
        //Make active segment a member of each segment to eliminate overlap
        
        AdvanceSegment();
    }

    public void StartFixed()
    {
        fixedStarted = true;
    }

    public void ForceAdvanceSegment()
    {
        StopCoroutine("SegmentCoroutine");

        if (ActiveProcess.End != null)
            ActiveProcess.End();

        ResetAnimatorSettings();
        ActiveSegment.Finished = true;
        
        AdvanceSegment();
    }

    public abstract void ShortCircuit(bool forceNoReuse = false);
    public abstract void ShortCircuitLogic();

    protected IEnumerator ProcessSegment()
    {
        AbilityProcess[] processes = ActiveSegment.Processes;
        for (int i = 0; i < processes.Length; i++)
        {
            ActiveProcess = processes[i];
            if (!ActiveProcess.Indefinite)
            {
                float scaledDuration = ActiveProcess.Duration * ActiveSegment.LoopFactor;

                if (ActiveProcess.Begin != null)
                    ActiveProcess.Begin();

                yield return TimeProcess(scaledDuration, ActiveSegment.LoopFactor);

                if (ActiveProcess.End != null)
                    ActiveProcess.End();
            }
            else
            {
                ActiveProcess.IndefiniteFinished = false;

                if (ActiveProcess.Begin != null)
                    ActiveProcess.Begin();

                yield return new WaitUntil(() => (ActiveProcess.IndefiniteFinished));

                if (ActiveProcess.End != null)
                    ActiveProcess.End();
            }
        }

        if (ActiveSegment.NormalizedDuration < 1)
        {
            yield return TimeProcessEnd();
        }
    }

    protected IEnumerator ProcessSegmentPhysics()
    {
        AbilityProcess[] processes = ActiveSegment.Processes;
        for (int i = 0; i < processes.Length; i++)
        {
            ActiveProcess = processes[i];
            fixedStarted = false;
            fixedFinished = false;
            fixedTimer = 0;
            fixedDuration = ActiveProcess.Duration * ActiveSegment.Clip.length * ActiveSegment.LoopFactor;

            if (!ActiveProcess.Indefinite)
            {
                if (ActiveProcess.Begin != null)
                    ActiveProcess.Begin();

                yield return new WaitUntil(() => (fixedTimer >= fixedDuration));

                if (ActiveProcess.End != null)
                    ActiveProcess.End();
            }
            else
            {
                ActiveProcess.IndefiniteFinished = false;

                if (ActiveProcess.Begin != null)
                    ActiveProcess.Begin();

                yield return new WaitUntil(() => (ActiveProcess.IndefiniteFinished));

                if (ActiveProcess.End != null)
                    ActiveProcess.End();
            }
        }

        if (ActiveSegment.NormalizedDuration < 1)
        {
            yield return TimeProcessEnd();
        }
    }

    protected IEnumerator TimeProcess(float scaledDuration, float maxNormalizedTime)
    {
        float durationNormalizedTime = scaledDuration;

        if (system.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextStateInfo = system.Animator.GetNextAnimatorStateInfo(0);
            if (maxNormalizedTime - nextStateInfo.normalizedTime < scaledDuration)
                durationNormalizedTime = maxNormalizedTime - nextStateInfo.normalizedTime;
            yield return new WaitForSeconds(nextStateInfo.length / system.Animator.speed * durationNormalizedTime);
        }
        else
        {
            AnimatorStateInfo stateInfo = system.Animator.GetCurrentAnimatorStateInfo(0);
            if (maxNormalizedTime - stateInfo.normalizedTime < scaledDuration)
                durationNormalizedTime = maxNormalizedTime - stateInfo.normalizedTime;
            yield return new WaitForSeconds(stateInfo.length / system.Animator.speed * durationNormalizedTime);
        }
    }

    protected IEnumerator TimeProcessEnd()
    {
        if (system.Animator.IsInTransition(0))
        {
            AnimatorStateInfo nextStateInfo = system.Animator.GetNextAnimatorStateInfo(0);
            yield return new WaitForSeconds(nextStateInfo.length / system.Animator.speed * (ActiveSegment.LoopFactor - nextStateInfo.normalizedTime));
        }
        else
        {
            AnimatorStateInfo stateInfo = system.Animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length / system.Animator.speed * (ActiveSegment.LoopFactor - stateInfo.normalizedTime));
        }
    }

    //The following AnimatorSettings methods switch the animator to a specific update mode to
    //follow the ability's update mode to have correct timing.
    protected void SetAnimatorSettings()
    {
        if (system.Physics != null)
        {
            system.Physics.Animating = true;
            
            if (ActiveSegment.Type == AbilitySegmentType.Physics)
            {
                system.Physics.ClampWhileAnimating = true;
            }
        }

        if (ActiveSegment.Type == AbilitySegmentType.RootMotion)
        {
            if (system.Physics != null)
                system.Physics.Body.isKinematic = true;
            system.Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            system.Animator.applyRootMotion = true;
        }

        system.Animator.speed = abilitySpeed;
    }

    protected void ResetAnimatorSettings()
    {
        if (system.Physics != null)
        {
            system.Physics.Animating = false;

            if (ActiveSegment.Type == AbilitySegmentType.Physics)
            {
                system.Physics.ClampWhileAnimating = false;
            }
        }

        if (ActiveSegment.Type == AbilitySegmentType.RootMotion)
        {
            if (system.Physics != null)
                system.Physics.Body.isKinematic = false;
            system.Animator.updateMode = AnimatorUpdateMode.Normal;
            system.Animator.applyRootMotion = false;
        }

        system.Animator.speed = 1;
    }

    protected abstract void AdvanceSegment();
    
    public abstract bool OnHit(GameObject character);
    public virtual void OnStay(GameObject character) {}
    public virtual void OnLeave(GameObject character) {}
    public virtual void DeleteResources() {}
}