using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Based on CommandBuffer.SetShadowSamplingMode manual page.

[ExecuteAlways]
public class VolumetricBlit : MonoBehaviour
{
    [SerializeField]
    private Renderer occlusionRenderer;
    [SerializeField]
    private Material occlusionMaterial;
    [SerializeField]
    private RenderTexture occlusionTexture;

    [SerializeField]
    private Light sunLight;
    [SerializeField]
    private Light moonLight;
    [SerializeField]
    private MeshRenderer applicatorRenderer;
    [SerializeField]
    private Color dayTimeColor;
    [SerializeField]
    private Color nightTimeColor;

    [SerializeField]
    private Camera gameCamera;
    [SerializeField]
    private Camera sensorCamera;

    private Light[] lights;

    private void Awake()
    {
        Camera.onPreRender = null;
        Camera.onPreRender += CopyOcclusionTexture;
        lights = new Light[2];
        lights[0] = sunLight;
        lights[1] = moonLight;
    }

    private float AngleBetween(Vector3 u, Vector3 v)
    {
        float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
        float denominator = (u).magnitude * (v).magnitude;
        return Mathf.Acos(numerator / denominator);
    }

    private void CopyOcclusionTexture(Camera currentCamera)
    {
        if (currentCamera.depthTextureMode != DepthTextureMode.Depth)
        {
            currentCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        foreach (Light light in lights)
        {
            if (light.enabled)
            {
                light.RemoveAllCommandBuffers();
                var lightCMDBuffer = new CommandBuffer();
                RenderTargetIdentifier shadowMap = BuiltinRenderTextureType.CurrentActive;
                lightCMDBuffer.SetShadowSamplingMode(shadowMap, ShadowSamplingMode.RawDepth);
                lightCMDBuffer.Blit(shadowMap, new RenderTargetIdentifier(occlusionTexture));
                light.AddCommandBuffer(LightEvent.AfterShadowMap, lightCMDBuffer);
            }
        }

        applicatorRenderer.sharedMaterial.SetColor("_SunColor",  lights[0].color);
        applicatorRenderer.sharedMaterial.SetColor("_MoonColor", lights[1].color);
        
        float sunIntensityFactor = CalculateSunIntensityFactor();
        float moonIntensityFactor = CalculateMoonIntensityFactor() * 0.6f;

        applicatorRenderer.sharedMaterial.SetFloat("_SunFogIntensity",  CalculateSunFogIntensityFactor());
        applicatorRenderer.sharedMaterial.SetFloat("_MoonFogIntensity", CalculateMoonFogIntensityFactor());

        applicatorRenderer.sharedMaterial.SetFloat("_SunColorIntensity",  CalculateSunColorIntensityFactor());
        applicatorRenderer.sharedMaterial.SetFloat("_MoonColorIntensity", CalculateMoonColorIntensityFactor());

        applicatorRenderer.sharedMaterial.SetFloat("_SunColorTransition",  CalculateSunColorTransitionFactor());
        applicatorRenderer.sharedMaterial.SetFloat("_SunSaturationTransition",  CalculateSunSaturationIntensityFactor());

        applicatorRenderer.sharedMaterial.SetFloat("_MoonColorTransition", CalculateMoonColorTransitionFactor());

        applicatorRenderer.sharedMaterial.SetFloat("_SunExpansionIntensity",  CalculateSunExpansionIntensityFactor());
        applicatorRenderer.sharedMaterial.SetFloat("_MoonExpansionIntensity", CalculateMoonExpansionIntensityFactor());

        lights[0].color = new Color(sunIntensityFactor,sunIntensityFactor,sunIntensityFactor,1) * dayTimeColor;
        lights[0].intensity = sunIntensityFactor;

        lights[1].color = new Color(moonIntensityFactor,moonIntensityFactor,moonIntensityFactor,1) * nightTimeColor;
        lights[1].intensity = moonIntensityFactor; //0.5
        Shader.SetGlobalVector("_SunDirection", lights[0].transform.forward);
        Shader.SetGlobalVector("_MoonDirection", lights[1].transform.forward);
    }

    private float CalculateSunIntensityFactor()
    {
        //1 - Mathf.Clamp01(1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) / (Mathf.PI / 2f)), 12))
        //Debug.Log(1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2 / Mathf.PI), 13));
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2 / Mathf.PI), 17);
    }

    private float CalculateMoonIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) * 2 / Mathf.PI), 17);
    }

    private float CalculateSunFogIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2 / Mathf.PI), 27);
    }

    private float CalculateMoonFogIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) * 2 / Mathf.PI), 45);
    }

    private float CalculateSunColorIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2.325f / Mathf.PI), 12);
    }

    private float CalculateSunSaturationIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2f / Mathf.PI), 12);
    }

    private float CalculateMoonColorIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) * 2.4f / Mathf.PI), 12);
    }

    private float CalculateSunColorTransitionFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2.1f / Mathf.PI), 20);
    }

    private float CalculateMoonColorTransitionFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) * 2.1f / Mathf.PI), 20);
    }

    private float CalculateSunExpansionIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,-1,0)) * 2.1f / Mathf.PI), 4);
    }

    private float CalculateMoonExpansionIntensityFactor()
    {
        return 1 - Mathf.Pow(Mathf.Clamp01(AngleBetween(lights[0].transform.forward, new Vector3(0,1,0)) * 2.1f / Mathf.PI), 4);
    }
}