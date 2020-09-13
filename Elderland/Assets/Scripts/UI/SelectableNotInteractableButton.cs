using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableNotInteractableButton : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private UnityEvent onValidClick;
    [SerializeField]
    private GameObject selectedTeleporter;
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

    public void OnSelect(BaseEventData eventData)
    {
        selectedTeleporter.transform.position = transform.position;
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
                interactableColorBlock.disabledColor; //DimColor(interactableColorBlock.normalColor)
            nonInteractableColorBlock.pressedColor =
                interactableColorBlock.disabledColor;
            nonInteractableColorBlock.selectedColor =
                interactableColorBlock.disabledColor;
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
        average *= 0.15f;
        return new Color(average, average, average, input.a * .15f);
    }

    private Color DimColor(Color input)
    {
        return 
            new Color(
                input.r * 0.15f,
                input.g * 0.15f,
                input.b * 0.15f,
                input.a);
    }
}
