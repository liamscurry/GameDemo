using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DebrisTriggerEvent : TriggerEvent
{
    protected override bool HasMetRequirements(GameObject invoker)
    {
        return invoker.GetComponent<DebrisTriggerTag>() != null;
    }
}
