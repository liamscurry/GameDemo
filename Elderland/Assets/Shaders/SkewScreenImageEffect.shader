// Based on vertex/fragment shader examples on unity documentation.

Shader "Custom/SkewScreenImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RefractionMap ("RefractionMap", 2D) = "white" {}
        _WaterBedColor ("WaterBedColor", Color) = (0,0,0,0)
        _Threshold ("Threshold", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        GrabPass { }
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WaterBedColor;
            sampler2D _CameraDepthTexture;
            sampler2D _GrabTexture;
            float _Threshold;

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
                float4 grabPosOffset = float4(0.0 * sin(uv.x * 23),
                                              -0.0 * sin(uv.y * 23),
                                              0,
                                              0);
                float4 grabPosScale = float4(1 + sin(uv.x * 20) * .1, 1 + sin(uv.y * 20) * .1, 1, 1);
                //float refraction
                //float4 grabPosScale = float4(1 + sin(uv.x * 45) * .1, 1 + sin(uv.x * 45) * .1, 1, 1);
                float4 existingColor = tex2Dproj(_GrabTexture, i.grabPos * grabPosScale + grabPosOffset);
                
                return existingColor * float4(0.9, 0.9, 0.9, 1);// * 
                //return _WaterBedColor * existingColor;
            }
            ENDCG
        }
    }
}
