// Based on UnityDeferredLibrary.cginc and Internal-DeferredReflections.shader files.
Shader "Custom/StampParticle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TimeOffset ("TimeOffset", float) = 0
        _Threshold ("Threshold", Range(0,1)) = 0
        _HeightMultiplier ("HeightMultiplier", float) = 1
    }
    SubShader
    {
        // No culling or depth
        ZWrite Off ZTest Always Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "Queue"="Geometry+10"}

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                //Found in UnityDeferredLibrary
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.localPosition = v.vertex;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _Color;
            float _TimeOffset;
            float _Threshold;
            float _HeightMultiplier;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 worldPosition = mul(unity_CameraToWorld, viewPosition);
                float3 horizontalWorldposition = float3(worldPosition.x, 0, worldPosition.z);
                // Helpful tip in forum "SHADER: Get *object* position or distinct valuer per *object*"
                float4 worldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1));
                float3 horizontalWorldCenter = float3(worldCenter.x, 0, worldCenter.z);
                float distanceBetween = length(horizontalWorldposition - horizontalWorldCenter);
                //return AngleBetween(horizontalWorldCenter - horizontalWorldposition, float3(1,0,0)) / Pi;
                float2 pixelDirection = normalize(horizontalWorldposition.xz - horizontalWorldCenter.xz);
                float pixelAngle = atan2(pixelDirection.y, pixelDirection.x); // working
                //sin(22* (pixelAngle * .5))
                float time = fmod((_Time / 15), 1);
                float radius = .75 + 0.05 * Ripple((pixelAngle / Pi) * .5, time + _TimeOffset);
                //float radius = 2;
                float radialPercentage = length(horizontalWorldposition.xz - horizontalWorldCenter.xz) / 1;
                if (radialPercentage > 1)
                    radialPercentage = 1;
                float rotationAngle = Pi * _TimeOffset;
                float2 rotatedPixelDirection =
                    float2(cos(rotationAngle) * pixelDirection.x - sin(rotationAngle) * pixelDirection.y,
                           sin(rotationAngle) * pixelDirection.x + cos(rotationAngle) * pixelDirection.y);
                float2 thresholdUV = (radialPercentage * rotatedPixelDirection * 0.5) + float2(0.5, 0.5);
                float thresholdTextureValue = tex2D(_MainTex, thresholdUV).r;
                float underThreshold = _Threshold > thresholdTextureValue;
                float inRange = distanceBetween >= radius;
                clip(-float2(inRange, underThreshold));
                //return (abs(worldPosition.y - worldCenter.y) > _HeightMultiplier);
                clip(-(abs(worldPosition.y - worldCenter.y) > _HeightMultiplier));
                float heightPercentage = (worldPosition.y - worldCenter.y) / _HeightMultiplier;
                if (heightPercentage < 0)
                    heightPercentage = 0;
                heightPercentage = 1 - heightPercentage;
                heightPercentage = pow(heightPercentage, 3);
                //return heightPercentage;
                //return fixed4(1,0,0,.5);
                return fixed4(_Color.xyz, heightPercentage);
            }
            ENDCG
        }
    }
}
