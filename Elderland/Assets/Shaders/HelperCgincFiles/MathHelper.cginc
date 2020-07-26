#define PI 3.141592

// Angle between working
float AngleBetween(float3 u, float3 v)
{
    float numerator = (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
    float denominator = length(u) * length(v);
    return acos(numerator / denominator);
}