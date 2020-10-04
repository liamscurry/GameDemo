using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialAnimationUI : MonoBehaviour
{
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void UpdateRadialToCurrentInteraction()
    {
        slider.value = PlayerInfo.Sensor.Interaction.HoldNormalizedTime;
    }

    public void ResetRadialToCurrentInteraction()
    {
        slider.value = 0;
    }
}
