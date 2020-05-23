using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField]
    private float seekRange;
    [SerializeField]
    private float seekDuration;

    private bool seekingPlayer;
    private float seekTimer;
    private Vector3 startPosition;

    private Rigidbody body;

    public bool SeekingPlayer { get { return seekingPlayer; } }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        seekingPlayer = false;
    }

    private void Update()
    {
        if (seekingPlayer)
        {
            seekTimer += Time.deltaTime;
            Vector3 lerpPosition =
                startPosition * (1f - seekTimer / seekDuration) +
                PlayerInfo.Player.transform.position * (seekTimer / seekDuration);
            body.MovePosition(lerpPosition);
            if (seekTimer >= seekDuration)
            {
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

    protected abstract void Recycle();
    protected abstract void OnReachPlayer();
    public abstract void OnForceRecycle();

    public void Reset(Vector3 position)
    {
        transform.position = position;
        seekingPlayer = false;
        body.isKinematic = false;
        body.useGravity = true;
    }

    public void SeekPlayer()
    {
        seekTimer = 0;
        startPosition = transform.position;
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
