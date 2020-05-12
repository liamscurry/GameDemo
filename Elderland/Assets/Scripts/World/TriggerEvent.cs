using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent triggerEvent;
    [SerializeField]
    private string triggerTag = "PlayerHealth";

    private bool executed;

    public void Reset()
    {
        executed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == triggerTag && gameObject.activeInHierarchy)
        {
            if (!executed)
            {
                executed = true;
                triggerEvent.Invoke();
            }
        }
    }
}
