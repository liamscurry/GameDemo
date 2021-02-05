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
            StatLock.ContructorTests();
            StatLock.ClaimLockTests();
            StatLock.TryReleaseLockTests();

            Debug.Log("StatLockUT: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("StatLockUT: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}