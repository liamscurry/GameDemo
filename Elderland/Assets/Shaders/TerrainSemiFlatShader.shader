//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/TerrainSemiFlatShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

                o.normal = normal;

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

                float4 screenPos = ComputeScreenPos(i.pos);
                float2 screenPercentagePos = screenPos.xy / screenPos.w;
                float2 checkerboard = float2(sin(screenPercentagePos.x * 2 * 3.151592 * _CrossFade * 16),
                                             sin(screenPercentagePos.y * 2 * 3.151592 * _CrossFade * 9));
                float checkboardClip = checkerboard.x > 0 ^ checkerboard.y > 0; 

                //return fixed4(screenPercentagePos.x, screenPercentagePos.x, screenPercentagePos.x, 1);

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
            // In Specular-Base.shader and other terrain shaders.
            #pragma multi_compile_local __ _ALPHATEST_ON

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"
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
            };

            v2f vert (appdata_full v, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //o._ShadowCoord = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.texcoord;//v.uv
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                Input data;
                SplatmapVert(v, data);
                o.tc = data.tc;

                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
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
                SplatmapMix(data, splatColor, weight, splatColor, i.normal);

                //return splatColor;
                //Terrain texture:
                
                //float4 textureColor = tex2D(_MainTex, i.uv);
                //if (textureColor.a < _Threshold)
                //    clip(textureColor.a - _Threshold);

                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = _Color * splatColor;

                //float4 shadowColor = (baseShadowColor * _ShadowStrength + finalColor * (1 - _ShadowStrength)) * (1 - inShadow) +
                //finalColor * inShadow;//(1 - _ShadowStrength)

                float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;//i.normal
                float inShadowSide = shadowProduct > 0.4; //was 0.5

                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                float groundAngle = saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, i.normal) / (PI));
                //finalColor *= float4(float3(groundAngle, groundAngle, groundAngle), 1);

                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 horizontalViewDir = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 horizontalReflectedDir = normalize(float3(-_WorldSpaceLightPos0.x, 0, -_WorldSpaceLightPos0.z));
                float f = 1 - saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, viewDir) / (PI / 2));
                f = pow(f, 2);
                f = f * 1;
                
                float inShadowBool = inShadow < 0.6;

                //return fadeValue;
                //return inShadowSide;
                //return inShadow * (1 - fadeValue);
                //finalColor = float4(1,0,0,1);
                float strechedShadowProduct = saturate(shadowProduct * 2);
                float4 shadowColor = (finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1)) * strechedShadowProduct +
                                     finalColor * (1 - strechedShadowProduct);
                //return shadowColor;
                //return finalColor;
                //if (!inShadowSide)
                {
                    //finalColor = finalColor + float4(0.9, .9, 1, 0) * f * 2;
                    
                    if (!inShadowBool)
                    {
                        //return float4(1,0,0,1);
                        
                        //return finalColor;
                        //shadowColor = shadowColor + float4(0.9, .9, 1, 0) * f * 2;
                        STANDARD_FOG(shadowColor + float4(0.9, .9, 1, 0) * f * 1);
                        STANDARD_FOG(finalColor);
                    }
                    else
                    {
                        //return strechedShadowProduct;
                        //return float4(0,1,0,1);
                        //return fadeValue;
                        float stretchedInShadow = saturate(inShadow * 2);
                        float shadowMerge = _LightShadowStrength * (1 - stretchedInShadow) + 1 * (stretchedInShadow);
                        float4 shadowSideColor = (shadowColor * float4(shadowMerge, shadowMerge, shadowMerge, 1));
                        
                        strechedShadowProduct = saturate(1 * 2);
                        float4 flatShadowColor = (finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1)) * strechedShadowProduct;
                        //flatShadowColor = flatShadowColor + float4(0.9, .9, 1, 0) * f * 1;
                        //STANDARD_FOG(shadowColor);
                        STANDARD_FOG((flatShadowColor * (1 - fadeValue) + shadowColor * fadeValue) + float4(0.9, .9, 1, 0) * f * 1);
                        STANDARD_FOG(shadowSideColor * (1 - fadeValue) + shadowColor * fadeValue);
                        //return float4(1,0,0,1) * float4(.5,0,0,1) * (1 - fadeValue) + float4(1,0,0,1) * (fadeValue);
                        //return inShadow;
                        float4 mergeColor = finalColor * (_LightShadowStrength) + 
                        (finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue)) * (1 - _LightShadowStrength);
                        //return fadeValue;
                        //STANDARD_FOG(finalColor * (fadeValue) + (finalColor * fixed4(.5, .5, .5, 1)) * (1 - fadeValue));
                        STANDARD_FOG(shadowColor * (1 - fadeValue) + finalColor * (fadeValue));
                        //STANDARD_FOG(mergeColor);
                    }
                }
                /*else
                {
                    STANDARD_FOG(shadowColor);
                    //return float4(1,0,0,1) * float4(.5,0,0,1);
                    //return fadeValue;
                    //return finalColor * fixed4(.5, .5, .5, 1);
                    //return finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue);
                    STANDARD_FOG(shadowColor * (1 - fadeValue) + finalColor * (fadeValue));
                }*/
            }
            ENDCG
        }
    }
}
