using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages applying abilities and updating them.
public class EnemyAbilityManager : AbilitySystem
{
    private List<EnemyAbility> abilities;

    public EnemyManager Manager { get; private set; }
    public Queue<EnemyAbility> QueuedAbilities { get; private set; }
    public Queue<EnemyAbilityType> QueuedTypes { get; private set; }
    public new EnemyAbility CurrentAbility { get { return (EnemyAbility) currentAbility; } set { currentAbility = value; } }

    public EnemyAbilityManager(
        Animator animator,
        AnimatorOverrideController animatorController,
        PhysicsSystem physics,
        MovementSystem movement,
        GameObject parent) : base(animator, animatorController, physics, movement, parent)
    { 
        Manager = parent.GetComponent<EnemyManager>();
        abilities = new List<EnemyAbility>();
        QueuedAbilities = new Queue<EnemyAbility>();
        QueuedTypes = new Queue<EnemyAbilityType>();
    }

	public override void UpdateAbilities() 
    {
        foreach (EnemyAbility ability in abilities)
        {
            ability.UpdateAbility();
        }
	}

    public void ApplyAbility(EnemyAbility ability)
    {
        ability.Initialize(this);
        abilities.Add(ability);
    }

    public void StartQueue()
    {
        QueuedAbilities.Dequeue().TryRun(QueuedTypes.Dequeue());
    }

    public void CancelQueue()
    {
        QueuedAbilities.Clear();
        QueuedTypes.Clear();
    }

    public override bool Ready()
    {
        return true;
    }
}