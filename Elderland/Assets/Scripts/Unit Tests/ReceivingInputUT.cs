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
Three types of tests:
1: Test that each input type cannot be used when used by another instance of input (ex ability can't
   be used when falling).
2: Override gameplay inputs are properly overriden.
3: Unoverride gameplay inputs cannot be overriden by none input events.
*/

// Concerns:
/*
    Abilities
    Takeout/put away sword
    Interactions
    Falling/Jump
    Camera Cutscenes
    Gameplay Cutscenes
*/

public class ReceivingInputUT : MonoBehaviour
{
    public enum ReceivingInputTestType { Uniqueness, Override, Nonoverride }

    [SerializeField]
    private ReceivingInputTestType testType;
    [SerializeField]
    private GameplayCutsceneEvent gameplayCutscene;
    [SerializeField]
    private CameraCutsceneEvent cameraCutscene;
    [SerializeField]
    private StandardInteraction interactionUniquenessObject1;
    [SerializeField]
    private StandardInteraction interactionUniquenessObject2;

    private XInputController fakeController;

    private const float overrideTakeOutSwordThreshold = 0.25f;
    
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

        // Wait until scene is fully loaded to test properly (inputs occur when they are supposed to)
        yield return new WaitForSeconds(1f);

        switch (testType)
        {
            case ReceivingInputTestType.Uniqueness:
                yield return UniquenessTest();
                break;
            case ReceivingInputTestType.Override:
                yield return OverrideTest();
                break;
                /*
            case ReceivingInputTestType.Uniqueness:
                yield return UniquenessTest();
                break;*/
        }
        
        
        Debug.Log("Receiving Input: Success");
    }

    // Tests:
    /*
    Abilities
    Take out/put away sword
    */
    private IEnumerator OverrideTest()
    {
        QueueSword();

        yield return new WaitUntil(() => PlayerInfo.AbilityManager.CurrentAbility != null);

        cameraCutscene.Invoke();

        try
        {
            UT.CheckEquality<bool>(
                PlayerInfo.AbilityManager.CurrentAbility == null,
                true);  
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        DisableSword();

        // Take out/put away disable
        Time.timeScale = 12f;
        yield return new WaitForSeconds(12f);
        Time.timeScale = 1;

        QueueSword();

        yield return new WaitUntil(
            () => PlayerInfo.AnimationManager.UpperLayer.GetLayerWeight > overrideTakeOutSwordThreshold);

        cameraCutscene.Invoke(); 

        try
        {
            // not instantanious. override for upper actions happens when new actions occur.
            // since new actions have not occured, still not in combat stance yet.
            /*
            UT.CheckEquality<bool>(
                PlayerInfo.AbilityManager.InCombatStance,
                true);  
                */
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker != PlayerInfo.AnimationManager,
                true);  
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        DisableSword();

        Time.timeScale = 9f;
        yield return new WaitForSeconds(9f);
        Time.timeScale = 1;

        yield return new WaitUntil(
            () => PlayerInfo.AnimationManager.UpperLayer.GetLayerWeight > overrideTakeOutSwordThreshold);

        cameraCutscene.Invoke(); 

        try
        {
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker != PlayerInfo.AnimationManager,
                true);  
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }
    }

    // Tests:
    /*
    Abilities
    Interactions
    Falling
    Jump
    */
    private IEnumerator UniquenessTest()
    {
        yield return AbilityTest();
        yield return InteractionTest();
        yield return FallingTest();
        yield return JumpTest();
    }

    // Right now only tests falling to the extent that the animator doesn't go into the falling state.
    // Ability specific tests will be made to ensure abilities can be used and transition safely if ungrounded
    // during the duration.
    private IEnumerator AbilityTest()
    {
        // Move forward
        SetFakeControllerDirection(new Vector2(0, 1).normalized * 0.95f);
        yield return new WaitForSeconds(0.5f);
        SetFakeControllerDirection(new Vector2(0, 0));

        // Take out sword
        QueueSword();

        yield return new WaitUntil(() => PlayerInfo.AbilityManager.CurrentAbility != null);

        // Tests during ability
        TurnOnAllUnique();

        // Wait for next frame in order for interaction to be considered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker == PlayerInfo.AbilityManager.Melee,
                true);  
            UT.CheckEquality<bool>(GameInfo.Manager.ReceivingInput.Value == GameInput.GameplayOverride, true); 
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        TurnOffAllUnique();

        yield return new WaitForSeconds(2f);
    }

    // Will add more strenious test for falling when interacting after adding kinematic mode for
    // char move system.
    private IEnumerator InteractionTest()
    {
        SetFakeControllerDirection(new Vector2(-1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(1f);
        SetFakeControllerDirection(new Vector2(0, 1));
        yield return new WaitForSeconds(1f);

        QueueInteraction();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Tests during ability
        TurnOnAllUnique();

        // Wait for next frame in order for interaction to be considered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker == interactionUniquenessObject1 ^
                GameInfo.Manager.ReceivingInput.Tracker == interactionUniquenessObject2,
                true);  
            UT.CheckEquality<bool>(GameInfo.Manager.ReceivingInput.Value == GameInput.None, true); 
            UT.CheckEquality<bool>(
                interactionUniquenessObject1.Activated ^ interactionUniquenessObject2.Activated,
                true); 
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        TurnOffAllUnique();

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator FallingTest()
    {
        SetFakeControllerDirection(new Vector2(-1, 0).normalized * 0.95f);
        yield return new WaitForSeconds(2f);
        SetFakeControllerDirection(new Vector2(0, 1));
        yield return new WaitForSeconds(2f);

        yield return new WaitUntil(() => !PlayerInfo.CharMoveSystem.Grounded);
        
        // Tests during fall
        TurnOnAllUnique();

        // Wait for next frame in order for interaction to be considered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker == PlayerInfo.MovementManager,
                true);  
            UT.CheckEquality<bool>(GameInfo.Manager.ReceivingInput.Value == GameInput.GameplayUnoverride, true); 
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        TurnOffAllUnique();
    }

    private IEnumerator JumpTest()
    {
        SetFakeControllerDirection(new Vector2(0, 1));
        yield return new WaitForSeconds(2f);

        QueueJump();

        yield return new WaitUntil(() => PlayerInfo.MovementManager.Jumping);
        
        // Tests during fall
        TurnOnAllUnique();

        // Wait for next frame in order for interaction to be considered
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            UT.CheckEquality<bool>(
                GameInfo.Manager.ReceivingInput.Tracker == PlayerInfo.MovementManager,
                true);  
            UT.CheckEquality<bool>(GameInfo.Manager.ReceivingInput.Value == GameInput.GameplayUnoverride, true); 
        }
        catch (Exception e)
        {
            Debug.Log("Receiving Input: Failed. " + e.Message + " " + e.StackTrace);
            yield break;
        }

        TurnOffAllUnique();
    }

    // Helper input functions. Manual update needed for chained events to be considered. //
    private void TurnOnAllUnique()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(1f, inputEvent);
            fakeController.leftShoulder.WriteValueIntoEvent(1f, inputEvent);
            fakeController.buttonNorth.WriteValueIntoEvent(1f, inputEvent);
            fakeController.buttonSouth.WriteValueIntoEvent(1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void TurnOffAllUnique()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(0f, inputEvent);
            fakeController.leftShoulder.WriteValueIntoEvent(0f, inputEvent);
            fakeController.buttonNorth.WriteValueIntoEvent(0f, inputEvent);
            fakeController.buttonSouth.WriteValueIntoEvent(0f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void QueueSword()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void DisableSword()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonWest.WriteValueIntoEvent(0f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void QueueAbility()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.leftShoulder.WriteValueIntoEvent(1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void DisableAbility()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.leftShoulder.WriteValueIntoEvent(0f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void QueueJump()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonNorth.WriteValueIntoEvent(1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void DisableJump()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonNorth.WriteValueIntoEvent(0f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void QueueInteraction()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
    }

    private void DisableInteraction()
    {
        InputEventPtr inputEvent;
        using (StateEvent.From(fakeController, out inputEvent))
        {
            fakeController.buttonSouth.WriteValueIntoEvent(-1f, inputEvent);
            InputSystem.QueueEvent(inputEvent);
        }
        InputSystem.Update();
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
