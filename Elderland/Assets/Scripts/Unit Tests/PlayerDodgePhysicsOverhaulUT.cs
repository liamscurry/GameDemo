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
public class PlayerDodgePhysicsOverhaulUT : MonoBehaviour
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
    Dodges forward towards edge.
    Dodges off edge forward and only goes to around near end of steps forward.
    Walks left to stairs
    Walks backward, dodges up stairs.
    Walks right
    Walks forward and dodges such that the Dodges ends right as the player leaves the ledge, but
    the player only travels a couple of feet off the edge as the limiter should be in place.
    */
    private IEnumerator GroundTests()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.25f);
        QueueDodge();

        yield return new WaitForSeconds(1f);
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
        CancelDodge();

        yield return new WaitForSeconds(0.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        QueueDodge();

        yield return new WaitForSeconds(0.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);
        CancelDodge();
        yield return new WaitForSeconds(2f);

        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, -1).normalized * 0.95f);
        yield return new WaitForSeconds(1f);
        QueueDodge();

        yield return new WaitForSeconds(1f);
        CancelDodge();
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, -1).normalized * 0.95f);
        yield return new WaitForSeconds(3f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(3.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(2f);
        QueueDodge();

        yield return new WaitForSeconds(0.5f);
        CancelDodge();
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
    }   

    private void QueueDodge()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }

    private void CancelDodge()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(0.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }
}
