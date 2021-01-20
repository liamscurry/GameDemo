using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Texture 3D generator class based on 3D textures unity manual.

public class CorruptionTextureGenerator : MonoBehaviour
{
    [SerializeField]
    private Material materialToClone;

    private static int count;
    private static readonly int maxCount = 10000;
    [ContextMenu("Generate Texture")]
    private void GenerateTexture()
    {
        int size = 256;
        TextureFormat textureFormat = TextureFormat.RGBA32;
        TextureWrapMode textureWrapMode = TextureWrapMode.Mirror;

        Texture3D volumeTexture = 
            new Texture3D(size, size, size, textureFormat, false);
        // Will use mipmaps later for performance
        volumeTexture.wrapMode = textureWrapMode;

        // Generate colors
        Color[] colors = new Color[size * size * size];
        for (int i = 0; i < size; i++)
        {
            int zStart = i * size * size;
            for (int j = 0; j < size; j++)
            {
                int yStart = j * size;
                for (int k = 0; k < size; k++)
                {
                    GenerateColors(ref colors, size, i, j, k, k + yStart + zStart);
                }
            }
        }
        
        volumeTexture.SetPixels(colors);
        volumeTexture.Apply();

        var meshRenderer =
            GetComponent<MeshRenderer>();
        
        string path = 
            "Assets/Sprites/Geometry/Walls/Corruption Wall/Generated Textures/Corruption";
        string texturePath = path + "Texture" + count;
        texturePath += ".asset";

        string materialPath = path + "Material" + count;
        materialPath += ".asset";
        count = (count + 1) % maxCount;

        AssetDatabase.CreateAsset(volumeTexture, texturePath);
        Texture3D instancedTexture =
            AssetDatabase.LoadAssetAtPath<Texture3D>(texturePath);

        Material clonedMaterial =
            new Material(materialToClone);
        clonedMaterial.SetTexture("_VolumeTexture", instancedTexture);
        
        AssetDatabase.CreateAsset(clonedMaterial, materialPath);
        Material instancedMaterial =
            AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        
        meshRenderer.material = instancedMaterial;
    }

    private void GenerateColors(ref Color[] colors, int size, int i, int j, int k, int index)
    {
        float noise = 
            Mathf.PerlinNoise(i * 1.0f / size * 10, k * 1.0f / size * 10);
        float cutoff = 1;
        if (j * 1.0f / size > 0.5f + noise * 0.1f)
            cutoff = 0;
        colors[index] =
            new Color(cutoff, 0, 0, cutoff);
        //new Color(i * 1.0f / size, j * 1.0f / size, k * 1.0f / size, 1);
    }
}
