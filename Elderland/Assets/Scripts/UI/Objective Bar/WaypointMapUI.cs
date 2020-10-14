using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMapUI : MonoBehaviour
{
    [SerializeField]
    private WaypointUI waypoint;

    private MapMenuUI menuUI;
    
    private void Awake()
    {
        menuUI = GetComponentInParent<MapMenuUI>();
    }

    private void OnEnable()
    {
        menuUI.CalculateCoordinateConversion();
        ((RectTransform) transform).anchoredPosition = 
            menuUI.WorldToUIPosition(waypoint.WorldPosition);
    }
}
