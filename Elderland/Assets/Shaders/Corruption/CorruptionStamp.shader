// Based on UnityDeferredLibrary.cginc and Internal-DeferredReflections.shader files.

// References: William Chyr. See References file for more details.
Shader "Custom/CorruptionStamp"
{
    Properties
    {
        _HorizontalTexture ("HorizontalTexture", 2D) = "white" {}
        _VerticalTexture ("VerticalTexture", 2D) = "white" {}
        _VolumeTexture ("VolumeTexture", 3D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TimeOffset ("TimeOffset", float) = 0
        _Threshold ("Threshold", Range(0,1)) = 0
        _ScaleX ("ScaleX", float) = 1
        _ScaleZ ("ScaleZ", float) = 1
        _ScaleY ("ScaleY", float) = 1
    }
    SubShader
    {
        // No culling or depth
        //ZWrite Off ZTest Always
        //Blend SrcAlpha OneMinusSrcAlpha
        //Tags { "Queue"="Geometry+10" }
        Tags { "Queue"="Geometry+100" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // From RayMarching.cginc in my RayMarchingResearch project.
            #define Pi 3.1515926536

            // Angle between working
            float AngleBetween(float3 u, float3 v)
            {
                float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
                float denominator = length(u) * length(v);
                return acos(numerator / denominator);
            }

            // explored via Desmos grapher
            float LimitRipple(float x)
            {
                return pow(x / 0.5, 8) - 1;
            }

            float Ripple(float x, float t)
            {
                float g = 0.75 * sin(20 * Pi * (x + 20 * t)) * LimitRipple(x);
                float h = 0.50 * sin(10 * Pi * (x - 10 * t)) * LimitRipple(x);
                float i = 3.00 * sin(4 * Pi * (x + 4 * t))   * LimitRipple(x);
                return sin(2 * Pi * x) + g + h + i;
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 ray : TEXCOORD1;
                float3 localPosition : TEXCOORD2;
            };

            sampler2D _CameraDepthNormalsTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                //o.screenPos.y = 1 - o.screenPos.y;
                //Found in UnityDeferredLibrary
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.localPosition = v.vertex;
                return o;
            }

            sampler2D _MainTex;
            //UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);
            sampler2D _HorizontalTexture;
            sampler2D _VerticalTexture;
            sampler3D _VolumeTexture;
            float4 _Color;
            float _TimeOffset;
            float _Threshold;

            float _ScaleX;
            float _ScaleZ;
            float _ScaleY;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float4 encodedColor = 
                    tex2D(_CameraDepthNormalsTexture, i.screenPos.xy);
                float depth;
                float3 normal;
                DecodeDepthNormal(encodedColor, depth, normal);
                return i.screenPos;
                return float4(depth, depth, depth, 1);
                return float4(normal, 1);
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 worldPosition = mul(unity_CameraToWorld, viewPosition);
                float3 horizontalWorldposition = float3(worldPosition.x, worldPosition.y, worldPosition.z);
                // Helpful tip in forum "SHADER: Get *object* position or distinct valuer per *object*"
                float4 worldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1));
                float3 horizontalWorldCenter = float3(worldCenter.x, worldCenter.y, worldCenter.z);
                
                float distanceBetweenX = abs(horizontalWorldposition.x - horizontalWorldCenter.x);
                float distanceBetweenZ = abs(horizontalWorldposition.z - horizontalWorldCenter.z);
                float distanceBetweenY = abs(horizontalWorldposition.y - horizontalWorldCenter.y);
                
                float inRangeX = distanceBetweenX >= _ScaleX;
                float inRangeZ = distanceBetweenZ >= _ScaleZ;
                float inRangeY = distanceBetweenY >= _ScaleY;
                clip(-float3(inRangeX, inRangeY, inRangeZ));
                
                // texture sample
                float displacementBetweenX = horizontalWorldposition.x - horizontalWorldCenter.x;
                float displacementBetweenZ = horizontalWorldposition.z - horizontalWorldCenter.z;
                float displacementBetweenY = horizontalWorldposition.y - horizontalWorldCenter.y;

                float xUV = (displacementBetweenX / _ScaleX * 0.5) + 0.5;
                float zUV = (displacementBetweenZ / _ScaleZ * 0.5) + 0.5;
                float yUV = (displacementBetweenY / _ScaleY * 0.5) + 0.5;


                float4 textureColor = tex3D(_VolumeTexture, float3(xUV, yUV, zUV));
                return textureColor;

                float4 textureColorHorizontal = tex2D(_HorizontalTexture, float2(xUV, zUV));
                float4 textureColorVertical = tex2D(_VerticalTexture, float2(xUV, yUV));
                float4 combinedTextureColor = (textureColorHorizontal + textureColorVertical) / 2;
                combinedTextureColor.a = 1;
                return combinedTextureColor;
            }
            ENDCG
        }
    }
}
