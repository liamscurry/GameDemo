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
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "FolliageHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR0;
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
               
                //float3 alteredObjectVertex = WarpGrass(v.vertex, v.uv, normal);
                float3 alteredObjectVertex = v.vertex;
                //WarpGrass(v.vertex, v.color.a, normal);
                //o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 100)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                    alteredObjectVertex = alteredObjectVertex;
                }
                else
                {
                    float limitedDepth = saturate((worldDistance - 100) / 30);
                    //o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth, 0, 0));
                    o.pos = UnityObjectToClipPos(alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0));
                    //alteredObjectVertex = alteredObjectVertex - fixed4(0, limitedDepth, 0, 0);
                    alteredObjectVertex = alteredObjectVertex - fixed4(0, limitedDepth * 1.75, 0, 0);
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
                SHADOW_CASTER_FRAGMENT(i)
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
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Color.cginc"
            #include "AutoLight.cginc"

            #include "/HelperCgincFiles/ShadingHelper.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
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
                SHADOW_COORDS(1)
                float4 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR0; //messing with multiple grass textures.
                float depth : COLOR2;
                float worldDistance : TEXCOORD2;
                float3 objectPos : TEXCOORD3;
                float3 normal : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
            };  
             
            float4 _ShadowColor;
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _WavingTint;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = v.vertex;
                //WarpGrass(v.vertex, v.color.a, normal);
                o.depth = length(UnityObjectToViewPos(alteredObjectVertex)) / 35;

                float4 worldPos = mul(unity_ObjectToWorld, float4(alteredObjectVertex, 1));
                o.worldPos = worldPos.xyz;
                float worldDistance = length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                //o.pos = UnityObjectToClipPos(v.vertex);
                if (worldDistance < 100)
                {
                    o.pos = UnityObjectToClipPos(alteredObjectVertex);
                }
                else
                {
                    float worldDistanceOverlap = (worldDistance - 100);
                    if (worldDistanceOverlap < 0)
                        worldDistanceOverlap = 0;
                    float limitedDepth = saturate(worldDistanceOverlap / 30);
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
               
                TRANSFER_SHADOW(o)
                o.objectPos = v.vertex;
                o.normal = UnityObjectToWorldNormal(normal);
                return o;
            }

            fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
            {
                float _Threshold = 1;//0.675
                _Threshold -= i.worldDistance * 0.1;
                if (_Threshold < 0.2)
                    _Threshold = 0.2;
                
                float4 textureColor = (tex2D(_MainTex, i.uv));

                clip(textureColor.w - _Threshold);

                // Hue
                float lightness = RGBLightness(textureColor);
                lightness = 0.5;
                float hue = RGBHue(textureColor.r, textureColor.g, textureColor.b);
                float saturation = RGBSat(textureColor.r, textureColor.g, textureColor.b);
                float4 hueTint = float4(HSLToRGB(hue, 0.7, .35), 1);//0.6
                hueTint = _WavingTint;

                // Local hue
                //i.color.xyz,
                float3 color = i.color.xyz;
                float localHue = RGBHue(color.r, color.g, color.b);
                float4 localHueTint = fixed4(HSLToRGB(localHue, 0.4, lightness), 1);//0.5
                localHueTint = float4(i.color.xyz, 1);

                // Gradient factors
                float hueFactor = saturate(1 - i.objectPos.y + 0.25);//i.uv.y, i.objectPos.y
                
                
                hueFactor = 1 - saturate(1 - i.color.a * 1);

                if (i.worldDistance > 20)
                {
                    float limitedDepth = ((i.worldDistance - 20) / 20);
  
                    if (hueFactor > 1)
                        hueFactor = 1;
               
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

                fixed4 finalColor =
                        (fixed4(1, 1, 1, 1) * localHueTint * lerpFactorTop +
                        fixed4(1, 1, 1, 1) * hueTint * lerpFactorBottom);

                float tipHighlight = 0.1;
        
                if (i.color.a > tipHighlight)
                {
                    finalColor += float4(1,1,1,1) * (i.color.a - tipHighlight) / (1 - tipHighlight) * .01;
                }

                float inShadow = SHADOW_ATTENUATION(i);
                float4 localColor = finalColor;
                localColor *= tex2D(_MainTex, i.uv);

                // Properties from Shading Helper (explicit)
                _ShadowStrength = 0.578;

                _HighlightStrength = 0.09;
                _HighlightIntensity = 0.26;

                _ReflectedIntensity = 1.09;

                // Learned in AutoLight.cginc
                // Shadow Fade
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = UnityComputeShadowFade(fadeDistance);

                return Shade(i.normal, i.worldPos, localColor, inShadow, fadeValue);
            }
            ENDCG
        }
    }
}
