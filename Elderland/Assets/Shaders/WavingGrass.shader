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
            #include "FolliageHelper.cginc"

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

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;  
               
                float3 alteredObjectVertex = WarpGrass(v.vertex, v.uv, normal);
                //o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 40)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                    alteredObjectVertex = alteredObjectVertex;
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 40) / 35);
                    o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth, 0, 0));
                    alteredObjectVertex = alteredObjectVertex - fixed4(0, limitedDepth, 0, 0);
                    //
                    //o.pos = UnityObjectToClipPos(v.vertex * float4(1, worldDistance / 30,1,1));
                }
                v.vertex = float4(alteredObjectVertex, 1);
                o.worldDistance = worldDistance;
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o); //upon further inspection, gets clip space of vertex (if ignoring bias), all information needed for depth map

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
            #include "/HelperCgincFiles/MathHelper.cginc"
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
                float3 objectPos : TEXCOORD3;
                float3 normal : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
            };  

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = WarpGrass(v.vertex, v.color.a, normal);
                o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                o.worldPos = worldPos.xyz;
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 40)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 40) / 35);
                    o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0));
                    //
                    //o.pos = UnityObjectToClipPos(v.vertex * float4(1, worldDistance / 30,1,1));
                }
                //o.pos = UnityObjectToClipPos(v.vertex);
                //o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                o.worldDistance = worldDistance;
                //In waving grass shader default
                o.uv = v.uv;
                //o.color = float4(1,0,0,1);
                o.color = v.color;
                o._ShadowCoord = ComputeScreenPos(o.pos);
                o.objectPos = v.vertex;
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }
            
            float4 _ShadowColor;
            sampler2D _ShadowMapTexture; 
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                //return fixed4(1,1,1,1)
                //return fixed4(i.color.xyz, 1);
                //float2 screenPercentagePos = i.screenPos.xy / i.screenPos.w;
                //return fixed4(screenPercentagePos.y, screenPercentagePos.y, screenPercentagePos.y, 1);
                //return i.color.a;
                //return fixed4(tex2D(_MainTex, screenPercentagePos).xyz * tex2D(_MainTex, screenPercentagePos).w, 1);

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
                textureColor;// *= float4(121.0 / 255, 152.0 / 255, 44.0 / 255, 1)
                //float4(169.0 / 255, 223.0 / 255, 32.0 / 255, 1) original saturated
                //return textureColor;
                clip(textureColor.w - _Threshold);

                // Hue
                float lightness = RGBLightness(textureColor);
                lightness = 0.5;
                float hue = RGBHue(textureColor.r, textureColor.g, textureColor.b);
                float saturation = RGBSat(textureColor.r, textureColor.g, textureColor.b);
                float4 hueTint = float4(HSLToRGB(hue, 0.7, .35), 1);//0.6
                hueTint = textureColor;
                //return hueTint;
                //return hueTint;

                // Local hue
                //i.color.xyz,
                float3 color = i.color.xyz;
                float localHue = RGBHue(color.r, color.g, color.b);
                float4 localHueTint = fixed4(HSLToRGB(localHue, 0.4, lightness), 1);//0.5
                localHueTint = float4(i.color.xyz, 1);
                //return localHueTint;
                //return localHueTint;
                //localHueTint = float4(1,1,1,1);

                // Gradient factors
                float hueFactor = saturate(1 - i.objectPos.y + 0.25);//i.uv.y, i.objectPos.y
                
                
                hueFactor = 1 - saturate(1 - i.color.a * 1);
                //return hueFactor;
                //return i.uv.y;
                //return textureColor;
                //hueFactor = 0;
                //return hue / 360;// HUE is distance issue.
                //hueFactor = 1;
                if (i.worldDistance > 20)
                {
                    float limitedDepth = ((i.worldDistance - 20) / 20);
                    //hueFactor += limitedDepth;
                    if (hueFactor > 1)
                        hueFactor = 1;
                    //return fixed4(limitedDepth,limitedDepth,limitedDepth,1);
                }

                float lerpFactorTop = 
                    float4((hueFactor),
                           (hueFactor),
                           (hueFactor), 
                           1);

                float lerpFactorBottom = 
                    float4(1 - hueFactor,
                           1 - hueFactor,
                           1 - hueFactor, 
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

                //return localHueTint;

                //return lerpFactorBottom;

                fixed4 finalColor =
                        (fixed4(1, 1, 1, 1) * localHueTint * lerpFactorTop +
                        fixed4(1, 1, 1, 1) * hueTint * lerpFactorBottom);
                //return finalColor;

                float tipHighlight = 0.8;
                if (i.color.a > tipHighlight)
                {
                    finalColor += float4(1,1,1,1) * (i.color.a - tipHighlight) / (1 - tipHighlight) * .05;
                }
                
                //o.normal actually is based on surface :>
                //return _WorldSpaceLightPos0;
                float groundAngle = saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, i.normal) / (PI));
                finalColor *= float4(float3(groundAngle, groundAngle, groundAngle), 1);
                //return fixed4(groundAngle, groundAngle, groundAngle, 1);
                //return finalColor;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 horizontalViewDir = normalize(float3(viewDir.x, 0, viewDir.z));
                float3 horizontalReflectedDir = normalize(float3(-_WorldSpaceLightPos0.x, 0, -_WorldSpaceLightPos0.z));
                float f = 1 - saturate(AngleBetween(-_WorldSpaceLightPos0.xyz, viewDir) / (PI / 2));
                f = pow(f, 2);
                f = f * 1;
                //return f;
                
                //float viewUpAngle = AngleBetween(viewDir, float3(0, 1, 0)); //working here rn

                if (inShadow)
                {
                    return finalColor + float4(0.9, .9, 1, 0) * f * 2;
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
