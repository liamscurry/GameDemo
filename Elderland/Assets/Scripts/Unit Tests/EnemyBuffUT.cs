using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Also tests EnemyFireChargeDebuff.

public class EnemyBuffUT : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            var manager = 
                GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyManager>();

            var debuff =
                new EnemyFireChargeDebuff(0.5f, manager.BuffManager, BuffType.Debuff, 1f);
            UT.CheckEquality<bool>(debuff != null, true);  
            UT.CheckEquality<float>(debuff.Timer, 0);  
            debuff.UpdateBuff();
            UT.CheckEquality<float>(debuff.Timer, Time.deltaTime); 
            float time = 2 * Time.deltaTime;
            debuff.UpdateBuff();
            UT.CheckEquality<float>(debuff.Timer, time); 
            debuff.Reset();
            UT.CheckEquality<float>(debuff.Timer, 0.0f); 

            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 0); 
            UT.CheckEquality<float>(manager.StatsManager.DamageTakenMultiplier.Value, 1); 
            manager.BuffManager.Apply(debuff);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1); 
            UT.CheckEquality<float>(manager.StatsManager.DamageTakenMultiplier.Value, 0.5f); 

            time = 0;
            debuff.Reset();
            while (time < 1)
            {
                time += Time.deltaTime;
                debuff.UpdateBuff();
            }
            time += Time.deltaTime;
            debuff.UpdateBuff();

            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 0); 
            UT.CheckEquality<float>(manager.StatsManager.DamageTakenMultiplier.Value, 1);

            manager.BuffManager.ClearBuffs();
            manager.BuffManager.ClearDebuffs();

            Debug.Log("EnemyBuff: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("EnemyBuff: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}
