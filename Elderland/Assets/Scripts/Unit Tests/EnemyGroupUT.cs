using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupUT : MonoBehaviour
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
            EnemyGroup.AddTest();
            EnemyGroup.RemoveTest();
            EnemyGroup.CalculateCenterTest();
            EnemyGroup.MoveTest();
            EnemyGroup.AbsoluteAngleBetweenTest();
            EnemyGroup.CalculateRotationConstantTest();
            EnemyGroup.RotateTest();
            EnemyGroup.ExpandTest();

            Debug.Log("EnemyGroup: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("EnemyGroup: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}