using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatLock
{
    private object tracker;

    public bool Value { get; private set; }

    public StatLock()
    {
        this.tracker = null;
        Value = false;
    }

    public StatLock(bool value)
    {
        this.tracker = null;
        Value = value;
    }

    public void ClaimLock(object tracker, bool value)
    {
        this.tracker = tracker;
        Value = value;
    }

    public void TryReleaseLock(object tracker, bool value)
    {
        if (this.tracker == tracker)
        {
            this.tracker = null;
            Value = value;
        }
    }

    public static void ContructorTests()
    {
        StatLock lock1 = new StatLock();
        UT.CheckEquality<bool>(lock1.Value, false);  
        UT.CheckEquality<bool>(lock1.tracker == null, true);  

        StatLock lock2 = new StatLock(true);
        UT.CheckEquality<bool>(lock2.Value, true);  
        UT.CheckEquality<bool>(lock2.tracker == null, true);  

        StatLock lock3 = new StatLock(false);
        UT.CheckEquality<bool>(lock3.Value, false);  
        UT.CheckEquality<bool>(lock3.tracker == null, true);  
    }

    public static void ClaimLockTests()
    {
        StatLock lock1 = new StatLock();
        object object1 = new Object();
        object object2 = new Object();
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
        StatLock lock1 = new StatLock();
        object object1 = new Object();
        object object2 = new Object();
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