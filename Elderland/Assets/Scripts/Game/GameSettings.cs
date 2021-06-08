using UnityEngine;

//Settings class in which objects are initialized with user settings.

public class GameSettings : MonoBehaviour 
{
    //Input
    //Abilities
    public KeyCode MeleeAbilityKey { get; set; }
    public KeyCode AlternateMeleeAbilityKey { get; set; }
    public KeyCode DodgeAbilityKey { get; set; }
    public KeyCode DashAbilityKey { get; set; }
    public KeyCode BlockAbilityKey { get; set; }
    public KeyCode UtilityAbilityKey { get; set; }
    public KeyCode UltimateAbilityKey { get; set; }
    public KeyCode FinisherAbilityKey { get; private set; }

    public float FireballTrigger { get { return Input.GetAxis("Right Trigger"); } }
    public float FireballTriggerOffThreshold { get { return 0.1f; } }
    public float FireballTriggerOnThreshold { get { return 0.5f; } }

    public int ObjectiveTrigger { get { return (int) Input.GetAxis("Horizontal DPad"); } }

    //Skills
    public KeyCode DodgeKey { get; set; }
    public KeyCode BlinkKey { get; set; }

    //Misc
    public KeyCode JumpKey { get; set; }
    public KeyCode SprintKey { get; set; }
    public KeyCode UseKey { get; set; }

    //Will be assigned later
    public Vector2 LeftDirectionalInput 
    { 
        get 
        { 
            Vector2 v = new Vector2(Input.GetAxis("Left Joystick Horizontal"), Input.GetAxis("Left Joystick Vertical")); 

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
            Vector2 v = new Vector2(Input.GetAxis("Right Joystick Horizontal"), Input.GetAxis("Right Joystick Vertical")); 

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
        //Input
        //Abilities
        MeleeAbilityKey = KeyCode.Joystick1Button2;
        AlternateMeleeAbilityKey = KeyCode.Joystick1Button3;
        DodgeAbilityKey = KeyCode.JoystickButton0;
        DashAbilityKey = KeyCode.Joystick1Button1;
        BlockAbilityKey = KeyCode.Joystick1Button5;
        FinisherAbilityKey = KeyCode.Joystick1Button9;

        //Misc
        JumpKey = KeyCode.Joystick1Button1;
        SprintKey = KeyCode.Joystick1Button8;
        UseKey = KeyCode.Joystick1Button0;
    }
}
