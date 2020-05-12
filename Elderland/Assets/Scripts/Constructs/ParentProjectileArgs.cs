using UnityEngine;

//Type of projectile argument. Holds information on a projectile's parent.

public class ParentProjectileArgs : ProjectileArgs 
{
    public readonly GameObject Parent;

    public ParentProjectileArgs(GameObject parent)
    {
        Parent = parent;
    } 
}
