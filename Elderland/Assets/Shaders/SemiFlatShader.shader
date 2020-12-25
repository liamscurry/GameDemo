//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/SemiFlatShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("BumpMap", 2D) = "bump" {}
        _BumpMapIntensity ("BumpMapIntensity", Range(0, 1)) = 0

        _CutoutTex ("CutoutTex", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _CrossFade ("CrossFade", float) = 0

        // Properties from Shading Helper
        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0

        _HighlightStrength ("HightlightStrength", Range(0, 2)) = 1 
        _HighlightIntensity ("HighlightIntensity", Range(0, 2)) = 1

        _ReflectedIntensity ("ReflectedIntensity", Range(0, 3)) = 1

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
                //Cull off
        LOD 400

        // Via SpeedTree.shader
        Pass
        {
            Name "SemiFlatShaderShadow"
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
            sampler2D _CutoutTex;

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

                float4 cutoutColor = tex2D(_CutoutTex, i.uv);
                float underThreshold = _Threshold > cutoutColor;
                clip(-underThreshold);

                SHADOW_CASTER_FRAGMENT(i)
                //return 0;
            }
            ENDCG
        }

        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SemiFlatShader"
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

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #define TERRAIN_STANDARD_SHADER
            #define TERRAIN_INSTANCED_PERPIXEL_NORMAL

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"
            #include "TerrainSplatmapCommon.cginc"

            #include "/HelperCgincFiles/NormalMapHelper.cginc"
            #include "/HelperCgincFiles/ShadingHelper.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
            #include "/HelperCgincFiles/LODHelper.cginc"

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
                float3 tangent : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                DECLARE_TANGENT_SPACE(6, 7, 8)
            };

            v2f vert (appdata v, float3 normal : NORMAL, float4 tangent : TANGENT)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                TRANSFER_SHADOW(o)
                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = float3(0,0,1);
                o.tangent = tangent;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                ComputeTangentSpace(normal, tangent, o.tanX1, o.tanX2, o.tanX3);
                return o;
            }
            
            float4 _Color;
            sampler2D _MainTex;
            sampler2D _BumpMap;
            float _BumpMapIntensity;

            sampler2D _CutoutTex;
            float _Threshold;

            float _CrossFade;
            
            float _WarmColorStrength;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                //Terrain texture:
                // Normal from terrain texture (From rendering systems project character helper):
                half3 tangentNormal = UnpackNormal(tex2D(_BumpMap, i.uv));
                tangentNormal.y *= -1;
                half3 worldNormal = 
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, tangentNormal);
                half3 originalWorldNormal = 
                    TangentToWorldSpace(i.tanX1, i.tanX2, i.tanX3, half3(0,0,1));
                worldNormal = worldNormal * _BumpMapIntensity + originalWorldNormal * (1 - _BumpMapIntensity);
                
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                //Remove later? Not used?
                float4 cutoutColor = tex2D(_CutoutTex, i.uv);
                float underThreshold = _Threshold > cutoutColor;
                clip(-underThreshold);
                
                ApplyDither(i.screenPos, _CrossFade);

                float inShadow = SHADOW_ATTENUATION(i);
                float4 localColor = _Color;
                localColor *= tex2D(_MainTex, i.uv);

                // Learned in AutoLight.cginc
                // Shadow Fade
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                return Shade(worldNormal, i.worldPos, localColor, inShadow, fadeValue);

                //STANDARD_FOG_TEMPERATURE(fadedShadowColor * (1 - shadeFade) + lightColor * shadeFade, _WarmColorStrength);
                //inShadow = (1 - fadeValue) * inShadow + (fadeValue) * 1;
                //STANDARD_SHADOWSIDE_FOG_TEMPERATURE(localShadowColor * _ShadowStrength + localColor * (1 - _ShadowStrength), _WarmColorStrength);
            }
            ENDCG
        }

        Pass
        {
            //Based on AutodeskInteractive additive forward pass structure in built in shaders.
            Name "PointLights"
            Tags
            {
                "LightMode"="ForwardAdd"
            }

            Blend One One
            ZWrite Off
            ZTest LEqual
            
            CGPROGRAM

            float _ApplyLight;

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster
            #include "/HelperCgincFiles/LightHelper.cginc"
            ENDCG
        }
    }
}
