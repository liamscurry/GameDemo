using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingObject : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration;

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutMaterialCoroutine());
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInMaterialCoroutine());
    }

    private IEnumerator FadeOutMaterialCoroutine()
    {
        Material fadeMaterial =
            GetComponent<MeshRenderer>().material;
        MeshRenderer fadeRenderer = 
            GetComponent<MeshRenderer>();
        float timer = 0;
        while (timer < fadeDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            fadeMaterial.SetFloat("_Threshold", timer / fadeDuration);
            fadeRenderer.material = fadeMaterial;
        }
        fadeMaterial.SetFloat("_Threshold", 1);
        fadeRenderer.material = fadeMaterial;
    }

    private IEnumerator FadeInMaterialCoroutine()
    {
        MeshRenderer[] fadeRenderers =
            GetComponentsInChildren<MeshRenderer>();
        //MeshRenderer fadeRenderer = 
        //    GetComponent<MeshRenderer>();
        float timer = 0;
        while (timer < fadeDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            foreach (MeshRenderer renderer in fadeRenderers)
            {
                renderer.material.SetFloat("_Threshold", 1 - timer / fadeDuration);
            }
        }

        foreach (MeshRenderer renderer in fadeRenderers)
        {
            renderer.material.SetFloat("_Threshold", 0);
        }
    }
}
