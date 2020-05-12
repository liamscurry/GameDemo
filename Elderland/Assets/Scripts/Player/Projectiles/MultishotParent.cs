using UnityEngine;

//Standard projectile which spawns an arc of smaller projectiles on impact

public sealed class MultishotParent : ParticleProjectile
{
    public override void Recycle()
    {
        GameInfo.ProjectilePool.Add<MultishotParent>(gameObject);
    }

    /*
    protected override void OnCustomDeath(Collider enemy)
    {       
        if (enemy == null && normal != Vector3.zero)
        {
            //Direction opposite to wall normal
            //float centerDirection = Matho.Angle(Matho.Reflect(-velocity, normal));

            //Spawns an arc of projectiles around centerDirection
            for (int i = 0; i < 3; i++)
            {
                //Vector3 velocity = 7 * Matho.DirectionVectorFromAngle(centerDirection - 15 + (15 * i));
                Vector3 velocity = Vector3.zero;

                PlayerInfo.ProjectilePool.Create<MultishotChild>(
                    Resources.Load<GameObject>(ResourceConstants.Player.Projectiles.MultishotChild),
                    transform.position,
                    velocity,
                    2,
                    ProjectileArgs.Empty);
            }
        }
    }
    */
}