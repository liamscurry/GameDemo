using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;

// Input based UT
public class CharacterMovementSystemEdgeSlideUT : MonoBehaviour
{
    XInputController fakeController;
    
    private void Awake()
    {
        fakeController = InputSystem.AddDevice<XInputController>();
        StartCoroutine(InputCoroutine());
    }

    private IEnumerator InputCoroutine()
    {
        yield return new WaitForSeconds(3f);

        InputEventPtr jumpTest;
        using (StateEvent.From(fakeController, out jumpTest))
        {
            fakeController.buttonNorth.WriteValueIntoEvent(1f, jumpTest);
            InputSystem.QueueEvent(jumpTest);
        }
        Debug.Log("called");
    }
}
