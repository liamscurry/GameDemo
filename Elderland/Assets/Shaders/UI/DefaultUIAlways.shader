// Custom shader which is default unlit shader but has specified draw order for image effects.
Shader "Custom/DefaultUIAlways"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0, 1)) = 0
        _Skew ("Skew", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+20"}
        LOD 100
        Cull Off
        ZWrite On
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Threshold;
            float _Skew;

            v2f vert (appdata v)
            {
                v2f o;
                float4 alteredVertex = v.vertex;
                float4 origin = mul(unity_ObjectToWorld, float3(0,0,1));
                float4 shiftedOrigin = mul(unity_ObjectToWorld, float3(1,0,1));
                float xScale = length(shiftedOrigin - origin);
                if (xScale != 0)
                    alteredVertex.x += _Skew * (1 / xScale) * sign(alteredVertex.y);
                o.vertex = UnityObjectToClipPos(alteredVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 color = tex2D(_MainTex, i.uv) * _Color;
                clip(color.a - _Threshold);
                return color;
            }
            ENDCG
        }
    }
}
