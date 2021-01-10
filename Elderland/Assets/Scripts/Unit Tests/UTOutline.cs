using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UT
{
    public static float Threshold = 0.01f;

    public static void CheckEquality<T>(
        T expression1,
        T expression2) where T : IComparable<T>
    {
        if (expression1 == null)
        {
            T temp = expression2;
            expression2 = expression1;
            expression1 = temp;
        }

        if (expression1 == null)
        {
            return;
        }

        if (expression1.CompareTo(expression2) != 0)
        {
            throw new System.Exception(
                expression1 +
                " != " +
                expression2);
        }
    }

    public static void CheckDifference<T>(
        T expression1,
        T expression2) where T : IComparable<T>
    {
        if (expression1 == null)
        {
            T temp = expression2;
            expression2 = expression1;
            expression1 = temp;
        }

        if (expression1 == null)
        {
            throw new System.Exception(
                "null" +
                " == " +
                "null");
        }

        if (expression1.CompareTo(expression2) == 0)
        {
            throw new System.Exception(
                expression1 +
                " == " +
                expression2);
        }
    }
}

// Generic outlines for testing a class
/* Testing methods will be public static methods in the below class, unless a concrete testing
   scenario is needed, in which then it is identicle but with a concretic implementation for test purposes.
   This allows for private and protected method tests.
   
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLASSNAMEUT : MonoBehaviour
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
            UT.CheckEquality<float>(1.0f, 1.0f);  

            Debug.Log("CLASSNAME: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("CLASSNAME: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}
*/