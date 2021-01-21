using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelMechanic : MonoBehaviour
{
    [SerializeField]
    private UnityEvent resetEvent;
    
    public UnityEvent ResetEvent { get { return resetEvent; } }
    public virtual void ResetSelf() {}
    public virtual void InvokeSelf() {}
}
