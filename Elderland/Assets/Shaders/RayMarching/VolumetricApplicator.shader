Shader "Custom/VolumetricApplicator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SunColor ("SunColor", Color) = (1,1,1,1)
        
        _SunFogIntensity("SunFogIntensity", Float) = 0.0
        _MoonFogIntensity("MoonFogIntensity", Float) = 0.0
        _SunColorIntensity("SunColorIntensity", Float) = 0.0
        _MoonColorIntensity("MoonColorIntensity", Float) = 0.0
        _SunExpansionIntensity("SunExpansionIntensity", Float) = 0.0
        _MoonExpansionIntensity("MoonExpansionIntensity", Float) = 0.0
        _SunColorTransition("SunColorTransition", Float) = 0.0
        _MoonColorTransition("MoonColorTransition", Float) = 0.0
        _SunSaturationTransition("SunSaturationTransition", Float) = 0.0

        //Color temperatures.
        _SunColor0("_SunColor0", Color) = (1,1,1,1)
        _SunColor1("_SunColor1", Color) = (1,1,1,1)
        _SunColor2("_SunColor2", Color) = (1,1,1,1)
        _SunColor3DayTime("_SunColor3DayTime", Color) = (1,1,1,1)
        _SunColor3NightTime("_SunColor3NightTime", Color) = (1,1,1,1)
        _SunColor4DayTime("_SunColor4DayTime", Color) = (1,1,1,1)
        _SunColor4NightTime("_SunColor4NightTime", Color) = (1,1,1,1)

        _MoonColor ("MoonColor", Color) = (1,1,1,1)

        //Color percentages
        _SunColor1Percentage("_SunColor1Percentage", Range(0.0,1.0)) = 0
        _SunColor2Percentage("_SunColor2Percentage", Range(0.0,1.0)) = 0
        _SunColor3Percentage("_SunColor3Percentage", Range(0.0,1.0)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            // Angle between working
            #define Pi 3.151592

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float3 ray : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                float3 rayOriginal = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex);
                o.ray = rayOriginal;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _OcclusionTexture;
            float4 _SunColor;
            float4 _MoonColor;
            float4 _SunDirection;
            float4 _MoonDirection;
            float _SunFogIntensity;
            float _MoonFogIntensity;
            float _SunSaturationTransition;

            float4 _SunColor0;
            float4 _SunColor1;
            float _SunColor1Percentage;
            float4 _SunColor2;
            float _SunColor2Percentage;
            float4 _SunColor3DayTime;
            float4 _SunColor3NightTime;
            float _SunColor3Percentage;
            float4 _SunColor4DayTime;
            float4 _SunColor4NightTime;
            float _SunColorIntensity;
            float _MoonColorIntensity;
            float _SunColorTransition;
            float _MoonColorTransition;

            float _SunExpansionIntensity;
            float _MoonExpansionIntensity;

            // 1 is center
            // 0 is edge of effect
            float4 SunGradientColorRamp(float percentage, float sunFactor)
            {
                float alteredSunColor1Percentage = 
                    _SunColor1Percentage * (_SunExpansionIntensity) + saturate(_SunColor1Percentage - 0.025) * (1 - _SunExpansionIntensity);

                float alteredSunColor2Percentage = 
                    _SunColor2Percentage * (_SunExpansionIntensity) + saturate(_SunColor2Percentage + 0.1) * (1 - _SunExpansionIntensity);// - 0.6
                //alteredSunColor2Percentage = _SunColor2Percentage;

                float alteredSunColor3Percentage = 
                    _SunColor3Percentage * (_SunExpansionIntensity) + saturate(_SunColor3Percentage) * (1 - _SunExpansionIntensity);

                float4 sunColor3 = _SunColor3DayTime * (sunFactor) + _SunColor3NightTime * (1 - sunFactor);
                float4 sunColor4 = _SunColor4DayTime * (sunFactor) + _SunColor4NightTime * (1 - sunFactor);
                //return sunColor3;

                percentage = 1 - percentage;
                if (percentage < alteredSunColor1Percentage)
                {
                    //Lerp between 0 and 1 colors
                    float segmentPercentage = percentage / alteredSunColor1Percentage;
                    return _SunColor0 * (1 - segmentPercentage) + _SunColor1 * segmentPercentage;
                }
                else if (percentage > alteredSunColor3Percentage)
                {
                    //Lerp between 3 and 4 colors
                    float segmentPercentage = (percentage - alteredSunColor3Percentage) / (1 - alteredSunColor3Percentage);
                    return sunColor3 * (1 - segmentPercentage) + sunColor4 * segmentPercentage;
                }
                else
                {
                    if (percentage < alteredSunColor2Percentage)
                    {
                        //Lerp between 1 and 2 colors
                        float segmentPercentage = (percentage - alteredSunColor1Percentage) / (alteredSunColor2Percentage - alteredSunColor1Percentage);
                        return _SunColor1 * (1 - segmentPercentage) + _SunColor2 * segmentPercentage;
                    }
                    else
                    {
                        //Lerp between 2 and 3 colors
                        float segmentPercentage = (percentage - alteredSunColor2Percentage) / (alteredSunColor3Percentage - alteredSunColor2Percentage);
                        return _SunColor2 * (1 - segmentPercentage) + sunColor3 * segmentPercentage;
                    }
                }
            }

            float4 MoonGradientColorRamp(float percentage)
            {
                //float4(_MoonColor.rgb, 1) * moonHalo 
                //return _MoonColor;
                return (_MoonColor * (1 - percentage) + float4(1,1,1,1) * percentage);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //return fixed4(1,1,0,1);
                float2 uv = i.screenPos.xy / i.screenPos.w;
                //return uv.y;

                fixed4 col = tex2D(_MainTex, uv);
                // just invert the colors
                //col.rgb = 1 - col.rgb;
                //return col;
                float sunHalo = pow(AngleBetween(_MoonDirection.xyz, i.ray) / Pi, 4);
                float sunBaseFactor = saturate(1 - pow(AngleBetween(_MoonDirection.xyz, float3(0,1,0)) / (Pi / 2), 1) + 0.0);
                //sunHalo = sunHalo * (1 - sunBaseFactor) + (0.0 * sunHalo) * (sunBaseFactor);
                float sunFactor = saturate(1 - pow(AngleBetween(_MoonDirection.xyz, float3(0,1,0)) / (Pi / 2), 20) + 0.8);
                //return float4(sunHalo,sunHalo,sunHalo, 1);
                //return float4(sunBaseFactor, sunBaseFactor, sunBaseFactor, 1);
                float moonHalo = pow(AngleBetween(_SunDirection.xyz, i.ray) / (Pi), 4);
                //float moonBaseFactor = pow(AngleBetween(_MoonDirection.xyz, i.ray) / (Pi / 2), 3);
                float moonFactor = 1 - sunFactor;

                //return float4(_SunColorIntensity, _SunColorIntensity, _SunColorIntensity, 1);
                //return float4(sunHalo, sunHalo, sunHalo, 1);
                //return SunGradientColorRamp(sunHalo);
                //return float4(_SunColorIntensity, _SunColorIntensity, _SunColorIntensity, 1);
                //return MoonGradientColorRamp(moonHalo);
                float4 baseFog = float4(0,0,0, (1 - col.r) * (1 - sunBaseFactor));
                float4 sunFogColor = float4(1, 1, 1, pow(sunHalo, 17) * 1.6 + 0.7) * _SunColorIntensity + SunGradientColorRamp(sunHalo, _SunColorTransition) * (1 - _SunColorIntensity);
                
                //float4 sunFogColor = float4(1, 1, 1, sunHalo) * _SunColorIntensity + float4(1,1,1, sunHalo * 1.1) * SunGradientColorRamp(sunHalo, _SunColorTransition) * (1 - _SunColorIntensity);
                
                float4 sunFog = sunFogColor * float4(col.r, col.r, col.r, .4);//(float4(-.2, -.2, -.2, 1) * (1 - col.r) + 
                float4 moonFogColor = float4(1,1,1,1) * MoonGradientColorRamp(pow(moonHalo,4));
                float4 moonFog = moonFogColor * float4(col.r, col.r, col.r, .4);//float4(-.2,-.2,-.2, 1) * (1 - col.r) + 

                float moonMask = 1 - (AngleBetween(-_SunDirection.xyz, i.ray) / (Pi) > 0.017);
                float moonMaskFactor = saturate(1 - pow(AngleBetween(_SunDirection.xyz, float3(0,1,0)) / (Pi / 2), 12) + 0);
                //moonMask = moonMask * moonMaskFactor;
                float4 moonColor = float4(moonMask, moonMask, moonMask, moonMask) * moonMaskFactor;
                //return moonColor;

                float sunMask = 1 - (AngleBetween(_SunDirection.xyz, i.ray) / (Pi) > 0.012);
                float sunMaskFactor = saturate(1 - pow(AngleBetween(-_SunDirection.xyz, float3(0,1,0)) / (Pi / 1.95), 16) + 0);
                float4 sunColor = float4(sunMask, sunMask, sunMask, sunMask) * sunMaskFactor;

                //return sunFog;
                //return sunFogColor;
                //return (baseFog + sunFog * sunFactor);// * (_SunFogIntensity + (1 - _SunColorIntensity) * .1)
                float sunFogIntensityDayNightFactor = (1 + pow(sunHalo, 9) * 2.5) * (_SunSaturationTransition) + ((1 + pow(sunHalo,1) * .7) * (1 + pow(sunHalo, 9) * 2.5)) * (1 - _SunSaturationTransition);
                return (sunFog * sunFactor) * (_SunFogIntensity * sunFogIntensityDayNightFactor) * _SunColor.a + (moonFog * moonFactor) * _MoonFogIntensity * _MoonColor.a + moonColor + sunColor;
            }
            ENDCG
        }
    }
}
