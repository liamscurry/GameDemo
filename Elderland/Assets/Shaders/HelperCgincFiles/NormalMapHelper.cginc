/* Paste into top of shader.
#pragma multi_compile_local __ _ALPHATEST_ON
#pragma multi_compile_local __ _NORMALMAP

#define TERRAIN_STANDARD_SHADER
#define TERRAIN_INSTANCED_PERPIXEL_NORMAL

#include "/HelperCgincFiles/NormalMapHelper.cginc"
*/

/*
For non terrain based normal mapping:
half3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));
tangentNormal.y *= -1;
pass tangentNormal into TangentToWorldSpace function.
*/
#ifndef NORMALMAP_HELPER
#define NORMALMAP_HELPER

#include "TerrainSplatmapCommon.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityCG.cginc"

#define DECLARE_TANGENT_SPACE(y1, y2, y3) half3 tanX1 : TEXCOORD##y1; half3 tanX2 : TEXCOORD##y2; half3 tanX3 : TEXCOORD##y3;

// Normal space matrix computation
void ComputeTangentSpace(float3 normal, float4 tangent, inout half3 tanX1, inout half3 tanX2, inout half3 tanX3)
{
    half3 worldNormal = UnityObjectToWorldNormal(normal);
    half3 worldTangent = UnityObjectToWorldDir(tangent.xyz);
    half crossSign = tangent.w * unity_WorldTransformParams.w;
    half3 worldCross = -1 * cross(worldNormal, worldTangent) * crossSign;
    tanX1 = half3(worldTangent.x, worldCross.x, worldNormal.x);
    tanX2 = half3(worldTangent.y, worldCross.y, worldNormal.y);
    tanX3 = half3(worldTangent.z, worldCross.z, worldNormal.z);
}

half3 TangentToWorldSpace(half3 tanX1, half3 tanX2, half3 tanX3, half3 tangentNormal)
{
    half3 worldNormal;
    worldNormal.x = dot(tanX1, tangentNormal);
    worldNormal.y = dot(tanX2, tangentNormal);
    worldNormal.z = dot(tanX3, tangentNormal);
    return worldNormal;
}
#endif