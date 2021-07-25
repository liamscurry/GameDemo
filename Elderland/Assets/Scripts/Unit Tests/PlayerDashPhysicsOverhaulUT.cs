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
Ability test with new physics system integration (CharacterMovementSystem)
Tested by inspection.
*/
public class PlayerDashPhysicsOverhaulUT : MonoBehaviour
{
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
        
        yield return GroundTests();
    }

    /*
    Expected Behaviour:
    Dashes forward towards edge.
    Dashes off edge forward and only goes to around near end of steps forward.
    Walks left to stairs
    Walks backward, dashes up stairs.
    Walks right
    Walks forward and dashes such that the dash ends right as the player leaves the ledge, but
    the player only travels a couple of feet off the edge as the limiter should be in place.
    */
    private IEnumerator GroundTests()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.25f);
        QueueDash();

        yield return new WaitForSeconds(1f);
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
        CancelDash();

        yield return new WaitForSeconds(0.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        QueueDash();

        yield return new WaitForSeconds(0.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);
        CancelDash();
        yield return new WaitForSeconds(2f);

        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, -1).normalized * 0.95f);
        yield return new WaitForSeconds(1f);
        QueueDash();

        yield return new WaitForSeconds(1f);
        CancelDash();
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(3f);

        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        QueueDash();

        yield return new WaitForSeconds(0.5f);
        CancelDash();
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
    }   

    private void QueueDash()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonEast.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }

    private void CancelDash()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonEast.WriteValueIntoEvent(0.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }
}
