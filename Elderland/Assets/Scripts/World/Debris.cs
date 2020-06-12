using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    [SerializeField]
    private float destroyDuration;
    [SerializeField]
    private float fadeDuration;
    [SerializeField]
    private GameObject fadeObject;

    public void Break()
    {
        StartCoroutine(DestroyAfterBreakCoroutine());
        StartCoroutine(FadeMaterialCoroutine());
    }

    private IEnumerator DestroyAfterBreakCoroutine()
    {
        yield return new WaitForSeconds(destroyDuration);
        GameObject.Destroy(gameObject);
    }

    private IEnumerator FadeMaterialCoroutine()
    {
        Material fadeMaterial =
            fadeObject.GetComponent<MeshRenderer>().material;
        MeshRenderer fadeRenderer = 
            fadeObject.GetComponent<MeshRenderer>();
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
}
