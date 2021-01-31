/*
// From CharacterEffectsHelper.cginc
_ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1.0
*/

#ifndef CHARACTER_EFFECTS_HELPER
#define CHARACTER_EFFECTS_HELPER
float _ClipThreshold;

inline float4 GenerateWorldOffset(float4 vertex) 
{
    float4 worldPos = mul(unity_ObjectToWorld, vertex);
    float4 worldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1));
    return worldPos - worldCenter;
}

inline float4 PercentageFromWorldDisplacement(float4 worldDisplacement, float _WorldMaxHeight)
{
    float verticalPercentage = worldDisplacement.y;
    verticalPercentage = verticalPercentage / _WorldMaxHeight;
    if (verticalPercentage > 1)
        verticalPercentage = 1;
    if (verticalPercentage < -1)
        verticalPercentage = -1;
    return float4(0, verticalPercentage, 0, 1);
}

inline void ApplyCharacterFade(float4 worldDisplacement, float _WorldMaxHeight) 
{
    float4 objectPos =
        PercentageFromWorldDisplacement(worldDisplacement, _WorldMaxHeight);

    if (-objectPos.y + 1 > _ClipThreshold * 2)
        clip(-1); 
}

inline void ApplyCharacterFadeRaw(float4 worldDisplacement) 
{
    float4 objectPos =
        worldDisplacement;

    if (-objectPos.y + 1 > _ClipThreshold * 2)
        clip(-1); 
}

#endif