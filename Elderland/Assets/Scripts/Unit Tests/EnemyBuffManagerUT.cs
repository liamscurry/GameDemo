using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBuffManagerUT : MonoBehaviour
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
                new EnemyFireChargeDebuff(0.5f, manager, EnemyBuff.BuffType.Debuff, 5f);
            
            var buff =
                new EnemyFireChargeDebuff(2f, manager, EnemyBuff.BuffType.Buff, 8f);

            // Apply, SearchForDebuff tests
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            manager.BuffManager.Apply(debuff);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            manager.BuffManager.Apply(debuff);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);

            UT.CheckEquality<float>(debuff.Timer, 0.0f);
            debuff.UpdateBuff();
            UT.CheckEquality<bool>(debuff.Timer != 0, true);
            manager.BuffManager.Apply(debuff);
            UT.CheckEquality<float>(debuff.Timer, 0.0f);

            // Apply, SearchForBuff
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            manager.BuffManager.Apply(buff);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 1);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            manager.BuffManager.Apply(buff);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 1);

            UT.CheckEquality<float>(buff.Timer, 0.0f);
            buff.UpdateBuff();
            UT.CheckEquality<bool>(buff.Timer != 0, true);
            manager.BuffManager.Apply(buff);
            UT.CheckEquality<float>(buff.Timer, 0.0f);

            // UpdateBuffs
            manager.BuffManager.UpdateBuffs();
            float currentDebuffTime = debuff.Timer;
            float currentBuffTime = buff.Timer;
            UT.CheckEquality<bool>(debuff.Timer != 0, true);
            UT.CheckEquality<bool>(buff.Timer != 0, true);
            manager.BuffManager.UpdateBuffs();
            UT.CheckEquality<bool>(debuff.Timer > currentDebuffTime, true);
            UT.CheckEquality<bool>(buff.Timer > currentBuffTime, true);

            // Clear
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 1);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            manager.BuffManager.Clear(buff);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);

            manager.BuffManager.Clear(debuff);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 0);

            // ClearBuffs and ClearDebuffs
            manager.BuffManager.Apply(debuff);
            manager.BuffManager.Apply(buff);
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 1);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            manager.BuffManager.ClearBuffs();
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 1);
            manager.BuffManager.ClearDebuffs();
            UT.CheckEquality<int>(manager.BuffManager.Buffs.Count, 0);
            UT.CheckEquality<int>(manager.BuffManager.Debuffs.Count, 0);

            manager.BuffManager.ClearBuffs();
            manager.BuffManager.ClearDebuffs();

            Debug.Log("EnemyBuffManager: Success");
        } 
        catch
        {
            Debug.Log("EnemyBuffManager: Failed");
        }
    }
}
