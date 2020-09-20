using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderMaterialUIController : MonoBehaviour
{
    private Image image;
    private bool initialized;

    private void Awake()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (!initialized)
        {
            image = GetComponent<Image>();
            image.material = new Material(image.material); // Allows individual instances of sliders to have their own value.
            image.material.SetFloat("_HorizontalOnPercentage", 1);
            initialized = true;
        }
    }

    public void UpdateMaterialSliderFloat(Slider slider)
    {
        TryInitialize();
        image.material.SetFloat("_HorizontalOnPercentage", slider.value);
    }
}
