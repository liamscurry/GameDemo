// Based on vertex/fragment shader examples on unity documentation.

Shader "Custom/StillWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Skybox ("Skybox", CUBE) = "" {}
        _SkyboxIntensity ("SkyboxIntensity", Range(0, 1)) = 0.5
        _WaterBedColor ("WaterBedColor", Color) = (0,0,0,0)
        _WaterLineThreshold ("WaterLineThreshold", Range(0, 1)) = 0
        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 0
        _Clearness ("Clearness", Range(0, 1)) = 1
        _ReflectedIntensity ("ReflectedIntensity", Range(0, 1)) = 1
        _EnableFog ("EnableFog", Range(0.0, 1.0)) = 0.0
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
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

        GrabPass { }
        Pass
        {
            Tags { "RenderType"="Geometry+20" "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fwdbase

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "Color.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
            #include "/HelperCgincFiles/LODHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1)
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 ray : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float4 grabPos : TEXCOORD6;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WaterBedColor;
            sampler2D _CameraDepthTexture;
            sampler2D _GrabTexture;
            samplerCUBE _Skybox;
            float _SkyboxIntensity;
            float _WaterLineThreshold;
            float _WarmColorStrength;
            float _Clearness;
            float _ReflectedIntensity;
            float _EnableFog;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                //float4 alteredVertex = v.vertex;
                //alteredVertex.y = v.vertex.y;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW(o)
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.normal = UnityObjectToWorldNormal(normal);
                // From grab pass manual.
                o.grabPos = ComputeGrabScreenPos(o.pos);
                
                return o;
            }

            fixed4 frag (v2f i, float3 normal : NORMAL) : SV_Target
            {
                float3 calculatedNormal = UnityObjectToWorldNormal(normal);//1.4// * .6
                float3 alteredNormal = normalize(
                    i.normal * 1 +
                    sin(cos(_Time.y * 2.3 + i.worldPos.x * .3)) * float3(.0035,0,0) +
                    sin(3* (_Time.y * 1.3 + i.worldPos.z) * .3) * float3(0,0,.0075))
                    ;//sin(.1* (_Time.y * 1.3 + i.worldPos.x * i.worldPos.z * .01)) * float3(.003,0,.0035)
                    //
                //+ sin(_Time.y + v.vertex.z * 50) * float3(0,0,.2)
                // Learned in vertex/fragment shader examples in unity docs.
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                //float3 worldNormal = alteredNormal;
                
                float3 reflectionDirection = reflect(-viewDir, alteredNormal);
                fixed4 color = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDirection);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 existingWorldPosition = mul(unity_CameraToWorld, viewPosition);
                float4 newWorldPosition = i.worldPos;

                float waterDepthFactor = saturate(pow(abs(existingWorldPosition.y - newWorldPosition.y) / 10, .5) + .1);
                //return waterDepthFactor;
                float sunAngle = 0;
                float areaSunAngle = 0;
                if (_WorldSpaceLightPos0.w < 0.5)
                {
                    areaSunAngle =
                        AngleBetween(-_WorldSpaceLightPos0.xyz, reflectionDirection) / (3.151592);
                    areaSunAngle = pow(areaSunAngle, 6) * 1.15f;

                    if (length(existingWorldPosition - newWorldPosition) /
                        (2 + sin(_Time.y + i.worldPos.x)) < _WaterLineThreshold)
                    {
                        sunAngle = areaSunAngle * 1;
                        //return float4(1,0,0,1);
                    }
                }
                
                // From grab pass manual.
                float grabDepthFactor = saturate(pow(abs(existingWorldPosition.y - newWorldPosition.y) / 10, 2));
                //return grabDepthFactor;
                float4 grabPosOffset = float4(sin(_Time.y + i.worldPos.x * .5) * grabDepthFactor * .3,
                                              sin(_Time.y * .7 + i.worldPos.z * .8) * grabDepthFactor * .3,
                                              0,
                                              0);
                float4 existingColor = tex2Dproj(_GrabTexture, i.grabPos + grabPosOffset);

                
                float fresnelAngle = saturate(pow(AngleBetween(alteredNormal, viewDir) / 3.151592 * 2, 6.5));
                fresnelAngle += 0.25f;
  
                float4 waterBedCompositeColor =
                    existingColor * (1 - waterDepthFactor) + _WaterBedColor * waterDepthFactor;

                waterBedCompositeColor = 
                    waterBedCompositeColor * _Clearness + _WaterBedColor * (1 - _Clearness);

                float4 skyboxColor = texCUBE(_Skybox, reflectionDirection);

                color = 
                    skyboxColor * _SkyboxIntensity + color * (1 - _SkyboxIntensity);

                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = color * (fresnelAngle) + waterBedCompositeColor * (1 - fresnelAngle);
                finalColor +=
                    float4(areaSunAngle, areaSunAngle, areaSunAngle, 0) * 0.6f * inShadow;
                finalColor = saturate(finalColor);
                
                //finalColor =
                //    finalColor * (1 - sunAngle) + float4(1, 1, 1, 1) * sunAngle;
                    
                
                if (_EnableFog)
                {
                    STANDARD_FOG_TEMPERATURE(finalColor, _WarmColorStrength);
                }
                else
                {
                    return finalColor;
                }
            }
            ENDCG
        }
    }
}
