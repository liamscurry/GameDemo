// Based on vertex/fragment shader examples on unity documentation.

Shader "Custom/SkewScreenImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Skybox ("Skybox", 2D) = "white" {}
        _WaterBedColor ("WaterBedColor", Color) = (0,0,0,0)
        
        _Threshold ("Threshold", Range(0, 1)) = 0
        _SizeWarpStrength ("SizeWarpStrength", Range(0,2)) = 1
        _WarpXOffset ("WarpXOffset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }// "LightMode"="ForwardBase"
        GrabPass { }
        Blend SrcAlpha OneMinusSrcAlpha
        //LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
            //#pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 ray : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float4 grabPos : TEXCOORD6;
                float2 uvOriginal : TEXCOORD7;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WaterBedColor;
            sampler2D _CameraDepthTexture;
            sampler2D _GrabTexture;
            float _Threshold;
            float _SizeWarpStrength;
            float _WarpXOffset;
            sampler2D _Skybox;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                //float4 alteredVertex = v.vertex;
                //alteredVertex.y = v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.normal = UnityObjectToWorldNormal(normal);
                // From grab pass manual.
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uvOriginal = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i, float3 normal : NORMAL) : SV_Target
            {
                float4 mainTexColor = tex2D(_MainTex, i.uvOriginal);
                clip (_Threshold - mainTexColor.a);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 existingWorldPosition = mul(unity_CameraToWorld, viewPosition);
                
                // From grab pass manual.
                float4 grabPosOffset = float4(_WarpXOffset,
                                              0,
                                              0,
                                              0);
                float4 grabPosScale =
                    float4(
                        1 + sin(uv.x * 20) * .1 * _SizeWarpStrength,
                        1 + sin(uv.y * 20) * .1 * _SizeWarpStrength,
                        1,
                        1);
                //float refraction
                //float4 grabPosScale = float4(1 + sin(uv.x * 45) * .1, 1 + sin(uv.x * 45) * .1, 1, 1);
                float4 existingColor = tex2Dproj(_GrabTexture, i.grabPos * grabPosScale + grabPosOffset);// 
                //    existingColor = tex2Dproj(_Skybox, ((i.grabPos + float4(0,2,0,0)) * float4(0.5,0.5,1,1)));
                //return tex2Dproj(_Skybox, i.grabPos * float4(2,2,1,1) + float4(.25,.25,0,0));
                //Skybox sample
                //return existingColor;
                
                float lightnessMultiplier = 0.9;//0.9 + i.color.r * .5, 1 works fine, 0.9 does not
                //return lightnessMultiplier;
                //return i.color.r; // both existing color and lightness multiplier fine, but result not
                // has nothing to do with i.color.r, removed it and it was still having the bug.
                //existingColor = 
                //    float4(existingColor.r * lightnessMultiplier,
                //           existingColor.g * lightnessMultiplier,
                //           existingColor.b * lightnessMultiplier,
                //           1);
                lightnessMultiplier = 0.075;
                existingColor = existingColor - 
                    float4(lightnessMultiplier,
                           lightnessMultiplier,
                           lightnessMultiplier,
                           0);

                if (depth > 0.99)
                {
                    existingColor = _WaterBedColor -
                        float4(lightnessMultiplier,
                           lightnessMultiplier,
                           lightnessMultiplier,
                           .5);  
                }

                return existingColor;
                //return _WaterBedColor * existingColor;
            }
            ENDCG
        }
    }
}
