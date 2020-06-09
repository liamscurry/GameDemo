using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HeavyEnemyManager : EnemyManager
{
    public HeavyEnemyHorizontalSword HorizontalSword { get; private set; }
    public HeavyEnemyVerticalSword VerticalSword { get; private set; }

    protected override void DeclareAbilities()
    {
        HorizontalSword = GetComponent<HeavyEnemyHorizontalSword>();
        AbilityManager.ApplyAbility(HorizontalSword);

        VerticalSword = GetComponent<HeavyEnemyVerticalSword>();
        AbilityManager.ApplyAbility(VerticalSword);

        MaxHealth = 6;
        Health = MaxHealth;
    }

    protected override void DeclareType()
    {
        Type = EnemyType.Melee;
    }

    protected override void SpawnPickups()
    {
        /*Pickup.SpawnPickups<HealthPickup>(
            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
            transform.position,
            0,
            3f,
            90f);*/
    }

    public override void ChooseNextAbility()
    {
        int chance = EnemyInfo.AbilityRandomizer.Next(10) + 1;

        if (chance <= 5)
        {
            VCombo();
        }
        else
        {
            if (chance == 6)
            {
                x2VCombo();
            }
            else if (chance == 7)
            {
                x2HCombo();
            }
            else if (chance == 8 || chance == 9)
            {
                VHCombo();
            }
            else if (chance == 10)
            {
                VHVCombo();
            }
        }
    }

    private void VCombo()
    {
        NextAttack = VerticalSword;
        VerticalSword.Queue(EnemyAbilityType.None);
    }

    private void x2VCombo()
    {
        NextAttack = VerticalSword;
        VerticalSword.Queue(EnemyAbilityType.First);
        VerticalSword.Queue(EnemyAbilityType.Last);
    }

    private void x2HCombo()
    {
        NextAttack = HorizontalSword;
        HorizontalSword.Queue(EnemyAbilityType.First);
        HorizontalSword.Queue(EnemyAbilityType.Last);
    }

    private void VHCombo()
    {
        NextAttack = VerticalSword;
        VerticalSword.Queue(EnemyAbilityType.First);
        HorizontalSword.Queue(EnemyAbilityType.Last);
    }
    
    private void VHVCombo()
    {
        NextAttack = VerticalSword;
        VerticalSword.Queue(EnemyAbilityType.First);
        HorizontalSword.Queue(EnemyAbilityType.Middle);
        VerticalSword.Queue(EnemyAbilityType.Last);
    }
}
