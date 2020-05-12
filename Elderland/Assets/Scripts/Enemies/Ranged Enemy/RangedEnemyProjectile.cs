using UnityEngine;

//Standard projectile with no unique properties.

public sealed class RangedEnemyProjectile : ParticleProjectile 
{
    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<RangedEnemyProjectile>(gameObject);
    }
}