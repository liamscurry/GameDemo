using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Input based UT
/*
Ability test with new physics system integration (CharacterMovementSystem)
Tested by inspection.
*/
public class PlayerSwordPhysicsOverhaulUT : MonoBehaviour
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
        
        yield return SwordTest();
        
        Debug.Log("Player Sword: Success");
    }

    private IEnumerator SwordTest()
    {
        QueueSword();

        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(1f);
        CancelSword();

        yield return new WaitForSeconds(2f);

        QueueSword();
        Time.timeScale = 0.1f; // goes up even when match target is set to zero weight. added in direct target,
        // added for some reason, will have to see why.

        // clamps correctly now when wanting to clamp, need to add cases where it should not clamp.
        // gameplay cutscene works as well.

        yield return new WaitForSeconds(0.5f);
        CancelSword();
        
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
    }

    private void QueueSword()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }

    private void CancelSword()
    {
        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(0.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();
    }
}
