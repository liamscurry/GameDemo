using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(VinePlacer))]
    public class VinePlacerEditor : Editor
    {
        void OnSceneGUI()
        {
            // Learned in "Disable mouse selection in editor view" unity answer.
            int eventID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0 && Event.current.isMouse)
                {
                    // Learned in "Disable mouse selection in editor view" unity answer.
                    GUIUtility.hotControl = eventID;
                    Event.current.Use();

                    Ray mousePositionRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(mousePositionRay, out hitInfo, 100f, LayerConstants.GroundCollision))
                    {
                        //Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal, Color.magenta, 5f);
                        Debug.Log(((VinePlacer) target));
                        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                        GameObject healthPickupPrefab = 
                            Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup);
                        GameObject instanced = 
                            GameObject.Instantiate(healthPickupPrefab, hitInfo.point, normalRotation);
                        instanced.transform.parent = ((VinePlacer) target).gameObject.transform;
                        Debug.Log(hitInfo.transform.name);
                        //Debug.Log(hitInfo.point);
                    }
                }
            }
        }
    }
}