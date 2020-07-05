//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/TreeLeaves"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
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
                float2 uv :TEXCOORD0;
                V2F_SHADOW_CASTER; //float4 pos : SV_POSITION thats it
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
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

            //ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            
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
                float4 _ShadowCoord : TEXCOORD1;
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o._ShadowCoord = ComputeScreenPos(o.pos);
                o.uv = v.uv;
                // Via Vertex and fragment shader examples docs.
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }
            
            float4 _Color;
            sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            float _Threshold;
            //float3 _WorldSpaceLightPos0;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;
                float4 finalColor = _Color;
                finalColor = finalColor + float4(1,1,1,0) * pow(saturate(i.uv.y - 0.5), 2) * 0.45;
                finalColor = finalColor + float4(1,1,1,0) * saturate(i.uv.y - 0.8) * 0.75;
                float verticalProduct = sin(i.worldPos.y * .5);
                float horizontalProduct = sin(i.worldPos.x * .25);
                finalColor = finalColor + float4(.2, .2, 0, 1) * verticalProduct + float4(.1, 0, .1, 1) * horizontalProduct;
                //float shadowProduct = AngleBetween(i.normal, _WorldSpaceLightPos0.xyz) / 3.151592;
                //float inShadowSide = shadowProduct > 0.5;
                //return fixed4(inShadowSide, inShadowSide, inShadowSide, 1);

                if (inShadow)
                {
                    return finalColor;
                }
                else
                {
                    return finalColor * fixed4(.85, .75, .75, 1);
                }
            }
            ENDCG
        }
    }
}
