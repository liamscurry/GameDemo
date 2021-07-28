using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

/*
Helpful UT methods aimed specificly at asserting player states. Addon to general UT class.

Input tests need to not have any background inputs in the editor while they are running, as 
this often causes abnormalities in tests as inputs are not sequenced correctly in the fake controller.
This was tested even with different frame rates, and the tests still run the same as long as
now outside editor input occurs during the tests as mentioned above. 7.24.21
*/
public static class PlayerUT
{
    public static IEnumerator AssertInteracting()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.AnimationManager.CurrentInteraction != null, true);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not interacting " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertNotInteracting()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.AnimationManager.CurrentInteraction == null, true);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Interacting " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertNotMoving()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.MovementManager.CurrentPercentileSpeed < UT.Threshold, true);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not on ground " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertGrounded()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Grounded, true);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not on ground " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertInAir()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Grounded, false);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not in air " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertKinematic()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Kinematic.Value, true);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not in kinematic mode " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static IEnumerator AssertNotKinematic()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Kinematic.Value, false);
        }
        catch (Exception e)
        {
            Debug.Log("Test Failed: Not in kinematic mode " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    public static void SetFakeControllerDirection(XInputController fakeController, Vector2 direction)
    {
        InputEventPtr walkEvent;
        using (StateEvent.From(fakeController, out walkEvent))
        {
            fakeController.leftStick.WriteValueIntoEvent(direction, walkEvent);
            InputSystem.QueueEvent(walkEvent);
        }
        InputSystem.Update();
    }
}