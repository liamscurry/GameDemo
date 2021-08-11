Shader "Custom/CorruptionThistle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _ThresholdMargin ("ThresholdMargin", Range(0, 1)) = 0
        _CrossFade ("CrossFade", float) = 0

        _ShadowStrength ("ShadowStrength", Range(0, 1)) = 0

        _WarmColorStrength ("WarmColorStrength", Range(0, 1)) = 1
        _ApplyLight ("ApplyLight", Range(0.0, 1.0)) = 1.0

        _WorldMaxHeight ("WorldMaxHeight", float) = 10000

        // From CharacterEffectsHelper.cginc
        _ClipThreshold ("ClipThreshold", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Cull Off
        // SemiFlatShader pass structure
        Pass
        {
            Name "SemiFlatShaderShadow"
            Tags { "LightMode"="ShadowCaster" }

            CGPROGRAM
            #pragma vertex vertWave
            #pragma fragment semiFlatFrag
            #pragma multi_compile_shadowcaster
            
            #include "Assets/Shaders/FolliageHelper.cginc"
            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderShadowCaster.cginc"

            v2f vertWave (appdata v, float3 normal : NORMAL, float4 tangent : TANGENT)
            {
                v2f o;
                float4 alteredObjectVertex = float4(WarpGrass(v.vertex, v.uv.y, normal), 1);
                v.vertex = alteredObjectVertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.screenPos = ComputeScreenPos(o.pos);
                o.objectPos = GenerateWorldOffset(v.vertex);
                o.worldView = WorldSpaceViewDir(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }
            ENDCG
        }

        Pass
        {
            Name "SemiFlatShader"
            Tags 
            { 
                "LightMode"="ForwardBase"
                "RenderType"="Geometry+20"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #pragma multi_compile_local __ _ALPHATEST_ON
            #pragma multi_compile_local __ _NORMALMAP

            #include "UnityCG.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "Assets/Shaders/HelperCgincFiles/ShadingHelper.cginc"
            #include "Assets/Shaders/FolliageHelper.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                SHADOW_COORDS(1)
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                float3 alteredObjectVertex = WarpGrass(v.vertex, v.uv.y, normal);
                o.pos = UnityObjectToClipPos(alteredObjectVertex);
                o.worldPos = mul(unity_ObjectToWorld, alteredObjectVertex);
                TRANSFER_SHADOW(o)
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Threshold;
            float _ThresholdMargin;
            float4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 textureColor = tex2D(_MainTex, i.uv);

                if (textureColor.a < _Threshold)
                    clip(textureColor.a - _Threshold);
                
                if (textureColor.a < _Threshold + _ThresholdMargin)
                    textureColor *= _Color;

                // Shadow
                float inShadow = SHADOW_ATTENUATION(i);
                float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
                float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
                float fadeValue = CompositeShadeFade(inShadow, fadeDistance);

                float4 shadowModifier = float4(1 - _ShadowStrength, 1 - _ShadowStrength, 1 - _ShadowStrength, 1);

                return
                    textureColor * (fadeValue) +
                    textureColor * shadowModifier * (1 - fadeValue);
            }
            ENDCG
        }

        Pass
        {
            Name "PointLights"
            Tags
            {
                "LightMode"="ForwardAdd"
            }

            Blend One One
            ZWrite Off
            ZTest LEqual
            
            CGPROGRAM
            
            #pragma vertex vertWave
            #pragma fragment lightHelperNoOccludeFrag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_shadowcaster

            #include "Assets/Shaders/FolliageHelper.cginc"
            #include "Assets/Shaders/ShaderCgincFiles/SemiFlatShaderAdditive.cginc"

            v2f vertWave (appdata_full v, float3 normal : NORMAL)
            {
                v2f o;
                float4 alteredObjectVertex = float4(WarpGrass(v.vertex, v.texcoord.y, normal), 1);
                o.pos = UnityObjectToClipPos(alteredObjectVertex);
                o.screenPos = ComputeScreenPos(o.pos);
                UNITY_TRANSFER_LIGHTING(o, v.uv1);
                o.worldPos = mul(unity_ObjectToWorld, alteredObjectVertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.objectPos = GenerateWorldOffset(alteredObjectVertex);
                o.uv = v.texcoord;
                o.worldView = WorldSpaceViewDir(alteredObjectVertex);
                return o;
            }
            ENDCG
        }
    }
}
