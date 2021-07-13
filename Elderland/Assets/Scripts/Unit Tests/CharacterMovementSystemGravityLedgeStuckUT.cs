using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Input based UT
// Before fix: character walks off of ledge and lands on too steep a wall, begins to walk up it.
// Behaviour after fix: lands on wall and falls down it.

// Currently falls down slope now that both air partitions are handled on collisions, which is the
// behaviour we want.
// Remaining problem is that when it lands on the ground after sliding, sometimes it begins climbing
// the slope again. When this happens, grounded is still true. Velocity still seems good <0, 0, 5>
// Curiously, when this is occuring, if I pause the game and unpause it the character stops. <- Turns
// out this is just the game zeroing input on pause. The bug still happens (slide up) when I haven't modified
// the slope limit from the initial 45 degrees. Could the issue be caused from the fact that the normal still reads as up
// and so the velocity input is still non zero? Thought the character controller would prohibit this movement.

// Not caused alone by:
// - ground clamp.

// Can mimic behaviour with a default character controller running at a slope limit higher then it should
// be able to climb, but still does.
// Although an old thread, there are reports of this being broken: https://forum.unity.com/threads/character-controller-and-slope-limit.36778/
// Will try and limit movement on slope using limiting on constant velocity based on slope and ground info.
public class CharacterMovementSystemGravityLedgeStuckUT : MonoBehaviour
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
