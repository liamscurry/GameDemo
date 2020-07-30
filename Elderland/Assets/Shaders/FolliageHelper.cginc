#include "/HelperCgincFiles/MathHelper.cginc"

float3 WarpFolliage(float3 vertex, float2 uv, float3 normal)
{
    float3 alteredObjectVertex = vertex;
    float3 planarDirection = cross(normal, float3(1,0,0));
    float3 oppositePlanarDirection = cross(planarDirection, normal);
    float2 scaledUV = uv * float2(2, 2) - float2(1, 1); 
    float opposingScalerX = (scaledUV.x > 0) ? 1 : 1.5;
    float opposingScalerY = (scaledUV.y > 0) ? 1 : 1.5;
    alteredObjectVertex += sin(_Time * 1.5 * opposingScalerX) * planarDirection * .025 * scaledUV.x;
    alteredObjectVertex += sin(_Time * 1.8 * opposingScalerY) * oppositePlanarDirection * .0375 * scaledUV.y;
    return alteredObjectVertex;
}

#define WAVE_EQUATION(x) (sin(5 * x)* pow((cos(5 * x) - PI), 3) * (1 / (2 * PI)) + 6.5) * 0.2 * 0.6

float3 WarpGrass(float3 vertex, float uvy, float3 normal)
{
    //i.color.a is 0 on bottom vertex and 1 on top vertex.
    float3 alteredObjectVertex = vertex;
    float3 planarDirection = cross(normal, float3(1,0,0));
    float3 oppositePlanarDirection = cross(planarDirection, normal);
    //float2 scaledUV = uv * float2(2, 2) - float2(1, 1); 
    float4 worldPos = mul(unity_ObjectToWorld, float4(vertex, 1));
    //alteredObjectVertex += sin(_Time * .75) * planarDirection * .1 * uv.y * sin(worldPos.x * 0.25 + _Time * 0.375);
    
    float waveCondition = WAVE_EQUATION((worldPos.x + worldPos.z) / 20 + _Time * 35);// sin(worldPos.x * .2 - _Time * 30) + 2
    //if (waveCondition > 0.9)
    {
        alteredObjectVertex += float3(-.2,0,-.2) * waveCondition * uvy;
    }

    //alteredObjectVertex += float3(1, 0, 0) * pow((sin(worldPos.x + _Time * 7) + 1), 2) * hueFactorUV;
    //alteredObjectVertex += sin(_Time * 3) * normal * .075 * uv.x;
    return alteredObjectVertex;
}