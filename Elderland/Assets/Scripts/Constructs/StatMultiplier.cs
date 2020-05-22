using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatMultiplier
{
    private List<float> modifiers;

    public float BaseValue { get; set; }
    
    public float Value
    {
        get
        {
            float value = BaseValue;
            foreach (float modifier in modifiers)
            {
                value *= modifier;
            }
            return value;
        }
    }

    public StatMultiplier(float baseValue)
    {
        this.BaseValue = baseValue;
        modifiers = new List<float>();
    }

    public void AddModifier(float modifier)
    {
        if (modifier >= 0)
            modifiers.Add(modifier);
    }

    public void RemoveModifier(float modifier)
    {
        modifiers.Remove(modifier);
    }
}
