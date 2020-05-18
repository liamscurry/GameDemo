using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UT
{
    public static void CheckEquality<T>(
        T expression1,
        T expression2) where T : IComparable<T>
    {
        if (expression1.CompareTo(expression2) != 0)
        {
            Debug.Log(
                "Failed: " +
                expression1 +
                " != " +
                expression2);
            //throw new System.Exception();
        }
    }
}

// Generic outlines for testing a class
/*
public class CLASSNAMEUT : MonoBehaviour
{
    void Start()
    {
        try
        {
            UT.CheckEquality<float>(1.0f, 1.0f);  

            Debug.Log("CLASSNAME: Success");
        } 
        catch
        {
            Debug.Log("CLASSNAME: Failed");
        }
    }
}
*/