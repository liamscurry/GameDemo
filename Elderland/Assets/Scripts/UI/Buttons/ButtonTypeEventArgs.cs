using System;

// Event args used when keybinds are changed in GameSettings.
public class ButtonTypeEventArgs : EventArgs
{
    public ButtonType TypeOfButton { get; set; }

    public ButtonTypeEventArgs(){}

    public ButtonTypeEventArgs(ButtonType type)
    {
        TypeOfButton = type;
    }
}