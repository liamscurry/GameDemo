﻿Shader "Custom/SemiFlatGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "white" {}
        _BumpMapIntensity ("BumpMapIntensity", Range(0, 1)) = 0
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
        _HighlightIntensity ("HighlightIntensity", Range(0, 2)) = 1
        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0

        // Properties from Shading Helper
        _FlatShading ("FlatShading", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
        _BakedLightLevel ("BakedLightLevel", Range(0, 1)) = 1

        _ReflectedIntensity ("ReflectedIntensity", Range(0, 6)) = 1

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        // Glitch properties
        _Glitch ("Glitch", Range(0.0, 1.0)) = 1.0
        _FresnelColor ("FresnelColor", Color) = (1,1,1,1)
        _FresnelStrength ("FresnelStrength", Range(0, 1)) = 0.0

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        //CharacterEffectsHelper.cginc
        _ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        // SemiFlatShader pass structure
        Pass
        {
            Name "SemiFlatShaderShadow"
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment semiFlatFrag
            #pragma multi_compile_shadowcaster
            
            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderShadowCaster.cginc"
            ENDCG
        }

        Pass
        {
            Name "SemiFlatShader"
            Tags 
            { 
                "LightMode"="ForwardBase"
                "RenderType"="Geometry+20"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment semiFlatFrag
            #pragma multi_compile_fwdbase

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderBase.cginc"
            ENDCG
        }

        Pass
        {
            Name "PointLights"
            Tags
            {
                "LightMode"="ForwardAdd"
            }

            Blend One One
            ZWrite Off
            ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment lightHelperFrag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderAdditive.cginc"
            ENDCG
        }

        // Glitch pass
        Pass
        {
            Name "SemiFlatShader"
            Tags 
            { 
                "RenderType"="Transparent"    
                "Queue"="Transparent"    
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../Color.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"
            #include "../HelperCgincFiles/MathHelper.cginc"
            #include "../HelperCgincFiles/FogHelper.cginc"
            #include "../HelperCgincFiles/LODHelper.cginc"
            #include "../HelperCgincFiles/CharacterEffectsHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float4 objectPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 worldView : TEXCOORD4;
            };

            v2f vert (appdata v, float3 normal : NORMAL, float3 tangent : TANGENT)
            {
                v2f o;
                o.uv = v.uv;
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float4 screenPos = ComputeScreenPos(clipPos);

                o.pos = clipPos;
                o.screenPos = screenPos;
                o.objectPos = GenerateWorldOffset(v.vertex);
                
                return o;
            }

            // Glitch properties
            float _Glitch;
            float4 _FresnelColor;
            float _FresnelStrength;

            float _WorldMaxHeight;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float2 screenPosPercentage = i.screenPos.xy / i.screenPos.w;

                float glitchX = pow(sin(512 * (screenPosPercentage.x * screenPosPercentage.y)), 2);
                float glitchY = sin(512 * (screenPosPercentage.x +  screenPosPercentage.y));

                ApplyCharacterFade(i.objectPos, _WorldMaxHeight);
                float glitchColorValue = _Glitch * sign(glitchY) * .3;
                return glitchColorValue * fixed4(1, 1, 1, 1);
            }
            ENDCG
        }

        // Fresnel Pass
        Pass
        {
            Name "SemiFlatShader"
            Tags 
            { 
                "RenderType"="Transparent"    
                "Queue"="Transparent"    
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../Color.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"
            #include "../HelperCgincFiles/MathHelper.cginc"
            #include "../HelperCgincFiles/FogHelper.cginc"
            #include "../HelperCgincFiles/LODHelper.cginc"
            #include "../HelperCgincFiles/CharacterEffectsHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float4 objectPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 worldView : TEXCOORD4;
            };

            v2f vert (appdata v, float3 normal : NORMAL, float3 tangent : TANGENT)
            {
                v2f o;
                o.uv = v.uv;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.objectPos = GenerateWorldOffset(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.worldView = WorldSpaceViewDir(v.vertex);
                
                return o;
            }

            // Fresnel properties
            float4 _FresnelColor;
            float _FresnelStrength;

            float _WorldMaxHeight;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float fresnel = 
                    AngleBetween(i.worldView, i.worldNormal) / (PI / 2);
                float alteredFresnel = 
                    pow(fresnel, 1);
                if (alteredFresnel < 0.5)
                {
                    alteredFresnel = 0;
                }
                else
                {
                    alteredFresnel = (alteredFresnel - 0.5) / 0.5;
                }
                    
                float4 fresnelColor = 
                    float4(_FresnelColor.r, _FresnelColor.g, _FresnelColor.b, alteredFresnel * _FresnelStrength);

                ApplyCharacterFade(i.objectPos, _WorldMaxHeight);
                return fresnelColor;
            }
            ENDCG
        }
    }
}
