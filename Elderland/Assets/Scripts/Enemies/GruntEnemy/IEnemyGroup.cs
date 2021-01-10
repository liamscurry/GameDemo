using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyGroup : IComparable<IEnemyGroup>
{
    EnemyGroup Group { get; set; }
    Vector3 Position { get; }
    Vector3 Velocity { get; set; }
    List<IEnemyGroup> NearbyEnemies { get; set; }
}