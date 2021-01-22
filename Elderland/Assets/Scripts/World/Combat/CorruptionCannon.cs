using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionCannon : LevelMechanic
{
    [SerializeField]
    private float shootDelay;
    [SerializeField]
    private float shootAmount;
    [SerializeField]
    private float shootMargin;
    [SerializeField]
    private float shootSpeed;
    [SerializeField]
    private float shootLifetime;
    [SerializeField]
    private float shootWallSpawnOffset;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= shootDelay)
        {
            timer = 0;
            ShootProjectiles();   
        }
    }

    public override void InvokeSelf()
    {
        this.enabled = true;
    }

    public override void ResetSelf()
    {
        timer = 0;
        this.enabled = false;
    }

    public override void DisableSelf()
    {
        ResetSelf();
    }

    private void ShootProjectiles()
    {
        Vector3 centerOffset = (shootAmount - 1) * shootMargin / 2f * transform.right;
        centerOffset -= transform.forward * shootWallSpawnOffset;
        for (int i = 0; i < shootAmount; i++)
        {
            Vector3 offset = 
                i * shootMargin * transform.right;
            offset -= centerOffset;
            GameInfo.ProjectilePool.Create<RangedEnemyProjectile>(
                    Resources.Load<GameObject>(ResourceConstants.Enemy.Projectiles.RangedEnemyProjectile),
                    transform.position + offset,
                    transform.forward * shootSpeed,
                    shootLifetime,
                    TagConstants.PlayerHitbox,
                    OnHit,
                    null);
        }
    }

    public bool OnHit(GameObject character)
    {
        if (character != null)
        {
            character.GetComponentInParent<PlayerManager>().ChangeHealth(-1);
            return true;
        }
        else
        {
            return false;
        }
    }
}
