using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Normally would move rigid body using rigidbody.MovePosition,
// but already made level content with current set up. As a result will use transform.position
// instead of changing speeds in level.
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

    private float DeltaTimeModifier
    {
        get { return Time.deltaTime * 60f * 0.85f; }
    }

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
            transform.position = 
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    liftSpeed * DeltaTimeModifier);
        }
    }

    public void Fall()
    {
        if (transform.position.y > closedPosition.y)
        {
            float playerDashModifier = 1;
            if (PlayerInfo.AbilityManager.dash != null &&
                PlayerInfo.AbilityManager.CurrentAbility == PlayerInfo.AbilityManager.dash)
            {
                playerDashModifier = 0.0f;
            }

            transform.position = 
                Vector3.MoveTowards(
                    transform.position,
                    closedPosition,
                    fallSpeed * playerDashModifier * DeltaTimeModifier);
        }
        else
        {
            body.position = closedPosition;
        }
    }

    public void CloseInstantly()
    {
        //body.position = closedPosition;
        transform.position = closedPosition;
    }

    public void OnRespawn(object sender, EventArgs e)
    {
        CloseInstantly();
    }
}
