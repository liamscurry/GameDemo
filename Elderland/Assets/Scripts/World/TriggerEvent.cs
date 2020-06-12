using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent triggerEvent;
    [SerializeField]
    protected string triggerTag = "PlayerHealth";
    [SerializeField]
    protected bool executed;
    [SerializeField]
    protected bool repeatable;

    public void Reset()
    {
        executed = false;
    }

    public void Enable()
    {
        executed = false;
    }

    public void Disable()
    {
        executed = true;
    }

    protected virtual bool HasMetRequirements(GameObject invoker)
    {
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == triggerTag && gameObject.activeInHierarchy)
        {
            if ((!executed || repeatable) && HasMetRequirements(other.gameObject))
            {
                executed = true;
                triggerEvent.Invoke();
            }
        }
    }
}
