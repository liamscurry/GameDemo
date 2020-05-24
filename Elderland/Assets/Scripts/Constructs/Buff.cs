using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType { Buff, Debuff }

public abstract class Buff<T>
{

    public BuffType Type { get; private set; }

    protected BuffManager<T> manager;

    protected float timer;
    protected float duration;

    public float Timer { get { return timer; } }
    public float Duration { get { return duration; } set { duration = value; } }

    public Buff(BuffManager<T> manager, BuffType type, float duration)
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
            manager.Clear(this);
        }
    }

    public void Reset()
    {
        timer = 0;
    }

    public abstract void ApplyBuff();
    public abstract void ReverseBuff();
}