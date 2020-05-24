using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LightEnemyManager : EnemyManager
{
    public LightEnemySword Sword { get; private set; }
    public LightEnemyCharge Charge { get; private set; }

    protected override void DeclareAbilities()
    {
        Sword = GetComponent<LightEnemySword>();
        AbilityManager.ApplyAbility(Sword);

        Charge = GetComponent<LightEnemyCharge>();
        AbilityManager.ApplyAbility(Charge);

        MaxHealth = 3;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Melee;
    }

    public override void ChooseNextAbility()
    {
        Vector2 projectedPosition = Matho.StandardProjection2D(transform.position);
        Vector2 projectedPlayerPosition = Matho.StandardProjection2D(PlayerInfo.Player.transform.position);
        float horizontalDistanceToPlayer = Vector2.Distance(projectedPosition, projectedPlayerPosition);

        if (Health <= (MaxHealth / 3f) && horizontalDistanceToPlayer > Charge.AttackDistance - Charge.AttackDistanceMargin)
        {
            NextAttack = Charge;
        }
        else
        {
            NextAttack = Sword;
        }
    }

    protected override void SpawnPickups()
    {
        Pickup.SpawnPickups<HealthPickup>(
            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
            transform.position,
            1,
            3f,
            90f);
    }
}