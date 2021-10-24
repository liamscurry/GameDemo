using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/*
Logic controller for UI buttons. This script is solely to have the selected indicator appear when
the button is selected.
*/
public class ButtonUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject selectedImage;

    private void Update()
    {
        UpdateSelectedImage();
    }

    /*
    Turns the selected outline on (and off) when the button is selected/not selected.

    Inputs:
    None

    Outputs:
    None
    */
    private void UpdateSelectedImage()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            if (!selectedImage.activeInHierarchy)
            {
                selectedImage.gameObject.SetActive(true);
            }
        }
        else
        {
            if (selectedImage.activeInHierarchy)   
            {
                selectedImage.gameObject.SetActive(false);
            }
        }
    }
}
