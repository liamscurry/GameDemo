//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Custom/WavingGrassGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Cull off

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o._ShadowCoord = ComputeScreenPos(o.pos);
                o.uv = v.uv;
                return o;
            }
            
            float4 _Color;
            sampler2D _ShadowMapTexture; 
            sampler2D _MainTex;
            float _Threshold;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float4 textureColor = tex2D(_MainTex, i.uv);
                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);

                float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;
                float4 finalColor = _Color;

                if (inShadow)
                {
                    return finalColor;
                }
                else
                {
                    return finalColor * fixed4(.5, .5, .5, 1);
                }
            }
            ENDCG
        }
    }
}
