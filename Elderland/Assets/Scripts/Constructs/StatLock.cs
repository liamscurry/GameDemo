using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatLock<T>
{
    private object tracker;

    public T Value { get; private set; }

    public StatLock()
    {
        this.tracker = null;
    }

    public StatLock(T value)
    {
        this.tracker = null;
        Value = value;
    }

    public void ClaimLock(object tracker, T value)
    {
        this.tracker = tracker;
        Value = value;
    }

    public void TryReleaseLock(object tracker, T value)
    {
        if (this.tracker == tracker)
        {
            this.tracker = null;
            Value = value;
        }
    }

    public static void ContructorTests()
    {
        var lock1 = new StatLock<bool>();
        UT.CheckEquality<bool>(lock1.Value, false);  
        UT.CheckEquality<bool>(lock1.tracker == null, true);  

        var lock2 = new StatLock<bool>(true);
        UT.CheckEquality<bool>(lock2.Value, true);  
        UT.CheckEquality<bool>(lock2.tracker == null, true);  

        var lock3 = new StatLock<bool>(false);
        UT.CheckEquality<bool>(lock3.Value, false);  
        UT.CheckEquality<bool>(lock3.tracker == null, true);  
    }

    public static void ClaimLockTests()
    {
        var lock1 = new StatLock<bool>();
        object object1 = new object();
        object object2 = new object();
        lock1.ClaimLock(object1, true);
        UT.CheckEquality<bool>(lock1.Value, true);  
        UT.CheckEquality<bool>(lock1.tracker == object1, true);  
        UT.CheckEquality<bool>(lock1.tracker == object2, false);  
        lock1.ClaimLock(object2, false);
        UT.CheckEquality<bool>(lock1.Value, false);  
        UT.CheckEquality<bool>(lock1.tracker == object1, false);  
        UT.CheckEquality<bool>(lock1.tracker == object2, true);  
    }

    public static void TryReleaseLockTests()
    {
        var lock1 = new StatLock<bool>();
        object object1 = new object();
        object object2 = new object();
        lock1.ClaimLock(object1, true);
        UT.CheckEquality<bool>(lock1.Value, true);  
        UT.CheckEquality<bool>(lock1.tracker == object1, true);  
        UT.CheckEquality<bool>(lock1.tracker == object2, false);  
        lock1.TryReleaseLock(object1, false);
        UT.CheckEquality<bool>(lock1.Value, false);  
        UT.CheckEquality<bool>(lock1.tracker == null, true);  

        lock1.ClaimLock(object1, true);
        lock1.TryReleaseLock(object2, false);
        UT.CheckEquality<bool>(lock1.Value, true);  
        UT.CheckEquality<bool>(lock1.tracker == object1, true);  
        UT.CheckEquality<bool>(lock1.tracker == object2, false);  
    }
}