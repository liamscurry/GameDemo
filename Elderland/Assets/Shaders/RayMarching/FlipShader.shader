Shader "Custom/FlipShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "white" {}
        _CutoutTex ("CutoutTex", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _CrossFade ("CrossFade", float) = 0
        _EvenFade ("EvenFade", Range(0, 1)) = 0
        _OddFade ("EvenFade", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 2)) = 0
        _LightShadowStrength ("LightShadowStrength", Range(0, 1)) = 0
        _MidFogColor ("MidFogColor", Color) = (1,1,1,1)
        _EndFogColor ("EndFogColor", Color) = (1,1,1,1)
        _HighlightStrength ("HightlightStrength", Range(0, 2)) = 1 
        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0
        _RenderDistance ("RenderDistance", float) = 10
        _RawShadowMap ("RawShadowMap", 2D) = "white" {}
    }
    SubShader
    {
        GrabPass { "ScreenTexture" }

        // No culling or depth
        Cull Off ZWrite On

        Pass
        {
            Tags 
            { 
                "LightMode"="ForwardBase"
                "RenderType"="Geometry+20"   
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"
            #include "VolumetricLightingHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1)
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float3 tangent : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float volume : TEXCOORD6;
                float4 objectPos : TEXCOORD7;
                float3 ray : TEXCOORD8;
                float4 grabPos : TEXCOORD9;
            };

            sampler2D _CameraDepthTexture;

            v2f vert (appdata v, float3 normal : NORMAL, float3 tangent : TANGENT)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                float3 rayOriginal = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.ray = rayOriginal;
                //TRANSFER_SHADOW(o)

                //Via AutoLight.cginc.
                //o._ShadowCoord = mul(unity_WorldToShadow[0], mul(unity_ObjectToWorld, v.vertex));
                #if defined (SHADOWS_SCREEN)
                o._ShadowCoord = ComputeScreenPos(o.pos);
                //float2 scaledScreenPos = o.screenPos.xy / o.screenPos.w;
                //float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, scaledScreenPos));
                //o.volume = depth;
                #endif

                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                o.tangent = UnityObjectToWorldNormal(tangent);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.objectPos = v.vertex;

                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 

            sampler2D _MainTex;
            float _Threshold;
            float _CrossFade;
            float _EvenFade;
            float _OddFade;
            
            float _ShadowStrength;
            float _LightShadowStrength;
            float4 _MidFogColor;
            float4 _EndFogColor;
            float _HighlightStrength;
            sampler2D _BumpMap;
            sampler2D _CutoutTex;
            float _WarmColorStrength;
            //float3 _WorldSpaceLightPos0;
            sampler2D ScreenTexture;
            float _RenderDistance;
            float4x4 _ShadowToWorld;
            sampler2D _RawShadowMap;
            float4 _SunDirection;
            float4 _MoonDirection;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                //return fixed4(1,1,0,1);

                float3 ray = i.ray * (_ProjectionParams.z / i.ray.z);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

                float3 existingShadowPosition = mul(unity_WorldToShadow[0], i.worldPos);
                
                float volume = ApproximateDepth(500, ray, depth, _RawShadowMap);

                float3 existingColor = (tex2Dproj(ScreenTexture, i.grabPos)).rgb;
  
                float scaledDepth = depth;
                float fadeValue = 1 - depth;
 
                //if (scaledDepth > 0.75)
                //    fadeValue = 1;
                //return fadeValue;
                //* (fadeValue) + fixed4(1,1,1,1) * (1 - fadeValue)
                //return SAMPLE_DEPTH_TEXTURE(_ShadowMapTexture, uv);
 
                return fixed4(volume, volume, volume, 1);
                //return fixed4(existingColor, 1)- fixed4(volume, 0);//

                return depth;

                return tex2Dproj(ScreenTexture, i.grabPos) * float4(.1,.1,.1,1);
                //Found in Internal-DeferredReflections frag function
                //float3 rayOriginal = UnityObjectToViewPos(i.objectPos) * float3(-1, -1, 1);
                /*float3 ray = i.ray * (_ProjectionParams.z / i.ray.z);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

                float3 existingShadowPosition = mul(unity_WorldToShadow[0], i.worldPos);
                float3 volume = ApproximateDepth(500, ray, depth);

                return fixed4(existingShadowPosition, 1);
                return fixed4(volume, 1);*/
            }
            ENDCG
        }
    }
}
