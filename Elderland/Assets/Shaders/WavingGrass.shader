﻿//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Hidden/TerrainEngine/Details/WavingDoublePass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "FolliageHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv :TEXCOORD0;
                V2F_SHADOW_CASTER; //float4 pos : SV_POSITION thats it
                float worldDistance : TEXCOORD2;
            };

            sampler2D _MainTex;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;  
               
                //float3 alteredObjectVertex = WarpGrass(v.vertex, v.uv, normal);
                float3 alteredObjectVertex = WarpGrass(v.vertex, v.color.a, normal);
                //o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 40)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                    alteredObjectVertex = alteredObjectVertex;
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 40) / 35);
                    //o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth, 0, 0));
                    o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0));
                    //alteredObjectVertex = alteredObjectVertex - fixed4(0, limitedDepth, 0, 0);
                    alteredObjectVertex = alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0);
                    //
                    //o.pos = UnityObjectToClipPos(v.vertex * float4(1, worldDistance / 30,1,1));
                }
                v.vertex = float4(alteredObjectVertex, 1);
                o.worldDistance = worldDistance;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float _Threshold = 1;//0.675
                _Threshold -= i.worldDistance * 0.02;
                if (_Threshold < 0.2)
                    _Threshold = 0.2;
                float4 textureColor = (tex2D(_MainTex, i.uv));
                clip(textureColor.w - _Threshold);
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
                "Queue"="Geometry+200"
                "RenderType"="Grass"
                //"RenderType"="Transparent"    
                //"Queue"="Transparent"    
                // In Waving grass default shader.
            }

            //ZWrite On
            //LOD 200

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
            #include "FolliageHelper.cginc"
            
            //float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 _ShadowCoord : TEXCOORD1;
                float4 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR0; //messing with multiple grass textures.
                float depth : COLOR2;
                float worldDistance : TEXCOORD2;
                float3 objectPos : TEXCOORD3;
                float3 normal : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
            };  
             
            float4 _ShadowColor;
            sampler2D _ShadowMapTexture; 
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _WavingTint;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = WarpGrass(v.vertex, v.color.a, normal);
                o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                o.worldPos = worldPos.xyz;
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 40)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 40) / 35);
                    o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0));
                    //
                    //o.pos = UnityObjectToClipPos(v.vertex * float4(1, worldDistance / 30,1,1));
                }
                //o.pos = UnityObjectToClipPos(v.vertex);
                //o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                o.worldDistance = worldDistance;
                //In waving grass shader default
                o.uv = v.uv;
                //o.color = float4(1,0,0,1);
                o.color = v.color;
               
                o._ShadowCoord = ComputeScreenPos(o.pos);
                o.objectPos = v.vertex;
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                //return _WavingTint;
                //return i.color;
                //return fixed4(1,1,1,1)
                //return fixed4(i.color.xyz, 1);
                //float2 screenPercentagePos = i.screenPos.xy / i.screenPos.w;
                //return fixed4(screenPercentagePos.y, screenPercentagePos.y, screenPercentagePos.y, 1);
                //return i.color.a;
                //return fixed4(tex2D(_MainTex, screenPercentagePos).xyz * tex2D(_MainTex, screenPercentagePos).w, 1);

                //return fixed4(i.worldDistance / 50,0,0,1);
                float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;
                //return inShadow;

                float _Threshold = 1;//0.675
                _Threshold -= i.worldDistance * 0.02;
                if (_Threshold < 0.2)
                    _Threshold = 0.2;
                //_Threshold = 0.5;
                //_Threshold = 0;
                //_Threshold -= i.worldDistance * .04;
                /*if (i.depth * 35 > 40)
                {
                    _Threshold = saturate(_Threshold + i.depth);   
                }*/
                //return fixed4(i.depth * 35 / 50, 0, 0, 1);
                
                float4 textureColor = (tex2D(_MainTex, i.uv));
                //return textureColor.r;
                /*if (textureColor.a > _Threshold)
                {
                    if (textureColor.r > 0.5)
                    {
                        textureColor = float4(121.0 / 255, 152.0 / 255, 44.0 / 255, textureColor.a);
                    }
                    else
                    {
                        textureColor = float4(230.0 / 255, 181.0 / 255, 96.0 / 255, textureColor.a);
                    }
                }*/
                
                
                //float4(169.0 / 255, 223.0 / 255, 32.0 / 255, 1) original saturated
                //return textureColor;
                clip(textureColor.w - _Threshold);
                //return textureColor;
                //textureColor.w = 1;
                //return textureColor;

                // Hue
                float lightness = RGBLightness(textureColor);
                lightness = 0.5;
                float hue = RGBHue(textureColor.r, textureColor.g, textureColor.b);
                float saturation = RGBSat(textureColor.r, textureColor.g, textureColor.b);
                float4 hueTint = float4(HSLToRGB(hue, 0.7, .35), 1);//0.6
                hueTint = _WavingTint;
                //return float4(textureColor.rgb, 1);
                //return hueTint;
                //return hueTint;

                // Local hue
                //i.color.xyz,
                float3 color = i.color.xyz;
                float localHue = RGBHue(color.r, color.g, color.b);
                float4 localHueTint = fixed4(HSLToRGB(localHue, 0.4, lightness), 1);//0.5
                localHueTint = float4(i.color.xyz, 1);
                //return localHueTint;
                //return localHueTint;
                //localHueTint = float4(1,1,1,1);

                // Gradient factors
                float hueFactor = saturate(1 - i.objectPos.y + 0.25);//i.uv.y, i.objectPos.y
                
                
                hueFactor = 1 - saturate(1 - i.color.a * 1);
                //return hueFactor;
                //return i.uv.y;
                //return textureColor;
                //hueFactor = 0;
                //return hue / 360;// HUE is distance issue.
                //hueFactor = 1;
                if (i.worldDistance > 20)
                {
                    float limitedDepth = ((i.worldDistance - 20) / 20);
                    //hueFactor += limitedDepth;
                    if (hueFactor > 1)
                        hueFactor = 1;
                    //return fixed4(limitedDepth,limitedDepth,limitedDepth,1);
                }

                float lerpFactorTop = 
                    float4((hueFactor),
                           (hueFactor),
                           (hueFactor), 
                           1);

                float lerpFactorBottom = 
                    float4(1 - hueFactor,
                           1 - hueFactor,
                           1 - hueFactor, 
                           1);

                //x, y  is texture scale, zw is offset
                //via boglus in unity forums. "get the scale of object related to worldspace"
                float scaledUV = (i.uv.y * _MainTex_ST.y) + _MainTex_ST.w;
                scaledUV = i.objectPos.y;
                //caledUV = abs(unity_ObjectToWorld[2].x);//scale of y
                //scaledUV = abs(_MainTex_ST.w);
                //scaledUV = i.uv.y;
                //scaledUV = _MainTex_ST.w;
                //return fixed4(hueFactor, hueFactor, hueFactor, 1);
                //return fixed4(1 - i.uv.y, 1 - i.uv.y, 1 - i.uv.y, 1);
                //return fixed4(hue, hue, hue, 1);
                //return hueTint;

                //return localHueTint;

                //return lerpFactorBottom;
                //return localHueTint;
                //return hueFactor;
                fixed4 finalColor =
                        (fixed4(1, 1, 1, 1) * localHueTint * lerpFactorTop +
                        fixed4(1, 1, 1, 1) * hueTint * lerpFactorBottom);
                //return finalColor;
                //return finalColor;
                //return finalColor;
                //return hueTint;
                //return fixed4(i.color.xyz, 1);
                float tipHighlight = 0.1;
                //return i.color.a;
                if (i.color.a > tipHighlight)
                {
                    //return float4(1,0,0,1);
                    finalColor += float4(1,1,1,1) * (i.color.a - tipHighlight) / (1 - tipHighlight) * .1;
                }
                
                
                //o.normal actually is based on surface :>
                //return _WorldSpaceLightPos0;
                float groundAngle = saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, i.normal) / (PI));
                //finalColor *= float4(float3(groundAngle, groundAngle, groundAngle), 1);
                //return fixed4(groundAngle, groundAngle, groundAngle, 1);
                //return finalColor;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 horizontalViewDir = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 horizontalReflectedDir = normalize(float3(-_WorldSpaceLightPos0.x, 0, -_WorldSpaceLightPos0.z));
                float f = 1 - saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, viewDir) / (PI / 2));
                f = pow(f, 2);
                f = f * 1;
                //return f;
                
                // Learned in AutoLight.cginc
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                //float viewUpAngle = AngleBetween(viewDir, float3(0, 1, 0)); //working here rn

                float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                float inShadowSide = shadowProduct > 0.5; //0.4

                float strechedShadowProduct = saturate(shadowProduct * 2);
                float _LightShadowStrength = 0.25;
                float4 shadowColor = finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1);
                float4 lightColor = shadowColor * strechedShadowProduct +
                                    finalColor * (1 - strechedShadowProduct);
                lightColor = lightColor + float4(0.9, .9, 1, 0) * f * 1;

                float inShadowBool = inShadow < 0.6;
                //return finalColor;
                //return shadowColor;

                if (!inShadowSide)
                {
                    //if (!inShadowBool)
                    //{
                        //return finalColor + float4(0.9, .9, 1, 0) * f * 2;
                        float shadeFade = inShadow;
                        //return shadeFade;
                        //return fixed4(1,0,0,1);
                        float4 fadedShadowColor = shadowColor * (1 - fadeValue) + lightColor * (fadeValue);
                        //return fadedShadowColor;
                        inShadow = (1 - fadeValue) * inShadow + (fadeValue) * 1;
                        STANDARD_FOG(fadedShadowColor * (1 - shadeFade) + lightColor * (shadeFade));

                        STANDARD_FOG(shadowColor + float4(0.9, .9, 1, 0) * f * 1);
                        return shadowColor + float4(0.9, .9, 1, 0) * f * 1;
                    //}
                    //else
                    /*{
                        strechedShadowProduct = saturate(1 * 2);
                        float4 flatShadowColor = (finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1)) * strechedShadowProduct;
                        STANDARD_FOG((flatShadowColor) + float4(0.9, .9, 1, 0) * f * 1);
                        return (flatShadowColor) + float4(0.9, .9, 1, 0) * f * 1;
                        //return inShadow;
                        //float4 mergeColor = finalColor * (inShadow) + 
                        //(finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue)) * (1 - inShadow);
                        float4 lightColor = finalColor + float4(0.9, .9, 1, 0) * f * 2;
                        float4 shadowColor2 = finalColor * float4(.5, .5, .5, 1);
                        float4 compositeColor = shadowColor2 * (1 - inShadow) + lightColor * (inShadow);
                        //return inShadow;
                        //return compositeColor;
                        //return (finalColor * fixed4(.5, .5, .5, 1) * (1 - fadeValue) + finalColor * (fadeValue)) * (1 - inShadow);
                        //STANDARD_FOG(mergeColor);
                    }*/
                }
                else
                {
                    inShadow = (1 - fadeValue) * inShadow + (fadeValue) * 1;
                    STANDARD_FOG(shadowColor);
                    strechedShadowProduct = saturate(1 * 2);
                    float4 flatShadowColor = (finalColor * float4(_LightShadowStrength, _LightShadowStrength, _LightShadowStrength, 1)) * strechedShadowProduct;
                    STANDARD_FOG((flatShadowColor) + float4(0.9, .9, 1, 0) * f * 1);
                    return (flatShadowColor) + float4(0.9, .9, 1, 0) * f * 1;
                    //return finalColor * fixed4(.5, .5, .5, 1);
                }
            }
            ENDCG
        }
    }
}
