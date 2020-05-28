using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingDoor : MonoBehaviour
{
    [SerializeField]
    private float liftHeight;
    [SerializeField]
    private float liftSpeed;
    [SerializeField]
    private float fallSpeed;

    private Vector3 closedPosition;
    private Rigidbody body;

    private void Start()
    {
        closedPosition = transform.position;
        GameInfo.Manager.OnRespawn += OnRespawn;
        body = GetComponent<Rigidbody>();
    }   

    private void Update()
    {
        Fall();
    }

    // Should only be called by a hold interaction.
    public void Open()
    {
        if (transform.position.y < closedPosition.y + liftHeight)
        {
            Vector3 targetPosition =
                closedPosition + Vector3.up * liftHeight;
            body.MovePosition(
                Vector3.MoveTowards(transform.position, targetPosition, liftSpeed));
        }
    }

    public void Fall()
    {
        if (transform.position.y > closedPosition.y)
        {
            body.MovePosition(
                Vector3.MoveTowards(transform.position, closedPosition, fallSpeed));
        }
        else
        {
            body.MovePosition(closedPosition);
        }
    }

    public void CloseInstantly()
    {
        transform.position = closedPosition;
    }

    public void OnRespawn(object sender, EventArgs e)
    {
        CloseInstantly();
    }
}
