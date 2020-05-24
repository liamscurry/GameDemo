using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager<T>
{
    private T manager;
    private List<Buff<T>> buffs;
    private List<Buff<T>> debuffs;

    public List<Buff<T>> Buffs { get { return buffs; } }
    public List<Buff<T>> Debuffs { get { return debuffs; } }

    public T Manager { get { return manager; } }

    public BuffManager(T manager)
    {
        this.manager = manager;
        buffs = new List<Buff<T>>();
        debuffs = new List<Buff<T>>();
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

    public void Apply<U>(U effect) where U : Buff<T>
    {
        if (effect.Type == BuffType.Buff)
        {
            SearchForBuff<U>(effect);
        }
        else
        {
            SearchForDebuff<U>(effect);
        }
    }

    private void SearchForBuff<U>(U effect) where U : Buff<T>
    {
        foreach (Buff<T> buff in buffs)
        {
            if (buff is U)
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

    private void SearchForDebuff<U>(U effect) where U : Buff<T>
    {
        foreach (Buff<T> debuff in debuffs)
        {
            if (debuff is U)
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

    public void Clear(Buff<T> effect)
    {
        effect.ReverseBuff();
        if (effect.Type == BuffType.Buff)
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
        foreach (Buff<T> buff in buffs)
        {
            buff.ReverseBuff();
        }
        buffs.Clear();
    }

    public void ClearDebuffs()
    {
        foreach (Buff<T> debuff in debuffs)
        {
            debuff.ReverseBuff();
        }
        debuffs.Clear();
    }
}
