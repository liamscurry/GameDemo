using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pottery : MonoBehaviour
{
    [SerializeField]
    private GameObject breakParticle;
    [SerializeField]
    private int healthPickupCount;
    [SerializeField]
    private float destroyDuration;

    public void Break()
    {
        // PlayerInfo.Player.transform.forward
        /*
        Vector3 particleDirection =
            (transform.position - PlayerInfo.Player.transform.position).normalized;
        breakParticle.transform.rotation =
            Quaternion.LookRotation(particleDirection, Vector3.up);
        */
        
        if (healthPickupCount > 0)
        {
            Pickup.SpawnPickups<HealthPickup>(
                Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
                transform.position,
                healthPickupCount,
                3f,
                90f);
        }

        StartCoroutine(DestroyAfterBreakCoroutine());
    }

    private IEnumerator DestroyAfterBreakCoroutine()
    {
        yield return new WaitForSeconds(destroyDuration);
        GameObject.Destroy(gameObject);
    }
}
