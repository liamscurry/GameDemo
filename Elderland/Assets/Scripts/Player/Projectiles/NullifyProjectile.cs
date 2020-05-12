using UnityEngine;

//Standard projectilie which slows the enemy it hits.

public sealed class NullifyProjectile : ParticleProjectile 
{
    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<NullifyProjectile>(gameObject);
    }

    /*
    protected override void OnCustomDeath(Collider enemy)
    {
        if (enemy != null && enemy.gameObject.tag == TagConstants.EnemyHitbox)
        {
            enemyBuffManager buffs = enemy.transform.parent.GetComponent<enemyManager>().buffs;
            buffs.AddBuff(Slowness.ApplySlowness(4, buffs));
        }
    }
    */
}
