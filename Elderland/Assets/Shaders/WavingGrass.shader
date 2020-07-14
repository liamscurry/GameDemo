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
                float worldDistance : TEXCOORD2;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                o.worldDistance = worldDistance;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float _Threshold = 1;//0.675
                _Threshold -= i.worldDistance * 0.02;
                if (_Threshold < 0.2)
                    _Threshold = 0.2;
                float4 textureColor = (tex2D(_MainTex, i.uv));
                clip(textureColor.w - _Threshold);
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
                "Queue"="Geometry+200"
                "RenderType"="Grass"
                //"RenderType"="Transparent"    
                //"Queue"="Transparent"    
                // In Waving grass default shader.
            }

            //ZWrite On
            //LOD 200

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "FolliageHelper.cginc"
            
            //float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float4 _ShadowCoord : TEXCOORD1;
                float4 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR0; //messing with multiple grass textures.
                float depth : COLOR2;
                float worldDistance : TEXCOORD2;
                float4 objectPos : TEXCOORD3;
            };  

            v2f vert (appdata v)
            {
                v2f o;
                o.depth = length(UnityObjectToViewPos(v.vertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 40)
                {
                    o.pos = UnityObjectToClipPos(v.vertex);
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 40) / 35);
                    o.pos = UnityObjectToClipPos(v.vertex - fixed4(0, limitedDepth, 0, 0));
                    //
                    //o.pos = UnityObjectToClipPos(v.vertex * float4(1, worldDistance / 30,1,1));
                }
                o.worldDistance = worldDistance;
                //In waving grass shader default
                o.uv = v.uv;
                //o.color = float4(1,0,0,1);
                o.color = v.color;
                o._ShadowCoord = ComputeScreenPos(o.pos);
                o.objectPos = v.vertex;
                return o;
            }
            
            float4 _ShadowColor;
            sampler2D _ShadowMapTexture; 
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                //return fixed4(i.worldDistance / 50,0,0,1);
                float inShadow = tex2Dproj(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).x;

                float _Threshold = 1;//0.675
                _Threshold -= i.worldDistance * 0.02;
                if (_Threshold < 0.2)
                    _Threshold = 0.2;
                //_Threshold = 0.5;
                //_Threshold = 0;
                //_Threshold -= i.worldDistance * .04;
                /*if (i.depth * 35 > 40)
                {
                    _Threshold = saturate(_Threshold + i.depth);   
                }*/
                //return fixed4(i.depth * 35 / 50, 0, 0, 1);
                
                float4 textureColor = (tex2D(_MainTex, i.uv));
                //return textureColor;
                clip(textureColor.w - _Threshold);

                // Hue
                float lightness = RGBLightness(textureColor);
                lightness = 0.5;
                float localHue = RGBHue(textureColor.r, textureColor.g, textureColor.b);
                float4 localHueTint = float4(HSLToRGB(localHue, 0.75, lightness), 1);

                // Local hue
                //i.color.xyz,
                float3 color = i.color.xyz;
                float hue = RGBHue(color.r, color.g, color.b);
                float4 hueTint = fixed4(HSLToRGB(hue, 0.75, 0.35), 1);
                //localHueTint = float4(1,1,1,1);

                // Gradient factors
                float hueFactor = saturate(1 - i.objectPos.y + 0.25);//i.uv.y
                hueFactor = saturate(1 - i.objectPos.y * 2);
                if (i.worldDistance > 20)
                {
                    float limitedDepth = ((i.worldDistance - 20) / 20);
                    hueFactor += limitedDepth;
                    if (hueFactor > 1)
                        hueFactor = 1;
                    //return fixed4(limitedDepth,limitedDepth,limitedDepth,1);
                }

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

                //x, y  is texture scale, zw is offset
                //via boglus in unity forums. "get the scale of object related to worldspace"
                float scaledUV = (i.uv.y * _MainTex_ST.y) + _MainTex_ST.w;
                scaledUV = i.objectPos.y;
                //caledUV = abs(unity_ObjectToWorld[2].x);//scale of y
                //scaledUV = abs(_MainTex_ST.w);
                //scaledUV = i.uv.y;
                //scaledUV = _MainTex_ST.w;
                //return fixed4(hueFactor, hueFactor, hueFactor, 1);
                //return fixed4(1 - i.uv.y, 1 - i.uv.y, 1 - i.uv.y, 1);
                //return fixed4(hue, hue, hue, 1);
                //return hueTint;

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
