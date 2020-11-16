Shader "Custom/VolumetricSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SunColor ("SunColor", Color) = (1,1,1,1)
        _MoonColor ("MoonColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Less

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            // Angle between working
            #define Pi 3.151592

            float AngleBetween(float3 u, float3 v)
            {
                float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
                float denominator = length(u) * length(v);
                return acos(numerator / denominator);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float3 ray : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                float3 rayOriginal = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex);
                o.ray = rayOriginal;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _OcclusionTexture;
            float4 _SunColor;
            float4 _MoonColor;
            float4 _SunDirection;
            float4 _MoonDirection;

            fixed4 frag (v2f i) : SV_Target
            {
                //return fixed4(1,1,0,1);
                float2 uv = i.screenPos.xy / i.screenPos.w;

                float sunHalo = pow(AngleBetween(_SunDirection.xyz, i.ray) / Pi, 3);
                float sunFactor = 1 - saturate(1 - pow(AngleBetween(_SunDirection.xyz, float3(0,1,0)) / (Pi / 2), 12) + 0.8);
                sunFactor = saturate(sunFactor + 0.2);

                //return moonColor;
                return (float4(sunFactor, sunFactor, sunFactor, 1) * float4(.5,.5,.5, 1));
            }
            ENDCG
        }
    }
}
