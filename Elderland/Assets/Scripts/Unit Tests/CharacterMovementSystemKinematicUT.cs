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
Tests kinematic mode transitions for character move system.
*/

public class CharacterMovementSystemKinematicUT : MonoBehaviour
{
    XInputController fakeController;
    
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
        
        yield return StillTest();
        yield return KinematicStationaryFallTest();
        yield return KinematicGlideFallTest();
        yield return KinematicLedgeFallTest();

        Debug.Log("Char Move System Kinematic: Success");
    }

    private IEnumerator StillTest()
    {
        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(1f);

        yield return PlayerUT.AssertGrounded();

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        SetFakeControllerDirection(new Vector2(0, 0));

        yield return PlayerUT.AssertInAir();
        yield return new WaitForSeconds(3f);
        yield return PlayerUT.AssertInAir();

        PlayerInfo.CharMoveSystem.Kinematic.TryReleaseLock(this, false);
        yield return PlayerUT.AssertGrounded();
        yield return AssertNotMoving();

        yield return new WaitForSeconds(1f);
        yield return PlayerUT.AssertGrounded();
    }

    private IEnumerator KinematicStationaryFallTest()
    {
        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.3f);

        var matchTarget = new PlayerAnimationManager.MatchTarget(
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 5f,
            Quaternion.identity,
            AvatarTarget.Root,
            Vector3.one,
            0,
            0,
            0.5f    
        );
        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        yield return PlayerUT.AssertInAir();
        PlayerInfo.AnimationManager.StartDirectTarget(matchTarget, false);

        yield return new WaitUntil(() => !PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertInAir();

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, false);
        yield return PlayerUT.AssertInAir();
        yield return AssertNotMoving();

        yield return new WaitForSeconds(1);
        yield return PlayerUT.AssertGrounded();
    }

    private IEnumerator KinematicGlideFallTest()
    {
        yield return new WaitForSeconds(1.3f);

        var matchTarget = new PlayerAnimationManager.MatchTarget(
            PlayerInfo.Player.transform.position + PlayerInfo.Player.transform.forward * 6,
            Quaternion.identity,
            AvatarTarget.Root,
            Vector3.one,
            0,
            0,
            0.5f    
        );
        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        yield return PlayerUT.AssertInAir();
        PlayerInfo.AnimationManager.StartDirectTarget(matchTarget, true);

        yield return new WaitUntil(() => !PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertGrounded(); // lock released in direct target.
        yield return AssertNotMoving();

        yield return new WaitForSeconds(1);
        yield return PlayerUT.AssertGrounded();
    }

    private IEnumerator KinematicLedgeFallTest()
    {
        yield return new WaitForSeconds(1.5f);

        Vector3 targetPos =
            PlayerInfo.Player.transform.position +
            PlayerInfo.Player.transform.forward * 2.55f +
            PlayerInfo.Player.transform.up * -1f;
        var matchTarget = new PlayerAnimationManager.MatchTarget(
            targetPos,
            Quaternion.identity,
            AvatarTarget.Root,
            Vector3.one,
            0,
            0,
            0.5f    
        );

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, true);
        yield return PlayerUT.AssertInAir();
        PlayerInfo.AnimationManager.StartDirectTarget(matchTarget, false);
        SetFakeControllerDirection(new Vector2(-1, 1).normalized * 0.95f);

        yield return new WaitUntil(() => !PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertInAir();

        PlayerInfo.CharMoveSystem.Kinematic.ClaimLock(this, false);
        yield return PlayerUT.AssertInAir();
        yield return AssertNotMoving();

        yield return new WaitForSeconds(2);
        yield return PlayerUT.AssertGrounded();
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
