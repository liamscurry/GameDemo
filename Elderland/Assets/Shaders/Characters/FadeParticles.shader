// Custom shader which is default unlit shader but has specified draw order for image effects.
Shader "Custom/FadeParticles"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0, 1)) = 0

        _WorldCenter ("WorldCenter", Vector) = (0,0,0,0)
        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        //CharacterEffectsHelper.cginc
        _ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+20"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "../HelperCgincFiles/CharacterEffectsHelper.cginc"

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
                float4 color : COLOR0;
                float4 objectPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Threshold;

            float4 _WorldCenter;
            float _WorldMaxHeight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color;
                o.objectPos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 color = tex2D(_MainTex, i.uv) * _Color * i.color;
                clip(color.a - _Threshold);
                float verticalPercentage = i.objectPos.y - _WorldCenter.y;
                verticalPercentage = verticalPercentage / _WorldMaxHeight;
                if (verticalPercentage > 1)
                    verticalPercentage = 1;
                if (verticalPercentage < -1)
                    verticalPercentage = -1;

                ApplyCharacterFadeRaw(float4(0, -verticalPercentage, 0, 1));

                return color;
            }
            ENDCG
        }
    }
}
