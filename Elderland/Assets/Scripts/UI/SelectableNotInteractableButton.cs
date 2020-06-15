using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableNotInteractableButton : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onValidClick;
    [SerializeField]
    private bool interactable;

    private Button button;
    private ColorBlock interactableColorBlock;
    private ColorBlock nonInteractableColorBlock;

    private bool initialized;

    private void Awake()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (!initialized)
        {
            initialized = true;
            Initialize();
        }
    }

    private void Initialize()
    {
        button = GetComponent<Button>();
        interactableColorBlock = button.colors;
        if (!interactable)
        {
            Disable(true);
        }
    }

    public void Disable(bool forceDisable = false)
    {
        TryInitialize();
        if (interactable || forceDisable)
        {
            interactable = false;
            nonInteractableColorBlock.normalColor =
                NeutralizeColor(interactableColorBlock.normalColor);
            nonInteractableColorBlock.pressedColor =
                NeutralizeColor(interactableColorBlock.pressedColor);
            nonInteractableColorBlock.selectedColor =
                NeutralizeColor(interactableColorBlock.selectedColor);
            nonInteractableColorBlock.disabledColor =
                interactableColorBlock.disabledColor;
            nonInteractableColorBlock.colorMultiplier =
                interactableColorBlock.colorMultiplier;
            nonInteractableColorBlock.fadeDuration =
                interactableColorBlock.fadeDuration;
            button.colors = nonInteractableColorBlock;
        }
    }

    public void Enable()
    {
        TryInitialize();
        if (!interactable)
        {
            interactable = true;
            button.colors = interactableColorBlock;
        }
    }

    public void Invoke()
    {
        if (interactable)
        {
            if (onValidClick != null)
                onValidClick.Invoke();
        }
    }

    private Color NeutralizeColor(Color input)
    {
        float average = (input.r + input.g + input.b) / 3f;
        return new Color(average, average, average, input.a);
    }
}
