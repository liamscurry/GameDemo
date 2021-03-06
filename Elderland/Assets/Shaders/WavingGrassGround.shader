﻿//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders, UnityShadowLibrary.cginc, AutoLight.cginc and shadow example on vertex and fragment shader examples in docs.
Shader "Custom/WavingGrassGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _MidFogColor ("MidFogColor", Color) = (1,1,1,1)
        _EndFogColor ("EndFogColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Cull off

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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) //float4 pos : SV_POSITION thats it
                //float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _CrossFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                TRANSFER_SHADOW(o) //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
                //o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 textureColor = (tex2D(_MainTex, i.uv));
                clip(textureColor.w - 1);
                return 0;
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

            //ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #include "UnityShadowLibrary.cginc"
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
                float4 worldPos : TEXCOORD2;
                float3 normal : NORMAL;
            };

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //o._ShadowCoord = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            float _Threshold;
            float4 _MidFogColor;
            float4 _EndFogColor;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = _Color;

                //float4 shadowColor = (baseShadowColor * _ShadowStrength + finalColor * (1 - _ShadowStrength)) * (1 - inShadow) +
                //finalColor * inShadow;//(1 - _ShadowStrength)

                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                float groundAngle = saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, i.normal) / (PI));
                finalColor *= float4(float3(groundAngle, groundAngle, groundAngle), 1);

                float inShadowBool = inShadow < 0.6;


                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 horizontalViewDir = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 horizontalReflectedDir = normalize(float3(-_WorldSpaceLightPos0.x, 0, -_WorldSpaceLightPos0.z));
                float f = 1 - saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, viewDir) / (PI / 2));
                f = pow(f, 2);
                //return f;
                
                if (inShadow)
                {
                    if (!inShadowBool)
                    {
                        //return finalColor;
                        STANDARD_FOG(finalColor + float4(0.9, .9, 1, 0) * f * 2);// + float4(0.9, .9, 1, 0) * f * .7
                    }
                    else
                    {
                        //return inShadow;
                        float4 mergeColor = finalColor * (inShadow) + 
                        (finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue)) * (1 - inShadow);
                        STANDARD_FOG(mergeColor);
                    }
                }
                else
                {
                    //return finalColor * fixed4(.5, .5, .5, 1);
                    //return finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue);
                    STANDARD_FOG(finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue));
                }
            }
            ENDCG
        }
    }
}
