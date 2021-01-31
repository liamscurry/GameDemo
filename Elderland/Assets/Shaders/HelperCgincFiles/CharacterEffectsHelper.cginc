/*
// From CharacterEffectsHelper.cginc
_ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1.0
*/

#ifndef CHARACTER_EFFECTS_HELPER
#define CHARACTER_EFFECTS_HELPER
float _ClipThreshold;

inline void ApplyCharacterFade(float4 objectPos) 
{
    if (-objectPos.y + 1 > _ClipThreshold * 2)
        clip(-1); 
}

#endif