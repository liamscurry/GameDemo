using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerChannelRenderer : MonoBehaviour
{
    [SerializeField]
    private float duration;
    [SerializeField]
    private Color onColor;
    [SerializeField]
    private Color offColor;

    private void Awake()
    {
        MeshRenderer renderer =
            GetComponentInChildren<MeshRenderer>();
        renderer.sharedMaterial.color = offColor;
    }

    public void TurnOn()
    {
        StartCoroutine(TurnOnCoroutine());
    }

    private IEnumerator TurnOnCoroutine()
    {
        MeshRenderer[] renderers =
            GetComponentsInChildren<MeshRenderer>();

        float timer = 0;
        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            SetMaterialsColors(
                renderers,
                LerpColor(offColor, onColor, timer / duration));
        }

        SetMaterialsColors(
            renderers,
            LerpColor(offColor, onColor, 1f));
    }

    private void SetMaterialsColors(MeshRenderer[] renderers, Color color)
    {
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    private Color LerpColor(Color start, Color end, float percentage)
    {
        return start * (1 - percentage) + end * percentage;
    }
}
