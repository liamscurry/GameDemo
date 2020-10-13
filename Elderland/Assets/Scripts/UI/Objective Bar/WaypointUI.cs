﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Always stays on the screen and faces the camera. Updates distance to player.
public class WaypointUI : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldPosition;
    [SerializeField]
    private Image directionIndicator;

    private Text distanceText;
    private CanvasScaler canvasScaler;

    private void Awake()
    {
        distanceText = GetComponentInChildren<Text>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
    }

    private void Update()
    {
        if (Camera.current != null)
        {
            Vector3 waypointScreenPosition =
                Camera.current.WorldToViewportPoint(worldPosition);
            Vector2 screenSize = canvasScaler.referenceResolution;
            waypointScreenPosition.x *= screenSize.x;
            waypointScreenPosition.y *= screenSize.y;
            //Debug.Log(waypointScreenPosition.x);
            Vector3 waypointCameraPosition = Camera.current.worldToCameraMatrix.MultiplyPoint(worldPosition);

            Vector3 waypointCameraDirection = waypointCameraPosition.normalized;
            Vector2 projectedWaypointCameraDirection = new Vector2(waypointCameraDirection.x, waypointCameraDirection.y).normalized;

            float radius = canvasScaler.referenceResolution.y * .45f;
            //float radius = Camera.current.pixelHeight * .45f;
            //Vector2 screenSize = new Vector2(Camera.current.pixelWidth, Camera.current.pixelHeight);

            Vector3 waypointClampDirection = 
                new Vector3(screenSize.x / 2, screenSize.y / 2, 0) + 
                new Vector3(projectedWaypointCameraDirection.x * radius, projectedWaypointCameraDirection.y * radius, 0);

            float x1 = .2f;
            float offsetAngle = 1 - Matho.AngleBetween(waypointCameraDirection, Vector3.forward) / 180f;
            float shiftedRadiusPercentage = Mathf.Clamp01((1f / (1 - x1)) * (offsetAngle - x1));
            //if (waypointScreenPosition.x < 0 || waypointScreenPosition.y < 0)
            //    shiftedRadiusPercentage = 1;

            UpdateDirectionIndicator(projectedWaypointCameraDirection, 25f * shiftedRadiusPercentage);
            directionIndicator.color =
            new Color(directionIndicator.color.r,
                        directionIndicator.color.g,
                        directionIndicator.color.b,
                        shiftedRadiusPercentage);


            if ((new Vector2(waypointScreenPosition.x, waypointScreenPosition.y) -
                new Vector2(screenSize.x / 2f, screenSize.y / 2f)).magnitude > radius ||
                waypointCameraPosition.z > 0)
            {   
                waypointScreenPosition = waypointClampDirection;
            }
            else
            {
                directionIndicator.color =
                    new Color(directionIndicator.color.r,
                              directionIndicator.color.g,
                              directionIndicator.color.b,
                              0);
            }

            ((RectTransform) transform).anchoredPosition = waypointScreenPosition;

            UpdateDistanceText();
        }
    }

    private void UpdateDistanceText()
    {
        float distance =
            (PlayerInfo.Player.transform.position - worldPosition).magnitude;

        distanceText.text = Mathf.RoundToInt(distance) + "m";
    }

    private void UpdateDirectionIndicator(Vector2 direction, float radius)
    {
        ((RectTransform) directionIndicator.transform).anchoredPosition = direction * radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(worldPosition, Vector3.one);
    }
}
