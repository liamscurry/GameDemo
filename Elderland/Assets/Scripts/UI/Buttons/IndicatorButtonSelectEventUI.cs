using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class IndicatorButtonSelectEventUI : ButtonSelectEventUI
{
    [SerializeField]
    private GameObject indicatorObject;

    public override void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null)
            onSelect.Invoke();
        indicatorObject.SetActive(true);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null)
            onDeselect.Invoke();
        indicatorObject.SetActive(false);
    }
    
    public override void OnDisable()
    {
        if (onDeselect != null)
            onDeselect.Invoke();
        indicatorObject.SetActive(false);
    }
}
