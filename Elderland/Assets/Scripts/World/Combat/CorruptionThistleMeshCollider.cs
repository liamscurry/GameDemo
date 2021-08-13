using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Structure used to tell the mesh generation what it is colliding with.
public class CorruptionThistleMeshCollider
{
    public float Radius { get; private set; }
    public Vector3 Position { get; set; }
    public float Strength { get; private set; } // how far the collider should push the thistle (when at weight = 1).
    public float Weight { get; set; } // 0 is off, 1 is on. How strong is the effect.

    public CorruptionThistleMeshCollider(float radius, float strength, Vector3 position)
    {
        Radius = radius;
        Position = position;
        Strength = strength;
        Weight = 1;
    }
}