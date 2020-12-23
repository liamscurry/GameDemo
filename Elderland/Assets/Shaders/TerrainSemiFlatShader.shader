//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.

// References
// - Environment reflection with a normal map example shader in VertexFragmentShaderExamples Unity manual page.
//   for normal mapping.
// - Standard-FirstPass/TerrainSplatmapCommon.cginc

Shader "Custom/TerrainSemiFlatShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_NormalMap("NormalMap", 2D) = "bump" {}
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
    }
    SubShader
    {
                //Cull off
        LOD 400

        // Via SpeedTree.shader
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_local __ _ALPHATEST_ON

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"

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
                float4 tc : TEXCOORD1;
                float3 normal : TEXCOORD2;
                //float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _CrossFade;
            float _Threshold;

            v2f vert (appdata_full v, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                //UNITY_TRANSFER_LIGHTING(o, v.uv1); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
                //o.screenPos = ComputeScreenPos(o.pos);
                Input data;
                SplatmapVert(v, data);
                o.tc = data.tc;

                o.normal = v.normal;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                Input data;
                data.tc = i.tc;
                // Based on built in FirstPass shader terrain diffuse.
                half4 splatControl;
                half weight;
                fixed4 splatColor;
                SplatmapMix(data, splatColor, weight, splatColor, i.normal);
  
                float4 textureColor = (tex2D(_MainTex, i.uv));
                //clip(textureColor.w - .1);
                if (textureColor.a < _Threshold)
                {
                    //return fixed4(1,0,0,1);
                    clip(textureColor.a - _Threshold);
                }

                SHADOW_CASTER_FRAGMENT(i)
                //return 0;
            }
            ENDCG
        }

        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags 
            { 
                "LightMode"="ForwardBase"
                "RenderType"="Geometry+20"
                //"RenderType"="Transparent"    
                //"Queue"="Transparent"    
            }

            //ZWrite Off

            //Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            // Default included in NormalMapHelper.cginc
            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"

            #include "/HelperCgincFiles/NormalMapHelper.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //float4 _ShadowCoord : TEXCOORD1;
                SHADOW_COORDS(1)
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float4 tc : TEXCOORD4;
                DECLARE_TANGENT_SPACE(5, 6, 7)
                float3 worldNormal : TEXCOORD8;
            };

            v2f vert (appdata_full v, float3 normal : NORMAL, float4 tangent : TANGENT)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //o._ShadowCoord = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.texcoord;//v.uv
                // Via Vertex and fragment shader examples docs.
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                // Normal space matrix computation
                ComputeTangentSpace(normal, tangent, o.tanX1, o.tanX2, o.tanX3);

                Input data;
                SplatmapVert(v, data);
                o.tc = data.tc;
                o.normal = v.normal;
                //o.normal = UnityObjectToWorldNormal(v.normal);
                //float3 worldNormal = normalize(tex2D(_TerrainNormalmapTexture, o.tc.zw).xyz * 2 - 1).xzy;

                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            //sampler2D _NormalMap;
            float _Threshold;
            float _CrossFade;
            float _EvenFade;
            float _OddFade;
            sampler2D _CameraDepthTexture;
            float _ShadowStrength;
            float _LightShadowStrength;
            float4 _MidFogColor;
            float4 _EndFogColor;
            float _HighlightStrength;
            //float3 _WorldSpaceLightPos0;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                Input data;
                data.tc = i.tc;
                // Based on built in FirstPass shader terrain diffuse.
                half4 splatControl;
                half weight; 
                fixed4 splatColor;
                half4 defaultSmoothness = half4(0,0,0,0);
                SplatmapMix(data, defaultSmoothness, splatControl, weight, splatColor, i.normal); // normal output is in 
                // tangent space
                //i.normal = float3(-i.normal.x, -i.normal.y, i.normal.z);
                //float3 worldNormal = UnityObjectToWorldNormal(i.normal);

                // Normal mapping
                //half3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                half3 worldNormal =
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, i.normal);

                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = _Color * splatColor;

                //float4 lightColor = (baseShadowColor * _ShadowStrength + finalColor * (1 - _ShadowStrength)) * (1 - inShadow) +
                //finalColor * inShadow;//(1 - _ShadowStrength)

                float shadowProduct = AngleBetween(worldNormal, _WorldSpaceLightPos0.xyz) / 3.151592;
                float originalShadowProduct = AngleBetween(i.worldNormal, _WorldSpaceLightPos0.xyz) / 3.151592;
                float inShadowSide = originalShadowProduct > 0.5; //0.4
                //return float4(worldNormal, 1);

                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                float groundAngle = saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, worldNormal) / (PI));
                //finalColor *= float4(float3(groundAngle, groundAngle, groundAngle), 1);

                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 horizontalViewDir = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 horizontalReflectedDir = normalize(float3(-_WorldSpaceLightPos0.x, 0, -_WorldSpaceLightPos0.z));
                float f = 1 - saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, viewDir) / (PI / 2));
                f = pow(f, 2);
                f = f * 1;
                
                float inShadowBool = inShadow < 0.6;

                //finalColor = float4(1,0,0,1);
                //saturate(1 - (1 - shadowProduct) * 2)
                float strechedShadowProduct = 1 - (1 - shadowProduct) * 1.2;
                float4 shadowColor = finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1);
                float4 lightColor = shadowColor * strechedShadowProduct +
                                    finalColor * (1 - strechedShadowProduct);

                lightColor = lightColor + float4(0.9, .9, 1, 0) * f * 1;
                //finalColor = finalColor + float4(0.9, .9, 1, 0) * f * 2;
                
                if (!inShadowSide)
                {
                    //return float4(1,0,0,1);
                    
                    //return finalColor;
                    //lightColor = lightColor + float4(0.9, .9, 1, 0) * f * 2;
                    
                    float shadeFade = inShadow;
                    //return shadeFade;
                    //return fixed4(1,0,0,1);
                    float4 fadedShadowColor = shadowColor * (1 - fadeValue) + lightColor * (fadeValue);
                    //return fadedShadowColor;
                    inShadow = (1 - fadeValue) * inShadow + (fadeValue) * 1;
                    //return inShadow;
                    STANDARD_FOG(fadedShadowColor * (1 - shadeFade) + lightColor * (shadeFade));
                }
                else
                {
                    //return strechedShadowProduct;
                    //return float4(0,1,0,1);
                    //return fadeValue;
                    //return fixed4(0,0,1,1);
                    float stretchedInShadow = saturate(inShadow * 2);
                    float shadowMerge = _LightShadowStrength * (1 - stretchedInShadow) + 1 * (stretchedInShadow);
                    float4 shadowSideColor = (lightColor * float4(shadowMerge, shadowMerge, shadowMerge, 1));
                    
                    strechedShadowProduct = saturate(1 * 2);
                    //float4 flatShadowColor = (finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1)) * strechedShadowProduct;
                    //flatShadowColor = flatShadowColor + float4(0.9, .9, 1, 0) * f * 1;
                    //STANDARD_FOG(lightColor);
                    
                    //return shadeFade;
                    float shadeFade = (1 - fadeValue) * inShadow;
                    inShadow = (1 - fadeValue) * inShadow + (fadeValue) * 1;
                    STANDARD_FOG(shadowColor);
                }
            }
            ENDCG
        }
    }
}
