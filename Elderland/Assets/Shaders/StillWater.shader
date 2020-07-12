// Based on vertex/fragment shader examples on unity documentation.

Shader "Custom/StillWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReflectionMap ("ReflectionMap", 2D) = "white" {}
        _WaterBedColor ("WaterBedColor", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
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

            // Angle between working
            float AngleBetween(float3 u, float3 v)
            {
                float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
                if (numerator > 1)
                    numerator = 1;
                if (numerator < -1)
                    numerator = -1;
                float denominator = length(u) * length(v);
                return acos(numerator / denominator);
            }

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WaterBedColor;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                //float4 alteredVertex = v.vertex;
                //alteredVertex.y = v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag (v2f i, float3 normal : NORMAL) : SV_Target
            {
                float3 calculatedNormal = UnityObjectToWorldNormal(normal);//1.4// * .6
                float3 alteredNormal = normalize(
                    i.normal * 1 +
                    sin(cos(_Time.y * 2.3 + i.worldPos.x * .3)) * float3(.0035,0,0) +
                    sin(3* (_Time.y * 1.3 + i.worldPos.z) * .3) * float3(0,0,.0075))
                    ;//sin(.1* (_Time.y * 1.3 + i.worldPos.x * i.worldPos.z * .01)) * float3(.003,0,.0035)
                    //
                //+ sin(_Time.y + v.vertex.z * 50) * float3(0,0,.2)
                // Learned in vertex/fragment shader examples in unity docs.
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                //float3 worldNormal = alteredNormal;
                
                float3 reflectionDirection = reflect(-viewDir, alteredNormal);
                fixed4 color = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDirection);

                /*float sunAngle = 0;
                //if (_WorldSpaceLightPos0.w < 0.5)
                //{
                    sunAngle = AngleBetween(-_WorldSpaceLightPos0.xyz, reflectionDirection) / 3.151592;// / 3.151592 * 180 (
                //}
                
                if (sunAngle > .95)
                {
                    return fixed4(1,0,0,1);
                }*/
                
                //return fixed4(_WorldSpaceLightPos0.xyz, 1);
                //return fixed4(sunAngle, sunAngle, sunAngle, 1);
                
                //float3 worldNormal = alteredNormal;
                

                //had to have in light mode forward base

                float fresnelAngle = saturate(pow(AngleBetween(alteredNormal, viewDir) / 3.151592 * 2.2, 3.5));
                //return fixed4(fresnelAngle, fresnelAngle, fresnelAngle, 1);
                
                return color * (fresnelAngle) + _WaterBedColor * (1 - fresnelAngle);
            }
            ENDCG
        }
    }
}
