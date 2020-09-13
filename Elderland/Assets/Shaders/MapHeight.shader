// Based on UnityDeferredLibrary.cginc and Internal-DeferredReflections.shader files.
Shader "Custom/MapHeight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _SeaHeightColor ("SeaHeightColor", Color) = (1,1,1,1)
        _MountainHeightColor ("MountainHeightColor", Color) = (1,1,1,1)
        _TimeOffset ("TimeOffset", float) = 0
        _Threshold ("Threshold", Range(0,1)) = 0
    }
    SubShader
    {
        // No culling or depth
        ZWrite Off ZTest Always Cull Off
        Tags { "Queue"="Geometry+10"}
        Blend DstColor Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // From RayMarching.cginc in my RayMarchingResearch project.
            #define Pi 3.1515926536

            // Angle between working
            float AngleBetween(float3 u, float3 v)
            {
                float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
                float denominator = length(u) * length(v);
                return acos(numerator / denominator);
            }

            // explored via Desmos grapher
            float LimitRipple(float x)
            {
                return pow(x / 0.5, 8) - 1;
            }

            float Ripple(float x, float t)
            {
                float g = 0.75 * sin(20 * Pi * (x + 20 * t)) * LimitRipple(x);
                float h = 0.50 * sin(10 * Pi * (x - 10 * t)) * LimitRipple(x);
                float i = 3.00 * sin(4 * Pi * (x + 4 * t))   * LimitRipple(x);
                return sin(2 * Pi * x) + g + h + i;
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 ray : TEXCOORD1;
                float3 localPosition : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                //Found in UnityDeferredLibrary
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.localPosition = v.vertex;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _Color;
            float _TimeOffset;
            float _Threshold;
            float4 _SeaHeightColor;
            float4 _MountainHeightColor;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 worldPosition = mul(unity_CameraToWorld, viewPosition);
                float colorInterpolation = ((depth * 2000) - .85) * 5;
                return _SeaHeightColor * colorInterpolation + _MountainHeightColor * (1 - colorInterpolation);
            }
            ENDCG
        }
    }
}
