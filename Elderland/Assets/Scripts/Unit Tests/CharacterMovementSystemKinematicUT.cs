using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Input based UT

public class CharacterMovementSystemKinematicUT : MonoBehaviour
{
    XInputController fakeController;
    
    private void Start()
    {
        fakeController = InputSystem.AddDevice<XInputController>();
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        StartCoroutine(InputCoroutine());
    }

    private IEnumerator InputCoroutine()
    {
        GameInfo.Settings.CurrentGamepad = fakeController;

        while (!PlayerInfo.CharMoveSystem.Grounded)
        {
            yield return new WaitForEndOfFrame();
        }

        /*
        InputEventPtr sprintEvent;
        using (StateEvent.From(fakeController, out sprintEvent))
        {
            fakeController.leftStickButton.pressPoint = 0.0f;
            fakeController.leftStickButton.WriteValueIntoEvent(1.0f, sprintEvent);
            InputSystem.QueueEvent(sprintEvent);
        }

        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        */
        
        yield return StillTest();
        yield return KinematicStationaryFallTest();

        Debug.Log("Char Move System Kinematic: Success");
    }

    private IEnumerator StillTest()
    {
        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(1f);

        yield return AssertGrounded();

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        SetFakeControllerDirection(new Vector2(0, 0));

        yield return AssertInAir();
        yield return new WaitForSeconds(3f);
        yield return AssertInAir();

        PlayerInfo.CharMoveSystem.Kinematic.TryReleaseLock(this, false);

        yield return AssertInAir();
        yield return AssertNotMoving();
        yield return new WaitForSeconds(1f);
        yield return AssertGrounded();
    }

    private IEnumerator KinematicStationaryFallTest()
    {
        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.3f);

        var matchTarget = new PlayerAnimationManager.MatchTarget(
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 4,
            Quaternion.identity,
            AvatarTarget.Root,
            Vector3.one,
            0,
            0,
            0.5f    
        );
        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        yield return AssertInAir();
        PlayerInfo.AnimationManager.StartDirectTarget(matchTarget);

        yield return new WaitUntil(() => !PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return AssertInAir();

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, false);
        yield return AssertInAir();
        yield return AssertNotMoving();

        yield return new WaitForSeconds(1);
        yield return AssertGrounded();
    }

    private IEnumerator AssertNotMoving()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.MovementManager.CurrentPercentileSpeed < UT.Threshold, true);
        }
        catch (Exception e)
        {
            Debug.Log("Char Move System Kinematic: Failed. Not on ground " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }


    private IEnumerator AssertGrounded()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Grounded, true);
        }
        catch (Exception e)
        {
            Debug.Log("Char Move System Kinematic: Failed. Not on ground " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    private IEnumerator AssertInAir()
    {
        try
        {
            UT.CheckEquality<bool>(PlayerInfo.CharMoveSystem.Grounded, false);
        }
        catch (Exception e)
        {
            Debug.Log("Char Move System Kinematic: Failed. Not in air " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    private void SetFakeControllerDirection(Vector2 direction)
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
