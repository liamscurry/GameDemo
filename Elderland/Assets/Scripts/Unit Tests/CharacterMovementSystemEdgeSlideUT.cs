using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Input based UT
// Before fix: character goes on edge up ramp, falls and then enters the ground, falls again and slides
// up side of the ramp.
// After fix: character goes up ramps and falls off to the right. May not fall off ledge on some runs,
// in which case it will run left.

// Not sure as of right now, as this is the first input based UT, but writing a normalized vector2
// to the left stick wraps around to the other side (negative). Has to do with clamping of AxisControl.
// Not sure why this is not set-able from the left stick field.

// Unit test class based on fake player input. Needed to debug rare bug when walking up sloped ledges
// in new Character Movement System class.

// bug was reproduced by hand by going up ramp from a bit on ramp (fully on it).
// after going up about 1/4, 1/5 of ramp when near edge and then moved left stick towards wall as I began
// nearing the edge and so the player bounced off of wall and then towards wall and then slide upwards.

// Test produces bug now. This does not happen every run, but it does get produced (~50 percent of the time)
// Does this mean frame dependence is causing bug?
// Changed system to used built in grounded field of controller and checks ledges for sharp tall geometry,
// in which case the player starts falling.
public class CharacterMovementSystemEdgeSlideUT : MonoBehaviour
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

        SetFakeControllerDirection(new Vector2(0f, 1).normalized * 0.95f);

        yield return new WaitForSeconds(0.7f);

        SetFakeControllerDirection(new Vector2(0.2f, 1).normalized * 0.95f);

        yield return new WaitForSeconds(0.35f);

        SetFakeControllerDirection(new Vector2(-.34f, 0.5f).normalized * 0.95f);
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
