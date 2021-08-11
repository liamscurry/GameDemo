#ifndef LIGHT_HELPER
#define LIGHT_HELPER

/*Pass
{
    //Based on AutodeskInteractive additive forward pass structure in built in shaders.
    Tags
    {
        "LightMode"="ForwardAdd"
    }

    Blend One One
    ZWrite Off
    ZTest LEqual
    
    CGPROGRAM

    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_fwdadd_fullshadows
    #pragma multi_compile_shadowcaster
*/
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "Assets/Shaders/Color.cginc"
    #include "AutoLight.cginc"
    #include "Assets/Shaders/HelperCgincFiles/LODHelper.cginc"
    #include "Assets/Shaders/HelperCgincFiles/MathHelper.cginc"
    #include "Assets/Shaders/HelperCgincFiles/CharacterEffectsHelper.cginc"
    
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR;
    };

    struct v2f
    {
        UNITY_LIGHTING_COORDS(1, 2)
        float4 pos : SV_POSITION;
        float3 worldPos : TEXCOORD3;
        float4 screenPos : TEXCOORD4;
        float3 normal : TEXCOORD5;
        float4 objectPos : TEXCOORD6;
        float2 uv : TEXCOORD7;
        float3 worldView : TEXCOORD8;
    };

    v2f vert (appdata_full v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.screenPos = ComputeScreenPos(o.pos);
        UNITY_TRANSFER_LIGHTING(o, v.uv1);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.objectPos = GenerateWorldOffset(v.vertex);
        o.uv = v.texcoord;
        o.worldView = WorldSpaceViewDir(v.vertex);
        return o;
    }

    float _CrossFade;
    float _WorldMaxHeight;
    sampler2D _MainTex;
    float _Threshold;

    // Point light brightening takes into account light position and normals of object.
    fixed4 lightHelperFrag(v2f i, fixed facingCamera : VFACE) : SV_Target
    {
        float4 textureColor = tex2D(_MainTex, i.uv);
        if (textureColor.a < _Threshold)
            clip(textureColor.a - _Threshold);

        // Learned in AutoLight.cginc
        // Shadow Fade
        float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
        float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
        float fadeValue = UnityComputeShadowFade(fadeDistance);

        // For disabling point lights on a shader.
        if (_ApplyLight < 0.5)
        {
            return fixed4(0,0,0,0);
        }

        float3 fragWorldPosition = mul(unity_ObjectToWorld, i.objectPos);
        ApplyDither(i.screenPos, _CrossFade);
        ApplyCharacterFade(i.objectPos, _WorldMaxHeight);

        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);

        float3 localLightDirection = normalize(i.worldPos - _WorldSpaceLightPos0.xyz);
        float shadowProduct = AngleBetween(i.normal, localLightDirection) / 3.151592;
        //return length(i.worldPos - _WorldSpaceLightPos0.xyz) / 20;
        float attenuationFade = 1 - saturate(fadeValue - 0.2);

        // Light cutoff blending
        float lightCutoff = 0.4;
        float darkCutoff = 0.5;
        float lightDarkPercentage;
        if (shadowProduct < lightCutoff)
        {
            lightDarkPercentage = 0;
        }
        else if (shadowProduct > darkCutoff)
        {
            lightDarkPercentage = 1;
        }
        else
        {
            float percentage =
                (shadowProduct - lightCutoff) / (darkCutoff - lightCutoff);
            lightDarkPercentage = percentage;
        }

        return attenuation * lightDarkPercentage * _LightColor0 * attenuationFade;
    }

    // Point lightbrightening without normal based darkening. (pixels in area are lit by light no matter what)
    fixed4 lightHelperNoOccludeFrag(v2f i, fixed facingCamera : VFACE) : SV_Target
    {
        float4 textureColor = tex2D(_MainTex, i.uv);
        if (textureColor.a < _Threshold)
            clip(textureColor.a - _Threshold);

        // Learned in AutoLight.cginc
        // Shadow Fade
        float zDistance = length(mul(UNITY_MATRIX_V, (_WorldSpaceCameraPos - i.worldPos.xyz)));
        float fadeDistance = UnityComputeShadowFadeDistance(i.worldPos.xyz, zDistance);
        float fadeValue = UnityComputeShadowFade(fadeDistance);

        // For disabling point lights on a shader.
        if (_ApplyLight < 0.5)
        {
            return fixed4(0,0,0,0);
        }

        float3 fragWorldPosition = mul(unity_ObjectToWorld, i.objectPos);
        ApplyDither(i.screenPos, _CrossFade);
        ApplyCharacterFade(i.objectPos, _WorldMaxHeight);

        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);

        float3 localLightDirection = normalize(i.worldPos - _WorldSpaceLightPos0.xyz);
        float shadowProduct = AngleBetween(i.normal, localLightDirection) / 3.151592;
        //return length(i.worldPos - _WorldSpaceLightPos0.xyz) / 20;
        float attenuationFade = 1 - saturate(fadeValue - 0.2);

        return attenuation * _LightColor0 * attenuationFade;
    }

/*   ENDCG
}*/

/*
For quick access
Pass
{
    //Based on AutodeskInteractive additive forward pass structure in built in shaders.
    Tags
    {
        "LightMode"="ForwardAdd"
    }

    Blend One One
    ZWrite Off
    ZTest LEqual
    
    CGPROGRAM

    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_fwdadd_fullshadows
    #pragma multi_compile_shadowcaster
    #include "/HelperCgincFiles/LightHelper.cginc"
    ENDCG
}
*/

#endif