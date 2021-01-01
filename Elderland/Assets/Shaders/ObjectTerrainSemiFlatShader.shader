//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/ObjectTerrainSemiFlatShader"
{
    Properties
    {
        _RepeatedTexture1 ("RepeatedTexture1", 2D) = "white" {}
        _RepeatedTexture1Scale ("RepeatedTexture1Scale", Range(0.1, 10)) = 1
        _RepeatedTexture1OffsetX ("RepeatedTexture1OffsetX", Range(0, 1)) = 0 
        _RepeatedTexture1OffsetY ("RepeatedTexture1OffsetY", Range(0, 1)) = 0 
        _RepeatedTexture1NormalMap ("RepeatedTexture1NormalMap", 2D) = "bump" {}
        _RepeatedNormalMapIntensity ("RepeatedNormalMapIntensity", Range(0, 1)) = 1

        _RepeatedTexture2 ("RepeatedTexture2", 2D) = "white" {}
        _RepeatedTexture2Scale ("RepeatedTexture2Scale", Range(0.1, 10)) = 1
        _RepeatedTexture2OffsetX ("RepeatedTexture2OffsetX", Range(0, 1)) = 0 
        _RepeatedTexture2OffsetY ("RepeatedTexture2OffsetY", Range(0, 1)) = 0 

        _RepeatedTextureBlend ("RepeatedFlatBlend", 2D) = "white" {}
        
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpMapIntensity ("BumpMapIntensity", Range(0, 1)) = 1
        _IsBumpMapConstantScale ("IsBumpMapConstantScale", Range(0, 1)) = 0
        _BumpMapConstantScale ("BumpMapConstantScale", float) = 1

        _Color ("Color", Color) = (1,1,1,1)
        _ColorMap ("ColorMap", 2D) = "white" {}

        _CrossFade ("CrossFade", float) = 0
        
        // Properties from Shading Helper
        _FlatShading ("FlatShading", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
        
        _HighlightStrength ("HightlightStrength", Range(0, 2)) = 1 
        _HighlightIntensity ("HighlightIntensity", Range(0, 2)) = 1
        _ReflectedIntensity ("ReflectedIntensity", Range(0, 3)) = 1

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 repeatedUV1 : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 repeatedUV1 : TEXCOORD0;
                SHADOW_COORDS(1)
            };

            sampler2D _RepeatedTexture1;
            float _CrossFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.repeatedUV1 = v.repeatedUV1;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 screenPos = ComputeScreenPos(i.pos);
                float2 screenPercentagePos = screenPos.xy / screenPos.w;
                float2 checkerboard = float2(sin(screenPercentagePos.x * 2 * 3.151592 * _CrossFade * 16),
                                             sin(screenPercentagePos.y * 2 * 3.151592 * _CrossFade * 9));
                float checkboardClip = checkerboard.x > 0 ^ checkerboard.y > 0; 

                float flipLOD = abs(unity_LODFade.x);
                if (unity_LODFade.x > 0)
                    flipLOD = 1 - flipLOD;
                flipLOD = 1 - flipLOD;

                //unity_LODFade.x at 1 is off.
                //unity_LODFade.x at 0 is on.

                //unity_LODFade.x at 1 is off.
                //unity_LODFade.x at 0 is on.
                
                int fadeSign = 1;
                if (unity_LODFade.x < 0)
                    fadeSign = -1;

                if ((checkboardClip * -1 < 0 && fadeSign == 1) || (checkboardClip * -1 >= 0 && fadeSign == -1))
                {
                    //clip(-1);
                    float rightLOD = (flipLOD - 0.5) * 2;
                    if (rightLOD < 0)
                        rightLOD = 0;

                    float evenClip = 0;
                    if (fadeSign == 1)
                    {    
                        evenClip = abs(checkerboard.x) > rightLOD && abs(checkerboard.y) > rightLOD;
                    }
                    else
                    {
                        evenClip = !(abs(checkerboard.x) > (1 - rightLOD) && abs(checkerboard.y) > (1 - rightLOD));
                    }
                    clip(evenClip * -1);
                }
                else
                {
                    //clip(-1);
                    float leftLOD = flipLOD * 2;
                    float oddClip = 0;
                    if (fadeSign == 1)
                    {
                        oddClip = abs(checkerboard.x) > leftLOD && abs(checkerboard.y) > leftLOD;
                    }
                    else
                    {
                        oddClip = !(abs(checkerboard.x) > (1 - leftLOD) && abs(checkerboard.y) > (1 - leftLOD));
                    }
                    clip(oddClip * -1);
                }


                float4 textureColor = (tex2D(_RepeatedTexture1, i.repeatedUV1));
                clip(textureColor.w - 1);

                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

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

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            
            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #include "TerrainSplatmapCommon.cginc"
            #include "/HelperCgincFiles/NormalMapHelper.cginc"
            #include "/HelperCgincFiles/ShadingHelper.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
            #include "/HelperCgincFiles/LODHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 repeatedUV1 : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 repeatedUV1 : TEXCOORD0;
                SHADOW_COORDS(1)
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            struct v2fInput
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD3;
                float4 tc : TEXCOORD1;
                float4 repeatedUV1 : TEXCOORD0;
                float4 repeatedUV2 : TEXCOORD2;
                float3 worldPos : TEXCOORD4;
                SHADOW_COORDS(10)
                float4 tangent : COLOR0;
                float4 originalUV : TEXCOORD5;
                float2 planeScale : COLOR1;
                float4 screenPos : TEXCOORD6;
                DECLARE_TANGENT_SPACE(7, 8, 9)
            };

            float _RepeatedTexture1Scale;
            float _RepeatedTexture1OffsetX;
            float _RepeatedTexture1OffsetY;

            float _RepeatedTexture2Scale;
            float _RepeatedTexture2OffsetX;
            float _RepeatedTexture2OffsetY;

            float _IsBumpMapConstantScale;
            float _BumpMapConstantScale;

            v2fInput vert (appdata_full v, float3 normal : NORMAL, float4 tangent : TANGENT)
            {
                v2fInput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);

                // UV calculation
                o.originalUV = v.texcoord;
                
                float tangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, v.tangent.xyz));
                float3 orthoTangent = cross(v.tangent.xyz, v.normal);
                float orthoTangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, orthoTangent));
                o.repeatedUV1 = 
                    (v.texcoord + float4(_RepeatedTexture1OffsetX, _RepeatedTexture1OffsetY, 0, 0)) *
                    float4(tangentScale * _RepeatedTexture1Scale, orthoTangentScale * _RepeatedTexture1Scale, 1, 1);
                o.repeatedUV2 = 
                    (v.texcoord + float4(_RepeatedTexture2OffsetX, _RepeatedTexture2OffsetY, 0, 0)) *
                    float4(tangentScale * _RepeatedTexture2Scale, orthoTangentScale * _RepeatedTexture2Scale, 1, 1);
                
                o.planeScale = float2(tangentScale, orthoTangentScale);
                
                // Shading Info
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                TRANSFER_SHADOW(o)
         
                // Normal calculation
                o.normal = v.normal;
                o.tangent = v.tangent;
                ComputeTangentSpace(normal, tangent, o.tanX1, o.tanX2, o.tanX3);

                // Texture info
                Input data;
                SplatmapVert(v, data);
                o.tc = data.tc;

                return o;
            }
            
            sampler2D _RepeatedTexture1;
            sampler2D _RepeatedTexture1NormalMap;
            sampler2D _RepeatedTexture2;
            sampler2D _RepeatedTextureBlend;
            float _RepeatedNormalMapIntensity;

            sampler2D _BumpMap;
            float _BumpMapIntensity;

            float4 _Color;
            sampler2D _ColorMap;

            float _CrossFade;
           
            float _WarmColorStrength;

            fixed4 frag(v2fInput i, fixed facingCamera : VFACE) : SV_Target
            {
                float3 normal = normalize(mul(unity_ObjectToWorld, i.normal));
                float3 tangent = normalize(mul(unity_ObjectToWorld, i.tangent.xyz));

                // Terrain texture:
                // Learned via standard-firstpass.shader in default shaders
                half4 splatControl;
                half weight;
                fixed4 mixedDiffuse;
                half4 defaultSmoothness = half4(0.05, 0.05, 0.05, 0.05);
                Input input = (Input)0;
                input.tc = i.tc;
                SplatmapMix(input, defaultSmoothness, splatControl, weight, mixedDiffuse, normal);

                // Normal mapping
                half3 tangentNormal;
                half3 worldNormal;
                half3 originalWorldNormal;

                if (_IsBumpMapConstantScale > 0.5)
                {
                    float4 repeatedBumpUV = 
                        (i.originalUV + float4(0, 0, 0, 0)) *
                        float4(i.planeScale.x * _BumpMapConstantScale, i.planeScale.y * _BumpMapConstantScale, 1, 1);

                    tangentNormal = UnpackNormal(tex2D(_BumpMap, repeatedBumpUV));
                    tangentNormal.y *= -1;
                    worldNormal = 
                        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
                    originalWorldNormal = 
                        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, half3(0,0,1));
                    worldNormal = worldNormal * _BumpMapIntensity + originalWorldNormal * (1 - _BumpMapIntensity);
                }
                else
                {
                    tangentNormal = UnpackNormal(tex2D(_BumpMap, i.originalUV));
                    tangentNormal.y *= -1;
                    worldNormal = 
                        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
                    originalWorldNormal = 
                        TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, half3(0,0,1));
                    worldNormal = worldNormal * _BumpMapIntensity + originalWorldNormal * (1 - _BumpMapIntensity);
                }

                tangentNormal = UnpackNormal(tex2D(_RepeatedTexture1NormalMap, i.repeatedUV1));
                tangentNormal.y *= -1;
                half3 worldRepeatedNormal1 = 
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
                worldRepeatedNormal1 = worldRepeatedNormal1 * _RepeatedNormalMapIntensity + originalWorldNormal * (1 - _RepeatedNormalMapIntensity);
                
                worldNormal = normalize(worldNormal + worldRepeatedNormal1);

                // Texture color calculation
                float4 repeatedTexture1Color = tex2D(_RepeatedTexture1, i.repeatedUV1);
                float4 repeatedTexture2Color = tex2D(_RepeatedTexture2, i.repeatedUV2);
                float repeatedTextureBlend = tex2D(_RepeatedTextureBlend, i.originalUV).r;
                float4 textureColor =
                    repeatedTexture1Color * repeatedTextureBlend +
                    repeatedTexture2Color * (1 - repeatedTextureBlend);

                textureColor *= _Color;
                float4 colorMapColor = tex2D(_ColorMap, i.originalUV);
                textureColor *= colorMapColor;

                ApplyDither(i.screenPos, _CrossFade);

                // Shadow calculation
                float inShadow = SHADOW_ATTENUATION(i);
                float4 localColor = textureColor;
                
                // Learned in AutoLight.cginc
                // Shadow Fade
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                // Shading and fog
                float4 shadedColor = Shade(worldNormal, i.worldPos, localColor, inShadow, fadeValue);
                STANDARD_FOG(shadedColor, worldNormal);
            }
            ENDCG
        }

        Pass
        {
            //Based on AutodeskInteractive additive forward pass structure in built in shaders.
            Tags
            {
                "LightMode"="ForwardAdd"
            }

            Blend One One
            ZWrite Off
            ZTest LEqual
            
            CGPROGRAM

            float _ApplyLight;

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster
            #include "/HelperCgincFiles/LightHelper.cginc"
            ENDCG
        }
    }
}
