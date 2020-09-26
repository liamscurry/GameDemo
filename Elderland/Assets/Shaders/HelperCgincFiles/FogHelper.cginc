float4 MultiplyColor(float percentage, float lightness, float4 startColor, float4 midColor)
{
    float4 multipliedColor = midColor * (1 - lightness) + startColor * lightness;
    return startColor * (1 - percentage) + 
           multipliedColor * percentage;
}

float4 WarmShadowColor(float4 startColor, float3 worldNormal, float isInShadowSide, float percentage)
{
    float normalVerticalAngle = AngleBetween(worldNormal, float3(0, 1, 0)) / PI;
    float normalWarmPercentage = (normalVerticalAngle - 0.5) * 2;
    //if (isInShadowSide)
    //{
        float warmLightness = RGBLightness(startColor) * 1.2;
        
        float4 warmModifier =
                float4(1, .55, .1, 1) * (normalWarmPercentage) + 
                float4(1, 1, 1, 1) * (1 - normalWarmPercentage);
            
        float4 newColor = startColor *
                (warmModifier * (1 - warmLightness) +
                float4(1,1,1,1) * (warmLightness));

        float4 blendedColor = newColor * (1 - isInShadowSide) + startColor * (isInShadowSide);
        
        return blendedColor * percentage + startColor * (1 - percentage);
    //}
    //return startColor;
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
    float distanceFactor = saturate((300.0 - (distance + 20)) / 50);
    if (distance < 30)
        distanceFactor = saturate(distanceFactor - saturate(1 - (distance / 30)));
    //return distanceFactor;
    //return distanceFactor;
    //return startColor;
    float4 haloAddonColor = 
        float4(
            distanceFactor * viewSunAngle,
            distanceFactor * viewSunAngle,
            distanceFactor * viewSunAngle,
            1);
    float haloFactor = distanceFactor * viewSunAngle;
    //return haloAddonColor;
    // + haloAddonColor * .75 * fixed4(235.0 / 255, 195.0 / 255, 52.0 / 255, 1)
    // * fixed4(1, 0.7, 0.3, 1)
    //startColor + fixed4(1, 0.7, 0.3, 0) * haloAddonColor
    //fixed4(253.0 / 255, 255.0 / 255, 222.0 / 255, 0)
    //return haloAddonColor;
    //float4(153.0 / 255, 195.0 / 255, 231.0 / 255, 1)
    return (fixed4(253.0 / 255, 255.0 / 255, 222.0 / 255, 0) * float4(.9, .9, .9, 1)) * haloFactor + 
           startColor * (1 - haloFactor);
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
    float isInShadowSide,
    float temperaturePercentage)
{
    //return startColor;
    startColor = WarmShadowColor(startColor, worldNormal, isInShadowSide, temperaturePercentage);
    //return startColor;

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

        float percentageMid = saturate((distance - startDistance) / (midDuration));
        returnColor = MultiplyColor(percentageMid, lightness, startColor, midColor);
        
        float percentage = saturate((distance - startDistance) / (endDuration));
        //return fixed4(percentage,percentage,percentage,1);
        float clampedHeight = worldPos.y + 20;
        if (clampedHeight < 0)
            clampedHeight = 0;
        percentage = saturate(percentage - clampedHeight / 100.0);
        returnColor = returnColor * (1 - percentage) + 
                      endColor * percentage;
        //return saturate((distance - startDistance) / 100);
        //float4 verticalFog = float4(1,1,1,0) * 1 * saturate((distance - startDistance) / 175) * (1 - saturate((worldPos.y + 10.0) / 20));
        //return verticalFog * .2;
        //returnColor = startColor + verticalFog * .1;
        //return returnColor;
        return HaloColor(returnColor, length(cameraPos - worldPos), worldPos);
    }
}
#define FOGCOLOR fixed4(201.0 / 255, 223.0 / 255, 255.0 / 255, 0)
#define FOGTINTCOLOR fixed4(63.0 / 255, 132.0 / 255, 235.0 / 255, 0)
//float4(49.0 / 255, 82.0 / 255, 171.0 / 255, 255.0 / 255)
//float4(181.0 / 255, 215.0 / 255, 244.0 / 255, 255.0 / 255) * float4(.95, .95, .95, 1)
#define STANDARD_FOG(color) return ApplyFog(color, FOGTINTCOLOR, FOGCOLOR, i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 120, 200, 200, i.normal, inShadow, 1);
#define STANDARD_FOG_TEMPERATURE(color, temperature) return ApplyFog(color, FOGTINTCOLOR, FOGCOLOR, i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 120, 200, 200, i.normal, inShadow, temperature);
#define STANDARD_SHADOWSIDE_FOG(color) return ApplyFog(color, FOGTINTCOLOR, FOGCOLOR, i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 120, 200, 200, i.normal, 1, 1);
#define STANDARD_SHADOWSIDE_FOG_TEMPERATURE(color, temperature) return ApplyFog(color, FOGTINTCOLOR, FOGCOLOR, i.worldPos.xyz, _WorldSpaceCameraPos.xyz, 120, 200, 200, i.normal, 1, temperature);