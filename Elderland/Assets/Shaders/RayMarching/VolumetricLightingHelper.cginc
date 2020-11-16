// Helper functions for volumetric lighting in forward rendering.
#include "UnityCG.cginc"
#include "AutoLight.cginc"

#define Pi 3.151592

// Angle between working
float AngleBetween(float3 u, float3 v)
{
    float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
    float denominator = length(u) * length(v);
    return acos(numerator / denominator);
}

// Function, tested in mono script
float3 Reflect(float3 w, float3 v)
{
    float3 wPerpendicular = w - (((w.x * v.x) + (w.y * v.y) + (w.z * v.z)) / pow(length(v), 2)) * v;
    return w - 2 * wPerpendicular;
}

struct filler
{
    float4 _ShadowCoord;
};

float ApproximateDepth(int n, float3 ray, float depth, sampler2D rawShadowMap)
{
    #if defined (SHADOWS_SCREEN)
    float atmosphere = 0.0;
    //n = 1;
    for (int i = 0; i < n; i++)
    {
        float currentDepth = (i * 1.0) / (n - 1);//
        float4 viewPosition = float4(ray * currentDepth, 1);
        float4 worldPosition = mul(unity_CameraToWorld, viewPosition);
        float4 shadowPosition = mul(unity_WorldToShadow[0], worldPosition);
        float4 existingObjectPosition = mul(unity_WorldToObject, worldPosition);
        float4 existingClipPosition = UnityObjectToClipPos(existingObjectPosition);
        filler f;
        f._ShadowCoord = ComputeScreenPos(existingClipPosition);
        //f._ShadowCoord.z = ;
        //Need to somehow get depth at from light source.
        //From HLSLSupport.cginc
        //float shadowDistance = SAMPLE_RAW_DEPTH_TEXTURE_PROJ(_ShadowMapTexture, UNITY_PROJ_COORD(f._ShadowCoord)).w;

        //shadowPosition = mul(shadowToWorld, shadowPosition);
        //shadowPosition = mul(unity_WorldToShadow[0], shadowPosition);

        float shadowDistance = unitySampleShadow(shadowPosition); //auto comparing, need to get raw info.
        
        //return tex2D(rawShadowMap, shadowPosition.xy).r;
        //return shadowPosition.z;

        //return abs(tex2D(rawShadowMap, shadowPosition.xy).r - shadowPosition.z);

        //return Linear01Depth(shadowDistance / shadowPosition.w);
        //return shadowDistance < shadowPosition.z / shadowPosition.w;
        //return shadowPosition.z / shadowPosition.w;

        //return shadowPosition.z / shadowPosition.w;

        //return shadowDistance;
        
        if (depth >= currentDepth)
        {
            if (shadowDistance > shadowPosition.z / shadowPosition.w)
            {
                //atmosphere = float3(0,0,0);

            }
            else
            {
                float distanceFactor = 1.0 / pow(80 * abs(tex2D(rawShadowMap, shadowPosition.xy).r - shadowPosition.z), 1.5); // works but effect is flipped on what I want.
                atmosphere = atmosphere + 
                    (15.0 / n) * saturate(distanceFactor) * (1 - currentDepth);
                    // 

                // * abs(shadowDistance - shadowPosition.z / shadowPosition.w) good linear fade
            }
        }
        else
        {
            //atmosphere = float3(1,0,0);
        }
        //float4 existingObjectPosition = mul(unity_WorldToObject, existingWorldPosition);
        //float4 existingClipPosition = UnityObjectToClipPos(existingObjectPosition);
        //if (depth > currentDepth)
        //{
        //    filler f;
        //    f._ShadowCoord = ComputeScreenPos(existingClipPosition);
        //    atmosphere = atmosphere + SHADOW_ATTENUATION(f) * 1;//(1.0 / n)
        //}
    }

    if (atmosphere > 0.15)
        atmosphere = 0.15;

    return 1 - atmosphere;
    #else
    return float3(1,1,1);
    #endif
}