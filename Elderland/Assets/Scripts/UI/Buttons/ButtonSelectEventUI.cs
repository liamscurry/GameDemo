using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonSelectEventUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    protected UnityEvent onSelect;
    [SerializeField]
    protected UnityEvent onDeselect;

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null)
            onSelect.Invoke();
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null)
            onDeselect.Invoke();
    }
    
    public virtual void OnDisable()
    {
        if (onDeselect != null)
            onDeselect.Invoke();
    }
}
