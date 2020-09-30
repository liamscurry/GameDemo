// Based on vertex/fragment shader examples on unity documentation.

Shader "Custom/StillWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReflectionMap ("ReflectionMap", 2D) = "white" {}
        _WaterBedColor ("WaterBedColor", Color) = (0,0,0,0)
        _WaterLineThreshold ("WaterLineThreshold", Range(0, 1)) = 0
        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 0
        _EnableFog ("EnableFog", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        GrabPass { }
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
            #include "Color.cginc"
            #include "/HelperCgincFiles/MathHelper.cginc"
            #include "/HelperCgincFiles/FogHelper.cginc"
            #include "/HelperCgincFiles/LODHelper.cginc"

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
                float3 ray : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float4 grabPos : TEXCOORD6;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _WaterBedColor;
            sampler2D _CameraDepthTexture;
            sampler2D _GrabTexture;
            float _WaterLineThreshold;
            float _WarmColorStrength;
            float _EnableFog;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                //float4 alteredVertex = v.vertex;
                //alteredVertex.y = v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                o.normal = UnityObjectToWorldNormal(normal);
                // From grab pass manual.
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i, float3 normal : NORMAL) : SV_Target
            {
                float inShadow = 0;
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

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
                //Found in Internal-DeferredReflections frag function
                i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float4 viewPosition = float4(i.ray * depth, 1);
                float4 existingWorldPosition = mul(unity_CameraToWorld, viewPosition);
                float4 newWorldPosition = i.worldPos;

                float waterDepthFactor = saturate(pow(abs(existingWorldPosition.y - newWorldPosition.y) / 10, .5) + .1);
                //return waterDepthFactor;
                if (length(existingWorldPosition - newWorldPosition) / (2 + sin(_Time.y + i.worldPos.x)) < _WaterLineThreshold)//.5
                {
                    if (_EnableFog)
                    {
                        STANDARD_FOG_TEMPERATURE(float4(1, 1, 1, 1), _WarmColorStrength);
                    }
                    else
                    {
                        return float4(1, 1, 1, 1);
                    }
                }
                
                // From grab pass manual.
                float grabDepthFactor = saturate(pow(abs(existingWorldPosition.y - newWorldPosition.y) / 10, 2));
                //return grabDepthFactor;
                float4 grabPosOffset = float4(sin(_Time.y + i.worldPos.x * .5) * grabDepthFactor * .3,
                                              sin(_Time.y * .7 + i.worldPos.z * .8) * grabDepthFactor * .3,
                                              0,
                                              0);
                float4 existingColor = tex2Dproj(_GrabTexture, i.grabPos + grabPosOffset);
                //return existingColor;

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

                //float fresnelAngle = saturate(pow(AngleBetween(alteredNormal, viewDir) / 3.151592 * 2.2, 3.5));
                float fresnelAngle = saturate(pow(AngleBetween(alteredNormal, viewDir) / 3.151592 * 2.2, 6.5));
                //return fixed4(fresnelAngle, fresnelAngle, fresnelAngle, 1);
                //return waterDepthFactor;
                float2 horizontalPosition = float2(newWorldPosition.x, newWorldPosition.z);
                float horizontalWaterDepthFactor = pow(length(_WorldSpaceCameraPos.xz - horizontalPosition) / 25, .5);
                //return waterDepthFactor;
                //return waterDepthFactor;
                float4 waterBedCompositeColor =
                    existingColor * (1 - waterDepthFactor) + _WaterBedColor * waterDepthFactor;
                //return waterDepthFactor;
                //return waterDepthFactor * (1 - horizontalWaterDepthFactor);
                //fresnelAngle -= waterDepthFactor * (1 - horizontalWaterDepthFactor) * 1;
                float4 finalColor = color * (fresnelAngle) + waterBedCompositeColor * (1 - fresnelAngle);
                if (_EnableFog)
                {
                    STANDARD_FOG_TEMPERATURE(finalColor, _WarmColorStrength);
                }
                else
                {
                    return finalColor;
                }
                //return _WaterBedColor * existingColor;
            }
            ENDCG
        }
    }
}
