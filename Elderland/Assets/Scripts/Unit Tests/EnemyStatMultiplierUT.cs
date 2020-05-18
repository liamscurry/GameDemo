using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatMultiplierUT : MonoBehaviour
{
    void Start()
    {
        try
        {
            EnemyStatMultiplier multiplier = new EnemyStatMultiplier(1);
            UT.CheckEquality<float>(multiplier.Value, 1.0f);  
            multiplier.AddModifier(0.5f);
            UT.CheckEquality<float>(multiplier.Value, 0.5f);  
            multiplier.AddModifier(2f);
            UT.CheckEquality<float>(multiplier.Value, 1.0f);  
            multiplier.AddModifier(0.25f);
            UT.CheckEquality<float>(multiplier.Value, 0.25f);  
            multiplier.RemoveModifier(0.5f);
            UT.CheckEquality<float>(multiplier.Value, 0.5f); 
            multiplier.BaseValue = -2f; 
            UT.CheckEquality<float>(multiplier.Value, -1f); 

            Debug.Log("EnemyStatMultiplier: Success");
        } 
        catch
        {
            Debug.Log("EnemyStatMultiplier: Failed");
        }
    }
}
