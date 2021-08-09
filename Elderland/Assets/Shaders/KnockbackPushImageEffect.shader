﻿// Shader used for player knockback push warp effect.

// References:
// Examples on unity documentation.
Shader "Custom/KnockbackPushImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Size of the warp effect from the center
        _Radius ("Radius", Range(0, 1)) = 0.5
        // Width of the warp effect from the ring defined by the radius above.
        _Width ("Width", Range(0, 1)) = 0
        // Strength of the image effect
        _SizeWarpStrength ("SizeWarpStrength", Range(0,2)) = 1
        // Should there be a dim filter applied to the effected area.
        _Dimness ("Dimness", Range(0.0, 1.0)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }// "LightMode"="ForwardBase"
        GrabPass { }
        Blend SrcAlpha OneMinusSrcAlpha
        //LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
            //#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float4 grabPos : TEXCOORD5;
                float2 uvOriginal : TEXCOORD6;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            sampler2D _GrabTexture;
            float _Radius;
            float _Width;
            float _SizeWarpStrength;
            float _Dimness;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                //float4 alteredVertex = v.vertex;
                //alteredVertex.y = v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                // From grab pass manual.
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uvOriginal = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i, float3 normal : NORMAL) : SV_Target
            {
                //float2 screenPos = i.screenPos.xy / i.screenPos.w;
                //float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

                float warpUVStrength = 
                    length(i.uvOriginal - float2(0.5, 0.5)) - _Radius;
                if (_Width != 0)
                {
                    if (warpUVStrength > _Width)
                    warpUVStrength = _Width;
                    if (warpUVStrength < -1 * _Width)
                        warpUVStrength = -1 * _Width;
                    warpUVStrength = warpUVStrength / _Width;
                    float radialPercentage = warpUVStrength;
                    warpUVStrength = cos(0.5 * PI * warpUVStrength);
                    warpUVStrength *= 1 - abs(radialPercentage);
                }
                else
                {
                    warpUVStrength = 0;
                }

                // Radial Strength Visualizer
                //return float4(warpUVStrength, warpUVStrength, warpUVStrength, 1);

                float2 centerDisplacement = float2(0.5, 0.5) - i.uvOriginal;
                float4 warpDirection = float4(normalize(centerDisplacement).x, 0, 0, 0);
                warpDirection.x *= warpUVStrength * _SizeWarpStrength;
                warpDirection.y *= warpUVStrength * _SizeWarpStrength;
                float4 existingColor = tex2Dproj(_GrabTexture, i.grabPos + warpDirection);
                
                float lightnessMultiplier = 0.075 * _Dimness * (warpUVStrength * _SizeWarpStrength);

                existingColor = existingColor - 
                    float4(lightnessMultiplier,
                           lightnessMultiplier,
                           lightnessMultiplier,
                           0);

                return existingColor;
            }
            ENDCG
        }
    }
}
