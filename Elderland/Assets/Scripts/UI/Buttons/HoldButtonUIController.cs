using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/*
Logic controller for UI buttons that require input to be held down for a specified duration.
If the button is let go early, the percentile timer resets to zero.
The button event logic is only run when the percentile timer reaches 1.0

onHoldComplete should be used for the event called on this button. Do not use onClick from the button component on
this component.
*/
public class HoldButtonUIController : MonoBehaviour
{
    private enum HoldState { NotHeld, Held }

    [Header("Settings")]
    [SerializeField]
    private float holdDuration;
    [SerializeField]
    private UnityEvent onHoldComplete;
    [Header("References")]
    [SerializeField]
    private GameObject slideObject;

    private float timer;
    private Button button;
    private HoldState holdState;

    private const float minHoldPercentage = 0.25f;

    private void Awake()
    {
        button = GetComponent<Button>();
        timer = 0;
        holdState = HoldState.NotHeld;
        ResetSliderScale();
    }

    private void OnEnable()
    {
        timer = 0;
        holdState = HoldState.NotHeld;
        ResetSliderScale();
    }

    // Currently zeroes on deselect and button release as intended. 9.25.21.
    private void Update()
    {
        if (holdState == HoldState.NotHeld)
        {
            // Entry case
            if (EventSystem.current.currentSelectedGameObject == gameObject &&
                Gamepad.all[0].buttonSouth.wasPressedThisFrame)
            {
                timer = 0;
                holdState = HoldState.Held;
            }
        }
        else
        {
            UpdatePressedButton();
        }
    }

    /*
    Updates the button timer as long as the button is pressed. Needs to be in pressed state enum, or
    else this method is not called

    Input:
    None

    Output:
    None
    */
    private void UpdatePressedButton()
    {
        // Exit cases 
        if (EventSystem.current.currentSelectedGameObject != gameObject ||
            (!Gamepad.all[0].buttonSouth.isPressed && Mathf.Clamp01(timer / holdDuration) > minHoldPercentage))
        {
            holdState = HoldState.NotHeld;
            ResetSliderScale();
            return;
        }

        timer += Time.unscaledDeltaTime;
        UpdateSliderScale();
        if (timer >= holdDuration)
        {
            if (onHoldComplete != null)
                onHoldComplete.Invoke();
            holdState = HoldState.NotHeld;
            ResetSliderScale();
            return;
        }
    }

    /*
    Updates the slider scale based on the timer's duration percentage, clamping at duration max.

    Inputs:
    None

    Outputs:
    None
    */
    private void UpdateSliderScale()
    {
        slideObject.transform.parent.localScale =
             new Vector3(
                Mathf.Clamp01(timer / holdDuration),
                slideObject.transform.parent.localScale.y,
                slideObject.transform.parent.localScale.z);
    }

    /*
    Resets the slider indicating the hold percentage to zero.

    Inputs:
    None

    Outputs:
    None
    */
    private void ResetSliderScale()
    {
        slideObject.transform.parent.localScale =
             new Vector3(
                0,
                slideObject.transform.parent.localScale.y,
                slideObject.transform.parent.localScale.z);
    }
}
