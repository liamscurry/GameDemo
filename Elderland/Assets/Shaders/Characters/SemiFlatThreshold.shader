﻿Shader "Custom/SemiFlatThreshold"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpMapIntensity ("BumpMapIntensity", Range(0, 1)) = 0

        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _CrossFade ("CrossFade", float) = 0

        // Properties from Shading Helper
        _FlatShading ("FlatShading", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
        _BakedLightLevel ("BakedLightLevel", Range(0, 1)) = 1

        _HighlightStrength ("HightlightStrength", Range(0, 2)) = 1 
        _HighlightIntensity ("HighlightIntensity", Range(0, 2)) = 1

        _ReflectedIntensity ("ReflectedIntensity", Range(0, 6)) = 1

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        [HideInInspector]_ClipThreshold ("CutoutThreshold", Range(0.0, 1.0)) = 1.0

        // From CharacterEffectsHelper.cginc
        _CutoutThreshold ("CutoutThreshold", Range(0.0, 1.0)) = 1.0
        _CutoutTexture ("CutoutTexture", 2D) = "white" {}
    }
    SubShader
    {
        LOD 400

        // SemiFlatShader pass structure
        Pass
        {
            Name "SemiFlatShaderShadow"
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            
            sampler2D _CutoutTexture;
            float _CutoutThreshold;

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderShadowCaster.cginc"
            #include "../HelperCgincFiles/SemiFlatThresholdHelper.cginc"
            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float clipped =
                    SFFresnelThreshold(i.screenPos, i.worldView, i.worldNormal, i.uv, facingCamera);
                clip(clipped);

                return semiFlatFrag(i);
            }
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
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            sampler2D _CutoutTexture;
            float _CutoutThreshold;

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderBase.cginc"
            #include "../HelperCgincFiles/SemiFlatThresholdHelper.cginc"
            fixed4 frag(customV2F i, fixed facingCamera : VFACE) : SV_Target
            {
                float clipped =
                    SFFresnelThreshold(i.screenPos, i.worldView, i.worldNormal, i.uv, facingCamera);
                clip(clipped);

                return semiFlatFrag(i, facingCamera);
            }
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
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster

            sampler2D _CutoutTexture;
            float _CutoutThreshold;

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderAdditive.cginc"
            #include "../HelperCgincFiles/SemiFlatThresholdHelper.cginc"
            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float clipped =
                    SFFresnelThreshold(i.screenPos, i.worldView, i.normal, i.uv, facingCamera);
                clip(clipped);

                return lightHelperFrag(i, facingCamera);
            }
            ENDCG
        }
    }
}
