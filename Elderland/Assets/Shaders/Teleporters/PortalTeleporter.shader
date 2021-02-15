Shader "Custom/PortalTeleporter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderSize ("BorderSize", Range(0, 0.5)) = 0.2
    }
    SubShader
    {
        // No culling or depth
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float2 objectScale : TEXCOORD2;
                float4 worldPos : TEXCOORD3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                float4 center = mul(unity_ObjectToWorld, float4(0,0,0,1));
                float4 x = mul(unity_ObjectToWorld, float4(1,0,0,1));
                float4 y = mul(unity_ObjectToWorld, float4(0,1,0,1));
                o.objectScale = float2(length(x - center), length(y - center));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _BorderSize;

            fixed4 frag (v2f i) : SV_Target
            {
                float yUVModifier = i.objectScale.y / i.objectScale.x;

                float xBorderLength = 
                    abs(i.uv.x - 0.5) - (0.5 - _BorderSize);
                float xBorderPercentage = saturate(xBorderLength / _BorderSize);
                float yBorderLength = 
                    yUVModifier * abs(i.uv.y - 0.5) - (yUVModifier * 0.5 - _BorderSize);
                float yBorderPercentage = saturate(yBorderLength / _BorderSize);
                float borderPercentage = max(xBorderPercentage, yBorderPercentage);
                float2 screenPercentagePos = i.screenPos.xy / i.screenPos.w;
                //return float4(screenPercentagePos.y, 0, 0, 1);
                float distanceToCamera =
                    length(_WorldSpaceCameraPos - i.worldPos);
                float distancePercentage = saturate((distanceToCamera - 2)/ 10);

                borderPercentage = borderPercentage * distancePercentage;
                //borderPercentage = 0 * i.vertex.;
                return 
                    tex2D(_MainTex, screenPercentagePos) * (1 - borderPercentage) +
                    float4(1,1,1,1) * borderPercentage ;
            }
            ENDCG
        }
    }
}
