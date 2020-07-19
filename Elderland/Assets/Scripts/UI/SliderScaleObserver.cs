using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScaleObserver : MonoBehaviour
{
    [SerializeField]
    private Slider mimicSlider;
    [SerializeField]
    private float maxScale;

    private ParticleSystem particles;

    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!initialized)
        {
            particles = GetComponent<ParticleSystem>();
            initialized = true;
        }
    }

    public void MimicSlider()
    {
        Initialize();
        particles.transform.localScale = 
            new Vector3(mimicSlider.value * maxScale, mimicSlider.value * maxScale, mimicSlider.value * maxScale);
    }
}
