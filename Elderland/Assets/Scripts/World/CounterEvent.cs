using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CounterEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent countEvent;
    [SerializeField]
    private bool executed;
    [SerializeField]
    private int ExecuteCount;

    private int currentCount;

    public void Enable()
    {
        executed = false;
        currentCount = 0;
    }

    public void Disable()
    {
        executed = true;
        currentCount = ExecuteCount;
    }

    public void IncreaseCounter()
    {
        currentCount++;

        if (currentCount >= ExecuteCount &&
            !executed)
        {
            executed = true;
            if (countEvent != null)
                countEvent.Invoke();
        }
    }
}
