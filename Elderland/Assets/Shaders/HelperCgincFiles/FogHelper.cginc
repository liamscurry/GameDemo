float4 MultiplyColor(float percentage, float lightness, float4 startColor, float4 midColor)
{
    float4 multipliedColor = midColor * (1 - lightness) + startColor * lightness;
    return startColor * (1 - percentage) + 
           multipliedColor * percentage;
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
    float normalVerticalAngle = AngleBetween(worldNormal, float3(0, 1, 0)) / PI;
    if (isInShadowSide)
    {
        return float4(normalVerticalAngle, 0, 0, 1);
    }

    float2 horizontalDisplacement = 
        (cameraPos - worldPos).xz;
    float distance = length(float3(horizontalDisplacement.x, 0, horizontalDisplacement.y));
    float toMidPercentage = 0;
    if (distance < startDistance)
    {
        return startColor;
    }
    else //  if (distance >= startDistance) && distance < startDistance + midDuration
    {
        float lightness = RGBLightness(startColor) * 0.5 + 0.3;
        
        if (distance < startDistance + midDuration)
        {
            float percentageMid = saturate((distance - startDistance) / (midDuration)) * 1.2f;
            return MultiplyColor(percentageMid, lightness, startColor, midColor);
        }
        else
        {
            float percentageMid = 1;
            float4 finalMidColor = MultiplyColor(percentageMid, lightness, startColor, midColor);

            float percentage = saturate((distance - (startDistance + midDuration)) / (endDuration));
            return finalMidColor * (1 - percentage) + 
                   endColor * percentage;
        }
    }
}

#define STANDARD_FOG(color) return ApplyFog(color, float4(49.0 / 255, 82.0 / 255, 171.0 / 255, 255.0 / 255), float4(181.0 / 255, 215.0 / 255, 244.0 / 255, 255.0 / 255) * float4(.95, .95, .95, 1), i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 40, 350, 300, i.normal, 0);
#define STANDARD_SHADOWSIDE_FOG(color) return ApplyFog(color, float4(49.0 / 255, 82.0 / 255, 171.0 / 255, 255.0 / 255), float4(181.0 / 255, 215.0 / 255, 244.0 / 255, 255.0 / 255) * float4(.95, .95, .95, 1), i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 40, 350, 300, i.normal, 1);