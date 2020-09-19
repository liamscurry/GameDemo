using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderMaterialUIController : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.material = new Material(image.material); // Allows individual instances of sliders to have their own value.
        image.material.SetFloat("_HorizontalOnPercentage", 1);
    }

    public void UpdateMaterialSliderFloat(Slider slider)
    {
        image.material.SetFloat("_HorizontalOnPercentage", slider.value);
        Debug.Log(slider.value);
    }
}
