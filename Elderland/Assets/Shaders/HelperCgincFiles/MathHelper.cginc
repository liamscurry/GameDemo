#define PI 3.141592

// Angle between working
float AngleBetween(float3 u, float3 v)
{
    float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
    float denominator = length(u) * length(v);
    return acos(numerator / denominator);
}

float4 UVClamp(float4 uv)
{
    float4 newUV = uv; 
    if (newUV.x > 0.5)
        newUV = float4(newUV.x - 0.5, newUV.yzw);
    if (newUV.y > 0.5)
        newUV = float4(newUV.x, newUV.y - 0.5, newUV.zw);
        
    newUV *= float4(2, 2, 1, 1);

    newUV *= float4(0.75, 0.75, 1, 1);
    newUV += float4(0.125, 0.125, 0, 0);

    newUV *= float4(0.5, 0.5, 1, 1);

    if (uv.x > 0.5)
        newUV = float4(newUV.x + 0.5, newUV.yzw);
    if (uv.y > 0.5)
        newUV = float4(newUV.x, newUV.y + 0.5, newUV.zw);
    return newUV;
}