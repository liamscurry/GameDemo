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
public class PlayerFireballPhysicsOverhaulUT : MonoBehaviour
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
    Walks left right and left again, all the while shooting fireballs.
    Then walks forward off ledge still firing fireballs in the air.
    After landing, begins firing fireballs again and falls off a small ledge.
    Keeps going forward a few feet while firing a last few fireballs and then stops.
    This entire process the player is shooting fireballs, and the movement is as if the player was
    walking normally along this path.
    */
    private IEnumerator GroundTests()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(0.25f);
        QueueFireball();

        yield return new WaitForSeconds(1f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);

        yield return new WaitForSeconds(1f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);

        yield return new WaitForSeconds(2f);
        Time.timeScale = 0.2f;
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);

        yield return new WaitForSeconds(2.5f);
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
        CancelFireball();
    }   

    private void QueueFireball()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.rightTrigger.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }

    private void CancelFireball()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.rightTrigger.WriteValueIntoEvent(0.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }
}
