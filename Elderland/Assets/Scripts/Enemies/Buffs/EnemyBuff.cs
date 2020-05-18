using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBuff
{
    public enum BuffType { Buff, Debuff }

    public BuffType Type { get; private set; }

    protected EnemyManager manager;

    protected float timer;
    protected float duration;

    public float Timer { get { return timer; } }
    public float Duration { get { return duration; } set { duration = value; } }

    public EnemyBuff(EnemyManager manager, BuffType type, float duration)
    {
        this.manager = manager;
        this.Type = type;
        this.duration = duration;
        this.timer = 0;
    }

    public void UpdateBuff()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            manager.BuffManager.Clear(this);
        }
    }

    public void Reset()
    {
        timer = 0;
    }

    public abstract void ApplyBuff();
    public abstract void ReverseBuff();
}