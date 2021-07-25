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
public class PlayerSwordPhysicsOverhaulUT : MonoBehaviour
{
    public enum TestType { NoTarget, CloseTarget, FarTarget }

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
            case TestType.NoTarget:
                yield return NoTargetTest();
                break;
            case TestType.CloseTarget:
                yield return CloseTargetTest();
                break;
            case TestType.FarTarget:
                yield return FarTargetTest();
                break;
        }
    }

    /*
    Expected Behaviour:
    Walks right, swings forward and ends up next to target. Walks right then forward a bit,
    swings and goes almost horizontally in the air, falls and keeps swinging while below target
    which is on the ledge.
    */
    private IEnumerator FarTargetTest()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(2f);
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);
        QueueSword();
        yield return new WaitForSeconds(1.0f);
        CancelSword();

        yield return new WaitForSeconds(2f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(2f);
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.65f);
        QueueSword();
    }   

    /*
    Expected Behaviour:
    Player walks off ledge to the left, swings on ground smoothly (forward) and then swings off ledge,
    falling as if walking off ledge. After landing the player swings again and goes horizontally across narrow gap.
    During this gap, it lands on the corner of the new geometry and thus goes down and up ever so slightly.
    Finally the player swings and goes up a small ledge. In addition it then swings, gets pushed to the right
    slightly and keeps turning towards its target, ending facing the -X direction, then goes into idle state
    facing +Z.
    */
    private IEnumerator NoTargetTest()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(-1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(2f);
        PlayerUT.SetFakeControllerDirection(fakeController, Vector2.zero);

        QueueSword();
        yield return new WaitForSeconds(2.0f);
        CancelSword();

        while (!PlayerInfo.CharMoveSystem.Grounded)
        {
            yield return new WaitForEndOfFrame();
        }

        QueueSword();
        yield return new WaitForSeconds(0.5f);
        CancelSword();

        yield return new WaitForSeconds(0.5f);
        QueueSword();
        yield return new WaitForSeconds(0.5f);
        CancelSword();

        yield return new WaitForSeconds(0.5f);
        QueueSword();
        yield return new WaitForSeconds(0.1f);
        PlayerInfo.CharMoveSystem.Push(new Vector3(8, 0, 0));
        yield return new WaitForSeconds(0.5f);
        CancelSword();
    }

    /*
    Expected Behaviour:
    Gets pushed to the left, starts to swing and turn toward target while moving, keeps hitting the target
    a few times after stopping. Then after this, the player begins to swing and then gets pushed off ledge.
    This transition should be smooth then it should stop attacking.
    */
    private IEnumerator CloseTargetTest()
    {
        QueueSword();

        Time.timeScale = 0.4f;

        yield return new WaitForSeconds(0.65f);
        PlayerInfo.CharMoveSystem.Push(new Vector3(-5, 0, 0));

        yield return new WaitForSeconds(2.3f);

        PlayerInfo.CharMoveSystem.Push(new Vector3(-1f, 0, 1).normalized * 3);

        Time.timeScale = 1f;
        yield return new WaitForSeconds(0.5f);
        CancelSword();
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
