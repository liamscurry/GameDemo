using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupEvent : MonoBehaviour
{
    [SerializeField]
    private UnityEvent acceptEvent;
    //[SerializeField]
    //private bool executed;

    /*
    public void Enable()
    {
        executed = false;
    }

    public void Disable()
    {
        executed = true;
    }
    */
    
    public void Update()
    {
        if (Input.GetKeyDown(GameInfo.Settings.UseKey))
        {
            //if (!executed)
            {
                //executed = true;
                if (acceptEvent != null)
                    acceptEvent.Invoke();
            }
        }
    }
}
