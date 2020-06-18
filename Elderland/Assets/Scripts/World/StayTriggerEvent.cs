using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StayTriggerEvent : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent stayEvent;
    [SerializeField]
    protected string triggerTag = "PlayerHealth";
    [SerializeField]
    protected bool active;

    public void Enable()
    {
        active = true;
    }

    public void Disable()
    {
        active = false;
    }

    protected virtual bool HasMetRequirements(GameObject invoker)
    {
        return true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == triggerTag && gameObject.activeInHierarchy)
        {
            if (active && HasMetRequirements(other.gameObject))
            {
                stayEvent.Invoke();
            }
        }
    }
}
