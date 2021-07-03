using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Must call Update Abilities in update and FixedUpdateAbilities in fixedupdate every frame.
//Furthermore, the implementation of UpdateAbilities must call UpdateAbility on each of its abilities every.
public abstract class AbilitySystem 
{
    protected Ability currentAbility;

    public Animator Animator { get; private set; }
    public PhysicsSystem Physics { get; private set; }
    public MovementSystem Movement { get; private set; }
    public CharacterMovementSystem CharMoveSystem { get; private set; }
    public GameObject Parent { get; private set; }
    public Ability CurrentAbility { get { return currentAbility; } set { currentAbility = value; } }
    public AnimationLoop AnimationLoop { get; protected set; }

    public AbilitySystem(
        Animator animator,
        AnimatorOverrideController animatorController,
        PhysicsSystem physics,
        MovementSystem movement,
        CharacterMovementSystem charMoveSystem,
        GameObject parent)
    {
        Animator = animator;
        Physics = physics;
        Movement = movement;
        CharMoveSystem = charMoveSystem;
        Parent = parent;

        AnimationLoop = new AnimationLoop(animatorController, Animator, "Segment");
    }

	public abstract void UpdateAbilities();

    public virtual void FixedUpdateAbilities()
    {
        if (CurrentAbility != null)
        {
            CurrentAbility.FixedUpdateAbility();
        }
    }

    public virtual bool Ready()
    {
        return Physics.TouchingFloor;
    }
}