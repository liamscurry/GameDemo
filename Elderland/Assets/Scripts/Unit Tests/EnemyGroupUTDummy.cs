using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Dummy concrete interface for UT purposes.

public sealed class EnemyGroupUTDummy : IEnemyGroup
{
    // Properties
    public EnemyGroup Group { get; set; }
    public Vector3 Position { get { return position; } }
    public Vector3 Velocity { get; set; }
    public List<IEnemyGroup> NearbyEnemies { get; set; }

    // Fields
    private Vector3 position;

    // Constructors
    public EnemyGroupUTDummy()
    {
        NearbyEnemies = new List<IEnemyGroup>();
    }

    public EnemyGroupUTDummy(Vector3 position)
    {
        this.position = position;
        NearbyEnemies = new List<IEnemyGroup>();
    }

    public int CompareTo(IEnemyGroup e)
    {
        if (this == e)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}