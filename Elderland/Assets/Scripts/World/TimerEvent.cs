using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent timerEvent;
    [SerializeField]
    private float delay;
    [SerializeField]
    private bool executed;

    public void Enable()
    {
        executed = false;
        StopAllCoroutines();
    }

    public void Disable()
    {
        executed = true;
        StopAllCoroutines();
    }

    public void Trigger()
    {
        if (!executed)
        {
            executed = true;
            StartCoroutine(TimerCoroutine());
        }
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(delay);
        if (timerEvent != null)
            timerEvent.Invoke();
    }
}
