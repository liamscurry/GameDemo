//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
//CGPROGRAM
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "TerrainSplatmapCommon.cginc"

#include "Assets/Shaders/Color.cginc"
#include "Assets/Shaders/HelperCgincFiles/NormalMapHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/ShadingHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/MathHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/FogHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/LODHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/CharacterEffectsHelper.cginc"

struct customInput
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct customV2F
{
    float2 uv : TEXCOORD0;
    //float4 _ShadowCoord : TEXCOORD1;
    SHADOW_COORDS(1)
    float4 pos : SV_POSITION;
    float3 normal : TEXCOORD2;
    float3 worldPos : TEXCOORD3;
    float3 tangent : TEXCOORD4;
    float4 screenPos : TEXCOORD5;
    DECLARE_TANGENT_SPACE(6, 7, 8)
    float4 objectPos : TEXCOORD9;
    float3 worldView : TEXCOORD10;
    float3 worldNormal : TEXCOORD11;
};

customV2F vert (customInput v, float3 normal : NORMAL, float4 tangent : TANGENT)
{
    customV2F o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.screenPos = ComputeScreenPos(o.pos);
    TRANSFER_SHADOW(o)
    o.uv = v.uv;
    // Via Vertex and fragment shader examples docs.
    o.normal = float3(0,0,1);
    o.tangent = tangent;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    ComputeTangentSpace(normal, tangent, o.tanX1, o.tanX2, o.tanX3);
    
    o.objectPos = GenerateWorldOffset(v.vertex);
    o.worldView = WorldSpaceViewDir(v.vertex);
    o.worldNormal = UnityObjectToWorldNormal(normal);
    return o;
}

float4 _Color;
sampler2D _MainTex;
sampler2D _BumpMap;
float _BumpMapIntensity;
sampler2D _SpecularMap;
sampler2D _SmoothnessMap;
float _ClampSmoothnessMap;

float _Threshold;

float _CrossFade;

float _WarmColorStrength;

float _WorldMaxHeight;

fixed4 semiFlatFrag(customV2F i, fixed facingCamera : VFACE) : SV_Target
{
    // Normal mapping
    half3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));
    tangentNormal.y *= -1;
    half3 worldNormal = 
        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
    half3 originalWorldNormal = 
        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, half3(0,0,1));

    float compositeBumpMapIntensity = _BumpMapIntensity;
    float smoothnessMapIntensity = tex2D(_SmoothnessMap, i.uv).r;
    if (_ClampSmoothnessMap > 0.5)
    {
        if (smoothnessMapIntensity  > 0.01)
        {
            compositeBumpMapIntensity = 0;
        }
    }

    worldNormal = worldNormal * compositeBumpMapIntensity + originalWorldNormal * (1 - compositeBumpMapIntensity);
    
    // Reflections
    float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); 
    float3 reflectionDirection = reflect(-viewDir, worldNormal);
    float4 reflectedColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDirection);

    // Fresnel value
    float fresnelValue = Fresnel(worldNormal, viewDir);

    float4 textureColor = tex2D(_MainTex, i.uv);
    if (textureColor.a < _Threshold)
        clip(textureColor.a - _Threshold);
    
    ApplyDither(i.screenPos, _CrossFade);

    ApplyCharacterFade(i.objectPos, _WorldMaxHeight);

    float inShadow = SHADOW_ATTENUATION(i);
    float4 localColor = _Color;
    localColor *= textureColor;

    // Learned in AutoLight.cginc
    // Shadow Fade
    float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
    float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
    float fadeValue = CompositeShadeFade(inShadow, fadeDistance);

    float specular = tex2D(_SpecularMap, i.uv).r;
    float4 shadedColor = Shade(worldNormal, i.worldPos, localColor, inShadow, fadeValue, specular);
    float4 fresnelColor = reflectedColor * fresnelValue + shadedColor * (1 - fresnelValue);
    
    float areaSunAngle =
        AngleBetween(-_WorldSpaceLightPos0.xyz, reflectionDirection) / (3.151592);
    if (areaSunAngle < 0.1)
        areaSunAngle = 0;
    areaSunAngle = pow(areaSunAngle, 7) * 0.6;
    areaSunAngle *= inShadow;
    areaSunAngle *= smoothnessMapIntensity * _Smoothness;

    float4 returnColor =
        fresnelColor * (smoothnessMapIntensity * _Smoothness) +
        shadedColor * (1 - smoothnessMapIntensity * _Smoothness);
        
    if (smoothnessMapIntensity * _Smoothness > 1)
    {
       
        float excessSmoothness = (smoothnessMapIntensity * _Smoothness) - 1;
        //return float4(excessSmoothness, excessSmoothness, excessSmoothness, 1);
        returnColor =
            reflectedColor * (excessSmoothness) +
            fresnelColor * (1 - (excessSmoothness));
    }

    returnColor =
        returnColor * (1 - areaSunAngle) + float4(1, 1, 1, 1) * areaSunAngle;
    
    //returnColor =
    //    returnColor * (1 - areaSunAngle) + _LightColor0 * areaSunAngle;

    STANDARD_FOG(returnColor, worldNormal);
}