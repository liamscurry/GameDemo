using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField]
    private float seekRange;
    [SerializeField]
    private float seekDuration;
    [SerializeField]
    private float recycleTime;

    protected bool seekingPlayer;
    private float seekTimer;
    private Vector3 startPosition;
    private float pathRotation;
    private const float pathRotationAmount = 120f;

    protected Rigidbody body;

    public bool SeekingPlayer { get { return seekingPlayer; } }
    protected bool alive;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        seekingPlayer = false;
    }

    private void Update()
    {
        if (seekingPlayer && alive)
        {
            seekTimer += Time.deltaTime;
            Vector3 startOffset =
                (startPosition - PlayerInfo.Player.transform.position).normalized;

            Vector3 normalOffset = 
                Matho.Rotate(Vector3.up, startOffset, pathRotation);

            startOffset = Matho.StandardProjection3D(startOffset).normalized;

            Vector3 lerpPosition =
                startPosition * (1f - seekTimer / seekDuration) +
                PlayerInfo.Player.transform.position * (seekTimer / seekDuration) + 
                Mathf.Sin((Mathf.PI) * (seekTimer / seekDuration)) * normalOffset * 3.5f + 
                Mathf.Sin((Mathf.PI) * (seekTimer / seekDuration)) * startOffset * 3.5f; 
                // want to arc towards player,
                // in addition want enemy to have a fresnel glow with purple edges and
                // player to have particles/glow when receiving the heal.
            body.MovePosition(lerpPosition);
            if (seekTimer >= seekDuration)
            {
                alive = false;
                OnReachPlayer();
                Recycle();
            }
        }
    }

    public static void SpawnPickups<T>(
        GameObject pickupResource,
        Vector3 position,
        int count,
        float speed,
        float maxRandomDegreeShift) where T : Pickup, new()
    {
        for (int i = 0; i < count; i++)
        {
            float angleShift =
                Random.Range(0f, 1f) * maxRandomDegreeShift * Mathf.Deg2Rad;

            T pickup =
                GameInfo.PickupPool.Create<T>(pickupResource, position);
            float angle = (i + 1f) / count * 2 * Mathf.PI;
            pickup.GetComponent<Rigidbody>().velocity =
                speed * new Vector3(Mathf.Cos(angle + angleShift), 0, Mathf.Sin(angle + angleShift));
        }
    }

    protected void Recycle()
    {
        StartCoroutine(RecycleCoroutine());
    }

    private IEnumerator RecycleCoroutine()
    {
        yield return new WaitForSeconds(recycleTime);
        GameInfo.PickupPool.Add<HealthPickup>(gameObject);
    }

    protected abstract void OnReachPlayer();
    public abstract void OnForceRecycle();

    public virtual void Initialize(Vector3 position)
    {
        transform.position = position;
        seekingPlayer = false;
        body.isKinematic = false;
        body.useGravity = true;
        alive = true;
    }

    public void SeekPlayer()
    {
        seekTimer = 0;
        startPosition = transform.position;
        pathRotation = (Random.value - 0.5f) * pathRotationAmount; 

        body.velocity = Vector3.zero;
        body.isKinematic = true;
        body.useGravity = false;
        seekingPlayer = true;
    }

    public virtual bool IsSeekValid()
    {
        return !seekingPlayer && IsInRange() && IsInLineOfSight();
    }

    protected virtual bool IsInRange()
    {
        return Vector3.Distance(
            PlayerInfo.Player.transform.position,transform.position) < seekRange;
    }

    protected virtual bool IsInLineOfSight()
    {
        return !Physics.Linecast(
            PlayerInfo.Player.transform.position,
            transform.position,
            LayerConstants.GroundCollision);
    }
}
