using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatLockUT : MonoBehaviour
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
            StatLock<bool>.ContructorTests();
            StatLock<bool>.ClaimLockTests();
            StatLock<bool>.TryReleaseLockTests();
            StatLock<bool>.NotifyLockTests();

            Debug.Log("StatLockUT: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("StatLockUT: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}