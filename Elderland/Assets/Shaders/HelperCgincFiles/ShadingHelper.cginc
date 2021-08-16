/*
_FlatShading ("FlatShading", Range(0, 1)) = 0
_ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
_BakedLightLevel ("BakedLightLevel", Range(0, 1)) = 1

_HighlightStrength ("HightlightStrength", Range(0, 2)) = 1 
_HighlightIntensity ("HighlightIntensity", Range(0, 2)) = 1

_ReflectedIntensity ("ReflectedIntensity", Range(0, 3)) = 1
_Smoothness ("Smoothness", Range(0, 2)) = 0
*/

#ifndef SHADING_HELPER
#define SHADING_HELPER
#include "MathHelper.cginc"

float _FlatShading;
float _ShadowStrength;
float _BakedLightLevel;

float _HighlightStrength;
float _HighlightIntensity;

float _ReflectedIntensity;
float _Smoothness;

float Fresnel(float3 normal, float3 viewDir)
{
    return saturate(pow(AngleBetween(normal, viewDir) / 3.151592 * 1.7, 3.5));
} 

float StandardShadeFade(float distance)
{
    return UnityComputeShadowFade(distance);
} 

float CustomShadeFade(float distance)
{
    return UnityComputeShadowFade(distance - 100);
}   

float CompositeShadeFade(float inShadow, float distance)
{
    float standardShadeFade = StandardShadeFade(distance);
    float customShadeFade = CustomShadeFade(distance);
    float fadedOutShadow = inShadow * (1 - standardShadeFade) + _BakedLightLevel * (standardShadeFade);
    float compositeFadedShadow = fadedOutShadow * (1 - customShadeFade) + 0 * (1 - customShadeFade);
    return compositeFadedShadow;
}

float4 Shade(float3 worldNormal, float3 worldPos, float4 localColor, inout float inShadow, float fadeValue, float specular)
{
    float normalIncidence = AngleBetween(worldNormal, _WorldSpaceLightPos0.xyz) / 3.151592;

    float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
    float3 reflectedDir = reflect(-_WorldSpaceLightPos0.xyz, worldNormal);

    // Light side color calculation
    float activeHighlight =
        1 - saturate(AngleBetween(reflectedDir, viewDir) / (3.141592 * .5));
    activeHighlight = pow(activeHighlight, 2 * _HighlightIntensity * specular);
    activeHighlight = activeHighlight * _HighlightStrength * specular;
    activeHighlight = saturate(activeHighlight);
    //activeHighlight = 0; // highlight acting weird on grass

    float4 localShadowColor =
        localColor *
        fixed4(1 - _ShadowStrength, 1 - _ShadowStrength, 1 - _ShadowStrength, 1);

    float lightIncidence = pow(saturate(normalIncidence * 2), 2);

    if (_FlatShading > 0.5)
        lightIncidence *= 0.5;
        
    float4 lightColor =
        localShadowColor * lightIncidence +
        localColor * (1 - lightIncidence);
    lightColor =
        (lightColor * (1 - activeHighlight) + (_LightColor0.rgb, 1) * activeHighlight);

    // Dark side color calculation
    float shadowIncidenceBounced =
        saturate(normalIncidence - 0.6);
    shadowIncidenceBounced = 
        pow(shadowIncidenceBounced, 0.75) * 0.2 * _ReflectedIntensity;

    float shadowIncidenceAmbient =
        saturate(0.6 - normalIncidence);
    shadowIncidenceAmbient = 
        pow(shadowIncidenceAmbient, 1.3) * 0.1;

    float shadowIncidence =
        shadowIncidenceBounced + shadowIncidenceAmbient;
    shadowIncidence = shadowIncidence * 2;

    float4 localReflectedColor =
        localColor *
        fixed4(1 - _ShadowStrength * 0.5, 1 - _ShadowStrength * 0.5, 1 - _ShadowStrength * 0.5, 1);

    float4 darkColor =
        localShadowColor * (1 - shadowIncidence) +
        localReflectedColor * (shadowIncidence);

    // Light and Dark Blending
    float lightCutoff = 0.4;
    float darkCutoff = 0.5;
    float lightDarkPercentage;
    if (normalIncidence < lightCutoff)
    {
        lightDarkPercentage = 1;
    }
    else if (normalIncidence > darkCutoff)
    {
        lightDarkPercentage = 0;
    }
    else
    {
        float percentage =
            (normalIncidence - lightCutoff) / (darkCutoff - lightCutoff);
        lightDarkPercentage = 1 - percentage;
    }

    // Include shadows in blend (occlude with more dark color)
    lightDarkPercentage =
        min(fadeValue, lightDarkPercentage);
    inShadow = lightDarkPercentage;

    float4 compositeColor =
        lightColor * (lightDarkPercentage) +
        darkColor * (1 - lightDarkPercentage);

    return compositeColor;
}
#endif