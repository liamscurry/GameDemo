float4 MultiplyColor(float percentage, float lightness, float4 startColor, float4 midColor)
{
    float4 multipliedColor = midColor * (1 - lightness) + startColor * lightness;
    return startColor * (1 - percentage) + 
           multipliedColor * percentage;
}

float4 WarmShadowColor(float4 startColor, float3 worldNormal, float isInShadowSide)
{
    float normalVerticalAngle = AngleBetween(worldNormal, float3(0, 1, 0)) / PI;
    float normalWarmPercentage = (normalVerticalAngle - 0.5) * 2;
    if (isInShadowSide)
    {
        float warmLightness = RGBLightness(startColor) * 1.2;
        
        float4 warmModifier =
                float4(1, .55, .1, 1) * (normalWarmPercentage) + 
                float4(1, 1, 1, 1) * (1 - normalWarmPercentage);
            
        float4 newColor = startColor *
                (warmModifier * (1 - warmLightness) +
                float4(1,1,1,1) * (warmLightness));
    
        return newColor;
    }
    return startColor;
}

float4 HaloColor(float4 startColor, float distance, float3 worldPos)
{
    //return startColor;
    float3 sunDirection = _WorldSpaceLightPos0.xyz;

    // Learned in vertex/fragment shader examples in unity docs.
    float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

    float viewSunAngle = 1 - saturate(AngleBetween(-sunDirection, viewDir) / (PI / 2));
    distance -= 40;
    if (distance < 0)
        distance = 0;
    float distanceFactor = 1 - saturate((100.0 - distance) / 100.0);
    //return startColor;
    float4 haloAddonColor = 
        float4(
            distanceFactor * viewSunAngle,
            distanceFactor * viewSunAngle,
            distanceFactor * viewSunAngle,
            1);
    //return haloAddonColor;
    return startColor + haloAddonColor * .5 * fixed4(235.0 / 255, 195.0 / 255, 52.0 / 255, 1);
}

fixed4 ApplyFog(
    float4 startColor,
    float4 midColor,
    float4 endColor,
    float3 worldPos,
    float3 cameraPos,
    float startDistance,
    float midDuration,
    float endDuration,
    float3 worldNormal,
    float isInShadowSide)
{
    startColor = WarmShadowColor(startColor, worldNormal, isInShadowSide);

    float2 horizontalDisplacement = 
        (cameraPos - worldPos).xz;
    float distance = length(float3(horizontalDisplacement.x, 0, horizontalDisplacement.y));
    float toMidPercentage = 0;
    if (distance < startDistance)
    {
        return HaloColor(startColor, length(cameraPos - worldPos), worldPos);
        return startColor;
    }
    else //  if (distance >= startDistance) && distance < startDistance + midDuration
    {
        float lightness = RGBLightness(startColor) * 0.5 + 0.3;
        
        float4 returnColor = float4(0,0,0,0);

        if (distance < startDistance + midDuration)
        {
            float percentageMid = saturate((distance - startDistance) / (midDuration) * 1.2f);
            returnColor = MultiplyColor(percentageMid, lightness, startColor, midColor);
        }
        else
        {
            float percentageMid = 1;
            float4 finalMidColor = MultiplyColor(percentageMid, lightness, startColor, midColor);

            float percentage = saturate((distance - (startDistance + midDuration)) / (endDuration));
            returnColor = finalMidColor * (1 - percentage) + 
                   endColor * percentage;
        }

        return HaloColor(returnColor, length(cameraPos - worldPos), worldPos);
    }
}

#define STANDARD_FOG(color) return ApplyFog(color, float4(49.0 / 255, 82.0 / 255, 171.0 / 255, 255.0 / 255), float4(181.0 / 255, 215.0 / 255, 244.0 / 255, 255.0 / 255) * float4(.95, .95, .95, 1), i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 40, 350, 300, i.normal, 0);
#define STANDARD_SHADOWSIDE_FOG(color) return ApplyFog(color, float4(49.0 / 255, 82.0 / 255, 171.0 / 255, 255.0 / 255), float4(181.0 / 255, 215.0 / 255, 244.0 / 255, 255.0 / 255) * float4(.95, .95, .95, 1), i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 40, 350, 300, i.normal, 1);