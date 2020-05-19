using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBuffManager
{
    private EnemyManager manager;
    private List<EnemyBuff> buffs;
    private List<EnemyBuff> debuffs;

    public List<EnemyBuff> Buffs { get { return buffs; } }
    public List<EnemyBuff> Debuffs { get { return debuffs; } }

    public EnemyBuffManager(EnemyManager manager)
    {
        this.manager = manager;
        buffs = new List<EnemyBuff>();
        debuffs = new List<EnemyBuff>();
    }

    public void UpdateBuffs()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            buffs[i].UpdateBuff();
        }

        for (int i = debuffs.Count - 1; i >= 0; i--)
        {
            debuffs[i].UpdateBuff();
        }
    }

    public void Apply<T>(T effect) where T : EnemyBuff
    {
        if (effect.Type == EnemyBuff.BuffType.Buff)
        {
            SearchForBuff<T>(effect);
        }
        else
        {
            SearchForDebuff<T>(effect);
        }
    }

    private void SearchForBuff<T>(T effect) where T : EnemyBuff
    {
        foreach (EnemyBuff buff in buffs)
        {
            if (buff is T)
            {
                if (buff.Duration - buff.Timer < effect.Duration)
                {
                    buff.Reset();
                    buff.Duration = effect.Duration;
                }

                return;
            }
        }

        buffs.Add(effect);
        effect.ApplyBuff();
    }

    private void SearchForDebuff<T>(T effect) where T : EnemyBuff
    {
        foreach (EnemyBuff debuff in debuffs)
        {
            if (debuff is T)
            {
                if (debuff.Duration - debuff.Timer < effect.Duration)
                {
                    debuff.Reset();
                    debuff.Duration = effect.Duration;
                }

                return;
            }
        }

        debuffs.Add(effect);
        effect.ApplyBuff();
    }

    public void Clear(EnemyBuff effect)
    {
        effect.ReverseBuff();
        if (effect.Type == EnemyBuff.BuffType.Buff)
        {
            buffs.Remove(effect);
        }
        else
        {
            debuffs.Remove(effect);    
        }
    }

    public void ClearBuffs()
    {
        foreach (EnemyBuff buff in buffs)
        {
            buff.ReverseBuff();
        }
        buffs.Clear();
    }

    public void ClearDebuffs()
    {
        foreach (EnemyBuff debuff in debuffs)
        {
            debuff.ReverseBuff();
        }
        debuffs.Clear();
    }
}
