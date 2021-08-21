using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Input based UT
/*
Before fix: When playing the scene, the player will move forward and attack two enemies, then it will
move to the left and only one enemy will follow. The second enemy will finish its attack and go to group
movement state. There it will stay and do nothing, not facing the player.
*/

public class MeleeGroupMovementStandstillBugUT : MonoBehaviour
{
    XInputController fakeController;
    
    private void Start()
    {
        fakeController = InputSystem.AddDevice<XInputController>();
        StartCoroutine(InputCoroutine());
    }

    private IEnumerator InputCoroutine()
    {
        GameInfo.Settings.CurrentGamepad = fakeController;

        while (!PlayerInfo.CharMoveSystem.Grounded)
        {
            yield return new WaitForEndOfFrame();
        }

        InputEventPtr sprintEvent;
        using (StateEvent.From(fakeController, out sprintEvent))
        {
            fakeController.leftStickButton.pressPoint = 0.0f;
            fakeController.leftStickButton.WriteValueIntoEvent(1.0f, sprintEvent);
            InputSystem.QueueEvent(sprintEvent);
        }

        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);

        yield return new WaitForSeconds(2.5f);
        SetFakeControllerDirection(Vector2.zero);

        yield return new WaitForSeconds(2f);
        PlayerUT.QueueAttack(fakeController);

        yield return new WaitForSeconds(0.5f);

        PlayerUT.CancelAttack(fakeController);
        SetFakeControllerDirection(new Vector2(-1, 0).normalized * 0.95f);

        yield return new WaitForSeconds(0.5f);
        PlayerUT.QueueDodge(fakeController);
        yield return new WaitForSeconds(1f);

        SetFakeControllerDirection(Vector2.zero);
    }

    private void SetFakeControllerDirection(Vector2 direction)
    {
        InputEventPtr walkEvent;
        using (StateEvent.From(fakeController, out walkEvent))
        {
            fakeController.leftStick.WriteValueIntoEvent(direction, walkEvent);
            InputSystem.QueueEvent(walkEvent);
        }
    }
}
