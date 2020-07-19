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
        _BumpMap ("BumpMap", 2D) = "white" {}
        _SecondaryBumpMap ("SecondaryBumpMap", 2D) = "white" {}
        _BlendMap ("BlendMap", 2D) = "white" {}
        _ColorMap ("ColorMap", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BumpMapScale ("BumpMapScale", float) = 1
        _ColorMapType ("ColorMapType", Range(0,1)) = 0
        _ColorMapUpperThreshold ("ColorMapUpperThreshold", Range(0, 2)) = 1
        _ColorMapLowerThreshold ("ColorMapLowerThreshold", Range(0, 2)) = 0
        _UVOffsetX ("UVOffsetX", Range(0, 1)) = 0 
        _UVOffsetY ("UVOffsetY", Range(0, 1)) = 0 
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _CrossFade ("CrossFade", float) = 0
        _EvenFade ("EvenFade", Range(0, 1)) = 0
        _OddFade ("EvenFade", Range(0, 1)) = 0
        _ShadowStrength ("ShadowStrength", Range(0, 2)) = 0
        _LightShadowStrength ("LightShadowStrength", Range(0, 1)) = 0
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

            //ZWrite Off

            //Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            //#pragma vertex vert
            //#pragma fragment frag
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
            #include "TerrainSplatmapCommon.cginc"
            
            // Angle between working
            float AngleBetween(float3 u, float3 v)
            {
                float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
                float denominator = length(u) * length(v);
                return acos(numerator / denominator);
            }

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

            struct v2fInput
            {
                //float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD2;
                float4 tc : TEXCOORD1;
                float4 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD3;
                SHADOW_COORDS(4)
                float4 tangent : COLOR0;
                float4 originalUV : TEXCOORD5;
                float2 planeScale : COLOR1;
            };

            float _BumpMapScale;
            float _UVOffsetX;
            float _UVOffsetY;

            //v2f vert (appdata v, float3 normal : NORMAL)
            v2fInput vert (appdata_full v)
            {
                /*v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                //o._ShadowCoord = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                */
                v.texcoord += float4(_UVOffsetX, _UVOffsetY, 0, 0);
                v2fInput o;
                o.originalUV = v.texcoord;
                o.pos = UnityObjectToClipPos(v.vertex);
                float tangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, v.tangent.xyz));
                float3 orthoTangent = cross(v.tangent.xyz, v.normal);
                float orthoTangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, orthoTangent));
                o.uv = v.texcoord * float4(tangentScale * _BumpMapScale, orthoTangentScale * _BumpMapScale, 1, 1);//float4(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].z, 1); 
                o.planeScale = float2(tangentScale, orthoTangentScale);
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                TRANSFER_SHADOW(o)

                Input data;
                SplatmapVert(v, data);
                o.tc = data.tc;
                //o.uv = v.uv;
                //return o;
                return o;
            }
            
            float4 _Color;
            //sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _SecondaryBumpMap;
            sampler2D _BlendMap;
            sampler2D _ColorMap;
            float _ColorMapType;
            float _ColorMapUpperThreshold;
            float _ColorMapLowerThreshold;
            float _Threshold;
            float _CrossFade;
            float _EvenFade;
            float _OddFade;
            sampler2D _CameraDepthTexture;
            float _ShadowStrength;
            float _LightShadowStrength;
            //sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            //float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
            //float3 _WorldSpaceLightPos0;

            //fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
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

                // Normal from terrain texture (From rendering systems project character helper):
                float3 unpackedNormal = UnpackNormal(tex2D(_BumpMap, i.uv));//fixed4(mixedDiffuse.rgb, weight)
                float3 secondaryUnpackedNormal = UnpackNormal(tex2D(_SecondaryBumpMap, i.uv));
                float3 orthogonalTangent = mul(unity_ObjectToWorld, -cross(i.normal, i.tangent.xyz));
                float3x3 tangentMatrix =
                    float3x3(tangent.x, orthogonalTangent.x, normal.x,
                             tangent.y, orthogonalTangent.y, normal.y,
                             tangent.z, orthogonalTangent.z, normal.z
                    );
                float3 worldUnpackedNormal = mul(tangentMatrix, unpackedNormal); //world unpacked normal is wrong.
               
                float3 worldSecondaryUnpackedNormal = mul(tangentMatrix, secondaryUnpackedNormal); 
                float4 blendFactors = tex2D(_BlendMap, i.originalUV);
                worldUnpackedNormal = normalize(worldUnpackedNormal * blendFactors.r + 
                                      worldSecondaryUnpackedNormal * blendFactors.b +
                                      normal * saturate(1 - blendFactors.r - blendFactors.b));
                //worldUnpackedNormal = unpackedNormal;
                //float a = AngleBetween(float3(0,1,0), unpackedNormal) / 3.141592;
                //worldUnpackedNormal = i.normal;
                //return fixed4(worldUnpackedNormal.y,worldUnpackedNormal.y,worldUnpackedNormal.y,1);
                //return float4(worldUnpackedNormal, 1);
                //return fixed4(mixedDiffuse.rgb, weight);
                
                float4 screenPos = ComputeScreenPos(i.pos);
                //return fixed4(unity_LODFade.x, unity_LODFade.x, unity_LODFade.x, 1);
                float4 textureColor = tex2D(_MainTex, i.uv);
                //if (textureColor.a < _Threshold)
                //    clip(textureColor.a - _Threshold);
                textureColor = float4(1,1,1,1);
                float primaryBumpPercentage = 1 - saturate(1 - blendFactors.r - blendFactors.b);
                //return float4(primaryBumpPercentage, primaryBumpPercentage, primaryBumpPercentage, 1);
                //return float4(1,1,1,1);
                float4 primaryBumpMapColor = tex2D(_MainTex, i.uv) * primaryBumpPercentage + float4(1,1,1,1) * (1 - primaryBumpPercentage); 
                primaryBumpMapColor.a = 1;
                //return primaryBumpMapColor;
                if (_ColorMapType < 0.5)
                {
                    float mainTexColor = tex2D(_MainTex, i.uv);
                    //return RGBLightness(primaryBumpMapColor);
                    if (RGBLightness(mainTexColor) < _ColorMapUpperThreshold &&
                        RGBLightness(mainTexColor) > _ColorMapLowerThreshold)
                    {
                        float2 normalizedPlaneScale = i.planeScale / i.planeScale.x;
                        textureColor = float4(tex2D(_ColorMap, i.originalUV * normalizedPlaneScale).rgb, 1) * primaryBumpMapColor * _Color;
                    }
                    else
                    {
                        //return fixed4(1,0,0,1);
                        float4 colorMapColor = tex2D(_ColorMap, i.originalUV);
                        textureColor = primaryBumpMapColor * _Color;
                    }
                }
                else
                {
                    float mainTexColor = tex2D(_MainTex, i.uv);
                    //return RGBLightness(primaryBumpMapColor);
                    if (RGBLightness(mainTexColor) < _ColorMapUpperThreshold &&
                        RGBLightness(mainTexColor) > _ColorMapLowerThreshold)
                    {
                        float4 colorMapColor = tex2D(_ColorMap, i.originalUV);
                        textureColor = float4(
                            colorMapColor.r * colorMapColor.a,
                            colorMapColor.g * colorMapColor.a,
                            colorMapColor.b * colorMapColor.a, 0) + primaryBumpMapColor * _Color;
                    }
                    else
                    {
                        //return fixed4(1,0,0,1);
                        float4 colorMapColor = tex2D(_ColorMap, i.originalUV);
                        textureColor = primaryBumpMapColor * _Color;
                    }
                }
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

                float inShadow = SHADOW_ATTENUATION(i);
                float4 finalColor = float4(1,1,1,1);
                finalColor *= textureColor;
                //finalColor *= fixed4(mixedDiffuse.rgb, weight);//tex2D(_MainTex, i.uv);
                //finalColor = finalColor + float4(1,1,1,0) * pow(saturate(i.uv.y - 0.5), 2) * 0.45;
                //finalColor = finalColor + float4(1,1,1,0) * saturate(i.uv.y - 0.8) * 0.75;

                float shadowProduct = AngleBetween(worldUnpackedNormal, _WorldSpaceLightPos0.xyz) / 3.151592;//i.normal
                float inShadowSide = shadowProduct > 0.5;

                float4 baseShadowColor = finalColor * fixed4(.75, .75, .85, 1) * fixed4(.35, .35, .35, 1);
                float4 shadowColor = baseShadowColor * _ShadowStrength +
                           finalColor * (1 - _ShadowStrength);
                
                if (inShadow && !inShadowSide)
                {    
                    float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                    float3 reflectedDir = reflect(-_WorldSpaceLightPos0.xyz, worldUnpackedNormal);//i.normal
                    float f = pow(AngleBetween(reflectedDir, -viewDir) / 3.141592, 2);
                    //return fixed4(f,f,f,1);
                    if (f > .9f)
                    {
                        //return fixed4(f,f,f,1);
                    }
                    float scaledShadowProduct = pow(saturate(shadowProduct * 2),3);
                    float4 lightShadowColor = shadowColor * scaledShadowProduct +
                                        finalColor * (1 - scaledShadowProduct);

                    return lightShadowColor * _LightShadowStrength +
                           finalColor * (1 - _LightShadowStrength) + f * .3;
                }
                else
                {
                    return shadowColor;
                }
                
            }
            ENDCG
        }
    }
}
