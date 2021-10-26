Shader "Custom/World/Folliage/SubsurfaceVegetation"
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

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        // From CharacterEffectsHelper.cginc
        _ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        LOD 400
        Cull Off

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
            #pragma fragment semiFlatVeg//semiFlatFrag
            #pragma multi_compile_fwdbase

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderBase.cginc"

            fixed4 semiFlatVeg(customV2F i, fixed facingCamera : VFACE) : SV_Target
            {
                // Normal mapping
                half3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));
                tangentNormal.y *= -1;
                half3 worldNormal = 
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
                half3 originalWorldNormal = 
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, half3(0,0,1));

                float compositeBumpMapIntensity = _BumpMapIntensity;
                float smoothnessMapIntensity = tex2D(_SmoothnessMap, i.uv).r;
                if (_ClampSmoothnessMap > 0.5)
                {
                    if (smoothnessMapIntensity  > 0.01)
                    {
                        compositeBumpMapIntensity = 0;
                    }
                }

                worldNormal = worldNormal * compositeBumpMapIntensity + originalWorldNormal * (1 - compositeBumpMapIntensity);
                
                // Reflections
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); 
                float3 reflectionDirection = reflect(-viewDir, worldNormal);
                float4 reflectedColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDirection);

                // Fresnel value
                float fresnelValue = Fresnel(worldNormal, viewDir);

                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);
                
                ApplyDither(i.screenPos, _CrossFade);

                ApplyCharacterFade(i.objectPos, _WorldMaxHeight);

                float inShadow = SHADOW_ATTENUATION(i);

                // We know Quad is facing the camera, need to swap rendering based on if looking toward
                // light or towards shadowside.   
                float normalFacingCamera = AngleBetween(viewDir, i.worldNormal) / PI; 
                if (normalFacingCamera > 0.5)
                {
                    normalFacingCamera = 1;
                }
                else
                {
                    normalFacingCamera = -1;
                }
                //return fixed4(normalFacingCamera, normalFacingCamera, normalFacingCamera, 1);

                if (AngleBetween(_WorldSpaceLightPos0.xyz * normalFacingCamera, i.worldNormal) / PI < 0.5)
                {
                    return float4(
                        inShadow * 0.5,
                        inShadow,
                        inShadow * 0.75,
                        1);
                }
                else
                {
                    // shadows work by default with cull off.
                    return float4(
                        inShadow,
                        inShadow,
                        inShadow,
                        1);
                }

                float4 localColor = _Color;
                localColor *= textureColor;

                // Learned in AutoLight.cginc
                // Shadow Fade
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = CompositeShadeFade(inShadow, fadeDistance);

                float specular = tex2D(_SpecularMap, i.uv).r;
                float4 shadedColor = Shade(worldNormal, i.worldPos, localColor, inShadow, fadeValue, specular);
                float4 fresnelColor = reflectedColor * fresnelValue + shadedColor * (1 - fresnelValue);
                
                float areaSunAngle =
                    AngleBetween(-_WorldSpaceLightPos0.xyz, reflectionDirection) / (3.151592);
                if (areaSunAngle < 0.1)
                    areaSunAngle = 0;
                areaSunAngle = pow(areaSunAngle, 7) * 0.6;
                areaSunAngle *= inShadow;
                areaSunAngle *= smoothnessMapIntensity * _Smoothness;

                float4 returnColor =
                    fresnelColor * (smoothnessMapIntensity * _Smoothness) +
                    shadedColor * (1 - smoothnessMapIntensity * _Smoothness);
                    
                if (smoothnessMapIntensity * _Smoothness > 1)
                {
                
                    float excessSmoothness = (smoothnessMapIntensity * _Smoothness) - 1;
                    //return float4(excessSmoothness, excessSmoothness, excessSmoothness, 1);
                    returnColor =
                        reflectedColor * (excessSmoothness) +
                        fresnelColor * (1 - (excessSmoothness));
                }

                returnColor =
                    returnColor * (1 - areaSunAngle) + float4(1, 1, 1, 1) * areaSunAngle;
                
                //returnColor =
                //    returnColor * (1 - areaSunAngle) + _LightColor0 * areaSunAngle;

                STANDARD_FOG(returnColor, worldNormal);
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
            Cull Off

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
    }
}
