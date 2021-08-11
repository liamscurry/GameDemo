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
Ability test with new physics system integration (CharacterMovementSystem). Also tests to make sure
player is invulnerable during cutscene.
Tested by inspection.
*/
public class CameraCutsceneKinematicUT : MonoBehaviour
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
        
        yield return Tests();

        Debug.Log("Camera Cutscene Kinematic: Success");
    }

    private IEnumerator Tests()
    {
        PlayerUT.SetFakeControllerDirection(fakeController, new Vector2(0, 1).normalized * 0.95f);
        PlayerUT.AssertNotKinematic();
        PlayerUT.AssertVulnerable();
        yield return new WaitUntil(() => GameInfo.CameraController.CameraState == CameraController.State.Cutscene);
        PlayerUT.AssertKinematic();
        PlayerUT.AssertInvulnerable();
        yield return new WaitUntil(() => GameInfo.CameraController.CameraState == CameraController.State.Gameplay);
        PlayerUT.AssertNotKinematic();
        PlayerUT.AssertVulnerable();
    }   
}
