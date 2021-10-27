//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in deferred shaders.
//References: 
//Standard.shader
//UnityStandardCore.cginc
//UnityStandardShadow.cginc
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

        // Custom, partial fragments from Internal-DeferredShading.shader
        // Pass 1: Lighting Pass
        Pass
        {
            Name "SemiFlatShader"
            Tags 
            { 
                "LightMode"="Deferred"
            }

            CGPROGRAM
            #pragma target 3.0
            #pragma multi_compile_lightpass
            #pragma multi_compile ___ UNITY_HDR_ON
            #pragma exclude_renderers nomrt

            #include "UnityCG.cginc"
            #include "UnityDeferredLibrary.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardUtils.cginc"
            #include "UnityGBuffer.cginc"
            #include "UnityStandardBRDF.cginc"

            #pragma vertex VolumeVert
            #pragma fragment VolumeFrag

            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;

            struct unity_v2f_deferred_custom {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };

            // Provided as is. Made changes to fit struct requirements.
            void UnityDeferredCalculateLightParamsCustom (
                unity_v2f_deferred_custom i,
                out float3 outWorldPos,
                out float2 outUV,
                out half3 outLightDir,
                out float outAtten,
                out float outFadeDist)
            {
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float2 uv = i.uv.xy / i.uv.w;

                // read depth and reconstruct world position
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                depth = Linear01Depth (depth);
                float4 vpos = float4(i.ray * depth,1);
                float3 wpos = mul (unity_CameraToWorld, vpos).xyz;

                float fadeDist = UnityComputeShadowFadeDistance(wpos, vpos.z);

                // spot light case
                #if defined (SPOT)
                    float3 tolight = _LightPos.xyz - wpos;
                    half3 lightDir = normalize (tolight);

                    float4 uvCookie = mul (unity_WorldToLight, float4(wpos,1));
                    // negative bias because http://aras-p.info/blog/2010/01/07/screenspace-vs-mip-mapping/
                    float atten = tex2Dbias (_LightTexture0, float4(uvCookie.xy / uvCookie.w, 0, -8)).w;
                    atten *= uvCookie.w < 0;
                    float att = dot(tolight, tolight) * _LightPos.w;
                    atten *= tex2D (_LightTextureB0, att.rr).r;

                    atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

                // directional light case
                #elif defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
                    half3 lightDir = -_LightDir.xyz;
                    float atten = 1.0;

                    atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

                    #if defined (DIRECTIONAL_COOKIE)
                    atten *= tex2Dbias (_LightTexture0, float4(mul(unity_WorldToLight, half4(wpos,1)).xy, 0, -8)).w;
                    #endif //DIRECTIONAL_COOKIE

                // point light case
                #elif defined (POINT) || defined (POINT_COOKIE)
                    float3 tolight = wpos - _LightPos.xyz;
                    half3 lightDir = -normalize (tolight);

                    float att = dot(tolight, tolight) * _LightPos.w;
                    float atten = tex2D (_LightTextureB0, att.rr).r;

                    atten *= UnityDeferredComputeShadow (tolight, fadeDist, uv);

                    #if defined (POINT_COOKIE)
                    atten *= texCUBEbias(_LightTexture0, float4(mul(unity_WorldToLight, half4(wpos,1)).xyz, -8)).w;
                    #endif //POINT_COOKIE
                #else
                    half3 lightDir = 0;
                    float atten = 0;
                #endif

                outWorldPos = wpos;
                outUV = uv;
                outLightDir = lightDir;
                outAtten = atten;
                outFadeDist = fadeDist;
            }

            unity_v2f_deferred_custom VolumeVert (float4 vertex : POSITION, float3 normal : NORMAL)
            {
                unity_v2f_deferred_custom o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uv = ComputeScreenPos(o.pos);
                o.ray = UnityObjectToViewPos(vertex) * float3(-1,-1,1);

                // normal contains a ray pointing from the camera to one of near plane's
                // corners in camera space when we are drawing a full screen quad.
                // Otherwise, when rendering 3D shapes, use the ray calculated here.
                o.ray = lerp(o.ray, normal, _LightAsQuad);

                // Need to calculate light direction on the screen space, that way we know what
                // direction to raymarch in. Then will use normals and depth to calculate occlusion.
                // From what I can tell, there are no conversion here for world space and object space in this pass.

                return o;
            }

            fixed4 VolumeFrag(unity_v2f_deferred_custom i, fixed facingCamera : VFACE) : SV_Target
            {
                // Overlay defined.
                //return fixed4(1,0,0,1);

                float2 scaledScreenPos = i.uv.xy / i.uv.w;

                // Normals defined.
                float3 worldNormal = normalize((float3)tex2D(_CameraGBufferTexture2, scaledScreenPos).xyz * 2 - 1);
                //return fixed4(worldNormal.xyz, 1);

                // Depth defined.
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, scaledScreenPos));
                //return fixed4(depth, depth, depth, 1);

                // Shadow 
                float3 wpos;
                float2 uv;
                float atten, fadeDist;
                UnityLight light; // light.dir is world space light direction (same for all values in each pixel)
                UNITY_INITIALIZE_OUTPUT(UnityLight, light);
                UnityDeferredCalculateLightParamsCustom (i, wpos, uv, light.dir, atten, fadeDist);

                float3 worldDeltaPos = wpos + _WorldSpaceLightPos0.xyz;

                float4 camPos = mul(unity_WorldToCamera, wpos); // changing based on camera.
                float4 camDeltaPos = mul(unity_WorldToCamera, wpos + 500 * light.dir);
                float2 camDelta = normalize((camDeltaPos - camPos).xy); // correct delta values based on camera
                return fixed4(camDelta.x, camDelta.y, 0, 1);
             
                return fixed4(atten, atten, atten, 1);
            }
            ENDCG
        }

        // Provided as is, from Internal-DeferredShading.shader
        // Pass 2: Final decode pass.
        // Used only with HDR off, to decode the logarithmic buffer into the main RT
        Pass {
            ZTest Always Cull Off ZWrite Off
            Stencil {
                ref [_StencilNonBackground]
                readmask [_StencilNonBackground]
                // Normally just comp would be sufficient, but there's a bug and only front face stencil state is set (case 583207)
                compback equal
                compfront equal
            }

        CGPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag
        #pragma exclude_renderers nomrt

        #include "UnityCG.cginc"

        sampler2D _LightBuffer;
        struct v2f {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
        };

        v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(vertex);
            o.texcoord = texcoord.xy;
        #ifdef UNITY_SINGLE_PASS_STEREO
            o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0f);
        #endif
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            return -log2(tex2D(_LightBuffer, i.texcoord));
        }
        ENDCG
        }
    }
}
