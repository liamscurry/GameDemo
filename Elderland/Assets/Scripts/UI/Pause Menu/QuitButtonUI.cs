using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class QuitButtonUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    protected GameObject quitSaveWarningObject;

    public virtual void OnSelect(BaseEventData eventData)
    {
        quitSaveWarningObject.gameObject.SetActive(true);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        quitSaveWarningObject.gameObject.SetActive(false);
    }
    
    public void OnDisable()
    {
        quitSaveWarningObject.gameObject.SetActive(false);
    }
}
