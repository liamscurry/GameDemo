using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/*
Image controller that sets its image according to the current keybind specified (must fit dropdown
paratype.) Automatically updates image when a keybind is changed.
*/
public class ControllerButtonImageUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private ButtonType buttonType;
    [Header("References")]
    [SerializeField]
    private Text buttonTypeText;

    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();

        var initArgs = new ButtonTypeEventArgs();
        initArgs.TypeOfButton = buttonType;
        UpdateImage(this, initArgs);

        GameInfo.Settings.OnHotKeyChange += UpdateImage;
    }

    /*
    Updates the image to the correct keybinding-image pair.

    Inputs:
    None

    Outputs:
    None
    */
    private void UpdateImage(object sender, ButtonTypeEventArgs args)
    {
        if (buttonType == args.TypeOfButton)
        {
            string foundText;
            Sprite foundSprite;
            (foundSprite, foundText) = GameInfo.Settings.GetImageButtonPair(buttonType);
            image.sprite = foundSprite;
            buttonTypeText.text = foundText;
        }
    }
}
