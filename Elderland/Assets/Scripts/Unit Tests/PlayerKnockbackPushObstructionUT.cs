using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Vision, Input based UT
/*
Ability test to obstruction checks for knockbackpush ability. To run test, make sure in player knockback
push.cs #define UT is uncommented at the top of the file.
*/
public class PlayerKnockbackPushObstructionUT : MonoBehaviour
{
    /*
    [Header("Enemy Colliders")]
    [SerializeField]
    private Collider leftDiagonal;
    [SerializeField]
    private Collider frontFar;
    [SerializeField]
    private Collider frontNear;
    [SerializeField]
    private Collider rightDiagonal;
    */

    private XInputController fakeController;

    private void Start()
    {
        fakeController = InputSystem.AddDevice<XInputController>();
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        StartCoroutine(InputCoroutine());
    }

    /*
    * Coroutine that runs all specified tests.
    */
    private IEnumerator InputCoroutine()
    {
        GameInfo.Settings.CurrentGamepad = fakeController;

        while (!PlayerInfo.CharMoveSystem.Grounded)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(3f);

        yield return Tests(); 
    }

    /*
    Expected printout:
    True
    True
    False
    True
    */
    private IEnumerator Tests()
    {
        QueueKnockback();
        yield return new WaitForSeconds(1.0f);
        CancelKnockback();
    }   

    private void QueueKnockback()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.leftShoulder.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }

    private void CancelKnockback()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.leftShoulder.WriteValueIntoEvent(0.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }
}
