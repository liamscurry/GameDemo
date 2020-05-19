// Based on UnityDeferredLibrary.cginc and Internal-DeferredReflections.shader files.
Shader "Custom/StampParticle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // From RayMarching.cginc in my RayMarchingResearch project.
            #define Pi 3.151592

            // Angle between working
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
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                //Found in UnityDeferredLibrary
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _Color;
            //UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 worldPosition = mul(unity_CameraToWorld, viewPosition);
                float3 horizontalWorldposition = float3(worldPosition.x, 0, worldPosition.z);
                // Helpful tip in forum "SHADER: Get *object* position or distinct valuer per *object*"
                float4 worldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1));
                float3 horizontalWorldCenter = float3(worldCenter.x, 0, worldCenter.z);
                float distanceBetween = length(horizontalWorldposition - horizontalWorldCenter);
                float radius = mul(unity_ObjectToWorld, float4(1,0,0,0)) / 2;
                float inRange = distanceBetween >= radius;
                clip(-inRange);
                return _Color;
            }
            ENDCG
        }
    }
}
