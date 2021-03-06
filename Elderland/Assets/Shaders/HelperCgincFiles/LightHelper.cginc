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
    #include "Color.cginc"
    #include "AutoLight.cginc"
    #include "LODHelper.cginc"
    #include "/HelperCgincFiles/MathHelper.cginc"
    
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
        float3 objectPos : TEXCOORD6;
    };

    v2f vert (appdata_full v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.screenPos = ComputeScreenPos(o.pos);
        UNITY_TRANSFER_LIGHTING(o, v.uv1);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.objectPos = v.vertex;
        return o;
    }

    float _CrossFade;

    fixed4 frag(v2f i, fixed facingCamera : VFACE) : SV_Target
    {
        if (_ApplyLight < 0.5)
        {
            return fixed4(0,0,0,0);
        }

        float3 fragWorldPosition = mul(unity_ObjectToWorld, i.objectPos);
        ApplyDither(i.screenPos, _CrossFade);
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
        float3 localLightDirection = normalize(i.worldPos - _WorldSpaceLightPos0.xyz);
        float shadowProduct = AngleBetween(i.normal, localLightDirection) / 3.151592;
        //return length(i.worldPos - _WorldSpaceLightPos0.xyz) / 20;
        if (shadowProduct > 0.5)
        {
            // in light
            return attenuation * _LightColor0;
        }
        else
        {
            return fixed4(0,0,0,0);
        }
        return shadowProduct;
        return attenuation * _LightColor0;
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