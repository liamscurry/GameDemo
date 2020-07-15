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
                float3 normal : TEXCOORD0;
                float4 tc : TEXCOORD1;
                float4 uv : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                SHADOW_COORDS(4)
            };

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
                v2fInput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.normal = UnityObjectToWorldNormal(v.normal);
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
                // Terrain texture:
                // Learned via standard-firstpass.shader in default shaders
                half4 splatControl;
                half weight;
                fixed4 mixedDiffuse;
                half4 defaultSmoothness = half4(0.05, 0.05, 0.05, 0.05);
                float3 normal = float3(0,1,0);
                Input input = (Input)0;
                input.tc = i.tc;
                SplatmapMix(input, defaultSmoothness, splatControl, weight, mixedDiffuse, normal);
                //return fixed4(mixedDiffuse.rgb, weight);
                
                float4 screenPos = ComputeScreenPos(i.pos);
                //return fixed4(unity_LODFade.x, unity_LODFade.x, unity_LODFade.x, 1);
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                
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
                float4 finalColor = _Color;
                finalColor *= fixed4(mixedDiffuse.rgb, weight);//tex2D(_MainTex, i.uv);
                //finalColor = finalColor + float4(1,1,1,0) * pow(saturate(i.uv.y - 0.5), 2) * 0.45;
                //finalColor = finalColor + float4(1,1,1,0) * saturate(i.uv.y - 0.8) * 0.75;

                float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                float inShadowSide = shadowProduct > 0.5;

                float4 baseShadowColor = finalColor * fixed4(.75, .75, .85, 1) * fixed4(.35, .35, .35, 1);
                float4 shadowColor = baseShadowColor * _ShadowStrength +
                           finalColor * (1 - _ShadowStrength);
                
                if (inShadow && !inShadowSide)
                {    
                    float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                    float3 reflectedDir = reflect(-_WorldSpaceLightPos0.xyz, i.normal);
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
                           finalColor * (1 - _LightShadowStrength) + f * .4;
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
