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
Tests kinematic setting on interactions to avoid conflicts with char move system during match target.
*/
public class InteractionKinematicUT : MonoBehaviour
{
    public enum TestType { HoldAir, PressAir, PressGround }

    [SerializeField]
    private TestType testType;
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
        
        switch (testType)
        {
            case TestType.HoldAir:
                yield return HoldAirTest();
                break;
            case TestType.PressAir:
                yield return PressAirTest();
                break;
            case TestType.PressGround:
                yield return PressGroundTest();
                break;
        }
        

        Debug.Log("Interaction Kinematic: Success");
    }

    private IEnumerator PressGroundTest()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(1.4f);

        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.5f);

        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();

        yield return new WaitUntil(() => PlayerInfo.AnimationManager.CurrentInteraction != null);
        yield return new WaitUntil(() => PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertInAir();
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);

        yield return new WaitForSeconds(1);
        yield return PlayerUT.AssertInAir();
        yield return PlayerUT.AssertInteracting();

        yield return new WaitForSeconds(2);

        yield return PlayerUT.AssertNotInteracting();
        yield return PlayerUT.AssertGrounded();
    }

    private IEnumerator PressAirTest()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(1f);

        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.5f);

        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();

        yield return new WaitUntil(() => PlayerInfo.AnimationManager.CurrentInteraction != null);
        yield return new WaitUntil(() => PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertInAir();

        yield return new WaitForSeconds(1);
        yield return PlayerUT.AssertInAir();
        yield return new WaitForSeconds(1);
        
        yield return PlayerUT.AssertInteracting();
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);

        yield return new WaitForSeconds(2f);
        yield return PlayerUT.AssertNotInteracting();
        yield return PlayerUT.AssertGrounded();
    }

    private IEnumerator HoldAirTest()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.5f);

        InputEventPtr pressEvent;
        using (StateEvent.From(fakeController, out pressEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(1.0f, pressEvent);
            InputSystem.QueueEvent(pressEvent);
        }
        InputSystem.Update();

        yield return new WaitUntil(() => PlayerInfo.AnimationManager.CurrentInteraction != null);
        yield return new WaitUntil(() => PlayerInfo.AnimationManager.InDirectTargetMatch);
        yield return PlayerUT.AssertInAir();

        yield return new WaitForSeconds(1);
        yield return PlayerUT.AssertInAir();
        yield return new WaitForSeconds(1);
        
        yield return PlayerUT.AssertInteracting();
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);

        yield return new WaitForSeconds(3f);
        yield return PlayerUT.AssertInteracting();
        yield return PlayerUT.AssertGrounded();

        yield return new WaitForSeconds(2f);
        yield return PlayerUT.AssertNotInteracting();
    }
}
