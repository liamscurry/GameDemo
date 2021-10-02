using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

//Settings class in which objects are initialized with user settings.

public class GameSettings : MonoBehaviour 
{
    [SerializeField]
    private Sprite cardinalImage; // image corresponding to generic north, south east and west buttons
    [SerializeField]
    private Sprite dPadImageUp;
    [SerializeField]
    private Sprite dPadImageRight;
    [SerializeField]
    private Sprite dPadImageDown;
    [SerializeField]
    private Sprite dPadImageLeft;

    //Input
    //Abilities
    public GamepadButton MeleeAbilityKey { get; set; }
    public GamepadButton DodgeAbilityKey { get; set; }
    public GamepadButton DashAbilityKey { get; set; }
    public GamepadButton BlockAbilityKey { get; set; }
    public GamepadButton FinisherAbilityKey { get; private set; }
    public GamepadButton AOEAbilityKey { get; private set; }

    public float FireballRightTrigger
    { 
        get { return GameInfo.Settings.CurrentGamepad.rightTrigger.EvaluateMagnitude(); }
    }
    public float FireballLeftTrigger { get { return Input.GetAxis("Left Trigger"); } }
    public float FireballTriggerOffThreshold { get { return 0.1f; } }
    public float FireballTriggerOnThreshold { get { return 0.5f; } }

    public int ObjectiveTrigger { get { return (int) Input.GetAxis("Horizontal DPad"); } }

    //Misc
    public GamepadButton JumpKey { get; set; }
    public GamepadButton SprintKey { get; set; }
    public GamepadButton UseKey { get; set; }
    public GamepadButton BackKey { get; set; }

    public Gamepad CurrentGamepad { get; set; }
    public Gamepad DefaultGamepad { get; set; }

    //Will be assigned later
    public Vector2 LeftDirectionalInput 
    { 
        get 
        { 
            Vector2 v = GameInfo.Settings.CurrentGamepad.leftStick.ReadValue();

            //Unit length restriction
            if (v.magnitude > 1f)
                v = v.normalized;

            //Dead zone restriction
            if (v.magnitude < 0.2f)
                v = Vector2.zero;

            return v;
        } 
    }

    public Vector2 RightDirectionalInput 
    { 
        get 
        { 
            Vector2 v =
                new Vector2(Input.GetAxis("Right Joystick Horizontal"), Input.GetAxis("Right Joystick Vertical")); 

            //Unit length restriction
            if (v.magnitude > 1f)
                v = v.normalized;

            //Dead zone restriction
            if (v.magnitude < 0.2f)
                v = Vector2.zero;

            return v;
        } 
    }

    public event EventHandler<ButtonTypeEventArgs> OnHotKeyChange;

    public void Initialize()
    {
        DefaultGamepad = Gamepad.all[0];
        CurrentGamepad = DefaultGamepad;

        //Input
        //Abilities
        MeleeAbilityKey = GamepadButton.West;
        DodgeAbilityKey = GamepadButton.South;
        DashAbilityKey = GamepadButton.East;
        BlockAbilityKey = GamepadButton.RightShoulder;
        FinisherAbilityKey = GamepadButton.RightStick;
        AOEAbilityKey = GamepadButton.LeftShoulder;

        //Misc
        JumpKey = GamepadButton.North;
        SprintKey = GamepadButton.LeftStick;
        UseKey = GamepadButton.South;
        BackKey = GamepadButton.East;
    }

    /*
    Test method to change use key to another key to get a response from controller button UI images.

    Inputs:
    None

    Outputs:
    None
    */
    public void ChangeKeybindTest()
    {
        BackKey = GamepadButton.DpadRight;
        if (OnHotKeyChange != null)
            OnHotKeyChange.Invoke(this, new ButtonTypeEventArgs(ButtonType.Back));
    }

    /*
    Finds the images and text associated with a button type that is currently bound to a button on the 
    controller

    Inputs:
    ButtonType : buttonType : the class of keybind being considered: ex use, back.

    Outputs:
    (Sprite, string) : sprite and text associated wwith button type currently.
    */
    public (Sprite, string) GetImageButtonPair(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Use:
                return (GetImage(UseKey), GetText(UseKey));
            case ButtonType.Back:
                return (GetImage(BackKey), GetText(BackKey));
            default:
                throw new System.ArgumentException("Button type not defined");
        }
    }

    /*
    Returns an image based on a designated gamepad button class.
    */
    private Sprite GetImage(GamepadButton buttonType)
    {
        if (buttonType == GamepadButton.North ||
            buttonType == GamepadButton.East ||
            buttonType == GamepadButton.South ||
            buttonType == GamepadButton.West)
        {
            return cardinalImage;
        }
        else if (
            buttonType == GamepadButton.DpadUp ||
            buttonType == GamepadButton.DpadRight ||
            buttonType == GamepadButton.DpadDown ||
            buttonType == GamepadButton.DpadLeft)
        {
            switch (buttonType)
            {
                case GamepadButton.DpadUp:
                    return dPadImageUp;
                case GamepadButton.DpadRight:
                    return dPadImageRight;
                case GamepadButton.DpadDown:
                    return dPadImageDown;
                default: // GamepadButton.DpadLeft
                    return dPadImageLeft;
            }
        }
        else
        {
            throw new System.Exception("Gamepad button image not found");
        }
    }

    /*
    Returns a string based on a designated gamepad button class.
    */
    private string GetText(GamepadButton buttonType)
    {
        if (buttonType == GamepadButton.North ||
            buttonType == GamepadButton.East ||
            buttonType == GamepadButton.South ||
            buttonType == GamepadButton.West)
        {
            switch (buttonType)
            {
                case GamepadButton.North:
                    return "Y";
                case GamepadButton.East:
                    return "B";
                case GamepadButton.South:
                    return "A";
                default: // GamepadButton.West
                    return "X";
            }
        }
        else if (
            buttonType == GamepadButton.DpadUp ||
            buttonType == GamepadButton.DpadRight ||
            buttonType == GamepadButton.DpadDown ||
            buttonType == GamepadButton.DpadLeft)
        {
            return "";
        }
        else
        {
            throw new System.Exception("Gamepad button image not found");
        }
    }
}
