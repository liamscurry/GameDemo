//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/SemiFlatShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpMapIntensity ("BumpMapIntensity", Range(0, 1)) = 0
        _SpecularMap ("SpecularMap", 2D) = "white" {}

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
        _Smoothness ("Smoothness", Range(0, 2)) = 0
        _SmoothnessMap ("SmoothnessMap", 2D) = "white" {}
        _ClampSmoothnessMap ("ClampSmoothnessMap", Range(0, 1)) = 0 //Clamps values of the smoothness map to 1 or 0.

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        // From CharacterEffectsHelper.cginc
        _ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1.0
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

        Pass
        {
            Name "VolumetricOcclusion"
            Tags
            {
                "RenderType"="Overlay"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment VolumeFrag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderBase.cginc"

            fixed4 VolumeFrag(customV2F i, fixed facingCamera : VFACE) : SV_Target
            {
                float inShadow = SHADOW_ATTENUATION(i);
                return fixed4(inShadow, inShadow, inShadow, 1);
                return fixed4(1,0,0,1);
            }
            ENDCG
        }
    }
}
