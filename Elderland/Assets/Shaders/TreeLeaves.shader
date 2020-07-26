//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders and shadow example on vertex and fragment shader examples in docs.
Shader "Custom/TreeLeaves"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _CrossFade ("CrossFade", float) = 0
        _FringeIntensity ("FringeIntensity", Range(0, 1)) = 1
        _MidFogColor ("MidFogColor", Color) = (1,1,1,1)
        _EndFogColor ("EndFogColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        //Cull off

        Pass
        {
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "FolliageHelper.cginc"

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
            float _Threshold;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = WarpFolliage(v.vertex, v.uv, normal);
                o.pos = UnityObjectToClipPos(alteredObjectVertex);
                o.uv = v.uv;
                TRANSFER_SHADOW(o) //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
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


                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);
                //clip(textureColor.w - 1);
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
            #include "FolliageHelper.cginc"
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
            };

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = WarpFolliage(v.vertex, v.uv, normal);
                o.pos = UnityObjectToClipPos(alteredObjectVertex);
                o.worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                //o.screenPos = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            float _Threshold;
            float _CrossFade;
            float _FringeIntensity;
            float4 _MidFogColor;
            float4 _EndFogColor;
            //float3 _WorldSpaceLightPos0;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
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
                

                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                //float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;
                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = _Color;
                
                //return fixed4(inShadow, inShadow, inShadow, 1);

                if (_FringeIntensity < 0.5)
                {
                    finalColor = finalColor + float4(1,1,1,0) * pow(saturate(i.uv.y - 0.5 * (1 - _FringeIntensity)), 2) * 0.45;
                    finalColor = finalColor + float4(1,1,1,0) * saturate(i.uv.y - 0.8) * 0.75 * (1 - 6 * _FringeIntensity);
                }
                float verticalProduct = sin(i.worldPos.y * .75) * (1 - _FringeIntensity * 1.3) * .5;
                if (verticalProduct < 0)
                    verticalProduct = 0;
                float horizontalProduct = sin(i.worldPos.x * .25) * (1 - _FringeIntensity * .1);
                //finalColor = finalColor + float4(.2, .2, 0, 1) * verticalProduct + float4(.1, 0, .1, 1) * horizontalProduct;

                //float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                //float inShadowSide = shadowProduct > 0.5;
                //return fixed4(inShadowSide, inShadowSide, inShadowSide, 1);

                //return fixed4(inShadow, inShadow, inShadow, 1);

                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                //return fixed4(inShadow, inShadow, inShadow, 1);

                if (inShadow > 0.3)
                {
                    //return finalColor;
                    STANDARD_FOG(finalColor);
                }
                else
                {
                    //return finalColor * fixed4(.85, .75, .75, 1) * (1 - fadeValue) + finalColor * (fadeValue);
                    STANDARD_FOG(finalColor * fixed4(1, .87, .625, 1) * (1 - fadeValue) + finalColor * (fadeValue));
                }
            }
            ENDCG
        }
    }
}
