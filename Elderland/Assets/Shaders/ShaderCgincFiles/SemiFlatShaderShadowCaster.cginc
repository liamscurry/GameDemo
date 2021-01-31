//CGPROGRAM
// Via SpeedTree.shader
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Assets/Shaders/HelperCgincFiles/LODHelper.cginc"
#include "Assets/Shaders/HelperCgincFiles/CharacterEffectsHelper.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float2 uv1 : TEXCOORD1; // From UnityStandardInput.cginc for transfer lighting.
};

struct v2f
{
    //float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    V2F_SHADOW_CASTER; //float4 pos : SV_POSITION thats it
    float4 screenPos : TEXCOORD1;
    float4 objectPos : TEXCOORD2;
};

sampler2D _MainTex;
float _CrossFade;
float _Threshold;
sampler2D _CutoutTex;

v2f vert (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    //UNITY_TRANSFER_LIGHTING(o, v.uv1); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
    o.screenPos = ComputeScreenPos(o.pos);
    o.objectPos = v.vertex;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    ApplyDither(i.screenPos, _CrossFade);
    ApplyCharacterFade(i.objectPos);

    SHADOW_CASTER_FRAGMENT(i)
    //return 0;
}
//ENDCG