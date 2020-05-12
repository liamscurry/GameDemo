//Learned from UnityCG.cginc
#define Distance(position) (pow(pow(position.x, 2) + pow(position.y, 2) + pow(position.z, 2), 0.5))

//Color lerp fog
#define ApplyFog(color, fogColor, distance, maxDistance) lerp(color, fogColor, saturate(distance / maxDistance))

#define AngleBetween(v1, v2) degrees(acos(saturate(((v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z)) / (length(v1) * length(v2)))))

//Alpha fog
//#define ApplyFog(color, distance, maxDistance) color.w = 1 - saturate(distance / maxDistance)
//Alternate version based on color lerp
//color.xyz = (lerp(color.xyz, fogColor.xyz, saturate(distance / maxDistance)))