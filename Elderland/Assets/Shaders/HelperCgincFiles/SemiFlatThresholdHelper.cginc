#include "../HelperCgincFiles/MathHelper.cginc"
float SFFresnelThreshold(float4 screenPos, float3 worldView, float3 worldNormal, float2 uv, fixed facingCamera)
{
    float2 screenPosPercentage = screenPos.xy / screenPos.w;

    float fresnel = 
        AngleBetween(worldView, worldNormal) / (PI / 2);
    float alteredFresnel = 
        pow(fresnel, 1);
    if (alteredFresnel < 0.5)
    {
        alteredFresnel = 0;
    }
    else
    {
        alteredFresnel = (alteredFresnel - 0.5) / 0.5;
    }
    alteredFresnel = 1 - alteredFresnel;

    float cutoutColor = tex2D(_CutoutTexture, uv).r;
    float scaledClipThreshold = _CutoutThreshold;
    float clipped = cutoutColor * alteredFresnel - scaledClipThreshold;
    return clipped;
}