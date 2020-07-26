//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/FlatShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1.1)) = 0.1
        _CrossFade ("CrossFade", float) = 0
        _EvenFade ("EvenFade", Range(0, 1)) = 0
        _OddFade ("EvenFade", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0
        _MidFogColor ("MidFogColor", Color) = (1,1,1,1)
        _EndFogColor ("EndFogColor", Color) = (1,1,1,1)
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

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                //float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _CrossFade;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                //UNITY_TRANSFER_LIGHTING(o, v.uv1); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
                //o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
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

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #include "UnityShadowLibrary.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "HelperCgincFiles/FogHelper.cginc"

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
                float3 normal : TEXCOORD3;
                float4 worldPos : TEXCOORD4;
            };

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //o._ShadowCoord = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o);
                //TRANSFER_SHADOW(o)
                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
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
            float4 _MidFogColor;
            float4 _EndFogColor;
            //float3 _WorldSpaceLightPos0;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float4 screenPos = ComputeScreenPos(i.pos);
                //return fixed4(unity_LODFade.x, unity_LODFade.x, unity_LODFade.x, 1);
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                {
                    //return fixed4(1,0,0,1);
                    clip(textureColor.a - _Threshold);
                }
                textureColor.a = 1;
                //return textureColor;

                //return textureColor;
                
                //float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPercentagePos));
                //return fixed4(depth,depth,depth,1);

                float2 screenPercentagePos = screenPos.xy / screenPos.w;
                float2 checkerboard = float2(sin(screenPercentagePos.x * 2 * 3.151592 * _CrossFade * 16),
                                             sin(screenPercentagePos.y * 2 * 3.151592 * _CrossFade * 9));
                float checkboardClip = checkerboard.x > 0 ^ checkerboard.y > 0; 

                //#ifdef LOD_FADE_CROSSFADE
                //    return fixed4(1,0,0,1);
                //#endif

                float flipLOD = abs(unity_LODFade.x);
                if (unity_LODFade.x > 0)
                    flipLOD = 1 - flipLOD;
                flipLOD = 1 - flipLOD;

                //return fixed4(abs(unity_LODFade.x), abs(unity_LODFade.x), abs(unity_LODFade.x), 1);

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
                //clip(checkboardClip * -1);

                //float inShadow = SHADOW_ATTENUATION(i);
                //float inShadow = 0;
                float inShadow = SHADOW_ATTENUATION(i);
                //inShadow = abs(inShadow);
                float4 finalColor = _Color;
                finalColor *= tex2D(_MainTex, i.uv);
                //return finalColor;
               // return finalColor;
                //finalColor = finalColor + float4(1,1,1,0) * pow(saturate(i.uv.y - 0.5), 2) * 0.45;
                //finalColor = finalColor + float4(1,1,1,0) * saturate(i.uv.y - 0.8) * 0.75;

                //float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                //float inShadowSide = shadowProduct > 0.5;

                float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                float inShadowSide = shadowProduct > 0.5;

                float4 shadowColor = finalColor * fixed4(.75, .75, .85, 1) * fixed4(.7, .7, .7, 1) * (_ShadowStrength) +
                           finalColor * (1 - _ShadowStrength);//(1 - _ShadowStrength)
                
                float inShadowBool = inShadow < 0.6;

                //return fixed4(inShadowSide, inShadowSide, inShadowSide, 1);

                //float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                //float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                //float fadeValue = UnityComputeShadowFade(fadeDistance);

                //float inShadowBool = inShadow < 0.96;
                //return fixed4(inShadow, inShadow, inShadow, 1);
                //return inShadow;

                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                //return fadeValue;

                if (!inShadowSide)
                {
                    if (!inShadowBool)
                    {
                        //return finalColor;
                        STANDARD_FOG(finalColor);
                    }
                    else
                    {
                        //return shadowColor * (1 - fadeValue) + finalColor * fadeValue;
                        //STANDARD_FOG(shadowColor * (1 - fadeValue) + finalColor * fadeValue);
                        
                        STANDARD_SHADOWSIDE_FOG(shadowColor * (1 - fadeValue) + finalColor * fadeValue);
                    }
                }
                else
                {
                    //return finalColor * fixed4(.75, .75, .85, 1) * fixed4(.7, .7, .7, 1) * (_ShadowStrength) +
                    //       finalColor * (1 - _ShadowStrength);
                    STANDARD_SHADOWSIDE_FOG(shadowColor);
                }
            }
            ENDCG
        }
    }
}
