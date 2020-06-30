//Right now casts shadows and receives shadows correctly. In normal scene casts shadows on surfaces but unity must
//disable this somehow. Fine for now as you dont need grass to project shadows but would be nice to know for if
//I develop a terrain system in the future.

//Wanted to add group of objects for forward and backward rendering but can't as unity terrain details
//can only have one object hierarchy per detail or else it is not drawn.

//Based on built in grass shaders.
Shader "Hidden/TerrainEngine/Details/WavingDoublePass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 _ShadowCoord : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float depth : COLOR2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o._ShadowCoord = ComputeScreenPos(o.pos);

                o.depth = length(UnityObjectToViewPos(v.vertex)) / 35;

                return o;
            }
            
            float4 _ShadowColor;
            sampler2D _ShadowMapTexture; 
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;

                float4 textureColor = (tex2D(_MainTex, i.uv));

                clip(textureColor.w - 1);

                //float depthFactor = i.depth;

                // Hue
                float lightness = RGBLightness(textureColor);
                float hue = RGBHue(textureColor.r, textureColor.g, textureColor.b);
                float4 hueTint = float4(HSLToRGB(hue, 0.75, 0.35), 1);

                // Local hue
                float4 color = float4(i.color.xyz, 1);
                float localHue = RGBHue(color.r, color.g, color.b);
                float4 localHueTint = fixed4(HSLToRGB(localHue, 0.75, 0.5), 1);

                // Gradient factors
                float hueFactor = saturate(1 - i.uv.y + 0.25);

                float lerpFactorTop = 
                    float4((1 - hueFactor),
                           (1 - hueFactor),
                           (1 - hueFactor), 
                           1);

                float lerpFactorBottom = 
                    float4(hueFactor,
                           hueFactor,
                           hueFactor, 
                           1);

                fixed4 finalColor =
                        (fixed4(1, 1, 1, 1) * localHueTint * lerpFactorTop +
                        fixed4(1, 1, 1, 1) * hueTint * lerpFactorBottom);

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
