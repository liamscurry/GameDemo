using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

//Settings class in which objects are initialized with user settings.

public class GameSettings : MonoBehaviour 
{
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
    }
}
