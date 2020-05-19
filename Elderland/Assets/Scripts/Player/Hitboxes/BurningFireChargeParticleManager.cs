using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningFireChargeParticleManager : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer floorRenderer1;
    [SerializeField]
    private MeshRenderer floorRenderer2;
    [SerializeField]
    private Texture fadeInTexture;
    [SerializeField]
    private Texture fadeOutTexture;

    private float timerIn;
    private float timerOut;

    public MeshRenderer ActiveFloorRenderer { get; private set; }
    public MeshRenderer DeactivatedFloorRenderer { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    public void SetMaterialOffset()
    {
        ActiveFloorRenderer.material.SetFloat("_TimeOffset", UnityEngine.Random.value);
    }

    private void Initialize()
    {
        ActiveFloorRenderer = floorRenderer1;
        DeactivatedFloorRenderer = floorRenderer2;
        ActiveFloorRenderer.material.SetFloat("_Threshold", 1);
        DeactivatedFloorRenderer.material.SetFloat("_Threshold", 1);
    }

    public void Reset()
    {
        Initialize();
        StopAllCoroutines();
    }

    public void SwapActive()
    {
        MeshRenderer temp = ActiveFloorRenderer;
        ActiveFloorRenderer = DeactivatedFloorRenderer;
        DeactivatedFloorRenderer = temp;
    }

    public void FadeActiveIn()
    {
        timerIn = 1;
        ActiveFloorRenderer.material.SetTexture("_MainTex", fadeInTexture);
        StartCoroutine(FadeInCoroutine());
    }

    public void FadeDeactivatedOut()
    {
        timerOut = 0;
        DeactivatedFloorRenderer.material.SetTexture("_MainTex", fadeOutTexture);
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        while (timerIn > 0)
        {
            timerIn -= Time.deltaTime;
            ActiveFloorRenderer.material.SetFloat("_Threshold", timerIn);
            yield return new WaitForEndOfFrame();
        }
        ActiveFloorRenderer.material.SetFloat("_Threshold", 0);
    }

    private IEnumerator FadeOutCoroutine()
    {
        while (timerOut < 1)
        {
            timerOut += Time.deltaTime;
            DeactivatedFloorRenderer.material.SetFloat("_Threshold", timerOut);
            yield return new WaitForEndOfFrame();
        }
        DeactivatedFloorRenderer.material.SetFloat("_Threshold", 1);
    }
}
