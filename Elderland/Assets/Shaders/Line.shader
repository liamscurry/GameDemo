//Pre generated initially from Unlit Shader created by Unity.

Shader "Custom/Line"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _FogColor ("Fog Color", Color) = (0,0,0,1)
        _Falloff ("Falloff", float) = 28
    }
    SubShader
    {
        //UNITY
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        ZWrite off
        Blend SrcAlpha OneMinusSrcAlpha
        //UNITY

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Line.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 fogPosition : TEXCOORD1;
            };

            float4 _Color;
            float4 _FogColor;
            float1 _Falloff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.fogPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 displacement = _WorldSpaceCameraPos - i.fogPosition.xyz;
                float angle = AngleBetween(float3(1, 0, 0), i.fogPosition.xyz);
                fixed4 color = ApplyFog(_Color, _FogColor, Distance(displacement), _Falloff);
                return color;
            }
            ENDCG
        }
    }
}
