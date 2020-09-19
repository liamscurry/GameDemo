Shader "Custom/DefaultUIStencil"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StencilTex("StencilMap", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0
        _HorizontalOnPercentage ("HorizontalOnPercentage", Range(0,1)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        // Via unity manual on stencils.

        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD1;
                float4 worldScale : TEXCOORD2;
                float3 normal : TEXCOORD3;
            };

            v2f vert (appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                float tangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, v.tangent.xyz));
                float3 orthoTangent = cross(v.tangent.xyz, v.normal);
                float orthoTangentScale = length(mul(unity_ObjectToWorld, float3(0,0,0)) - mul(unity_ObjectToWorld, orthoTangent));
                o.worldScale = v.texcoord * float4(tangentScale, orthoTangentScale, 1, 1);
                o.normal = v.tangent;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _StencilTex;
            float _Stencil;
            float4 _Color;
            float _Threshold;
            float _HorizontalOnPercentage;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.a *= tex2D(_StencilTex, i.uv).a;
                clip(_HorizontalOnPercentage - i.uv.x );
                //return i.uv.y / i.uv.w;
                // return tex2D(_StencilTex, i.worldScale).a;
                // just invert the colors
                //col.rgb = 1 - col.rgb;
                //clip(-(col.a < _Threshold));
                //return _Color * i.color;
                return col * _Color;
            }
            ENDCG
        }
    }
}
