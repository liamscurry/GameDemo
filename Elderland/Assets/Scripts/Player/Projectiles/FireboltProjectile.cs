using UnityEngine;

//Standard projectile with no unique properties.

public sealed class FireboltProjectile : ParticleProjectile 
{
    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<FireboltProjectile>(gameObject);
    }
}