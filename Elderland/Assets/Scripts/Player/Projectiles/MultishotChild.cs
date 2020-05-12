using UnityEngine;

//Standard projectile spawned from a MultishotParent projectile.

public sealed class MultishotChild : ParticleProjectile
{
    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<MultishotChild>(gameObject);
    }
}