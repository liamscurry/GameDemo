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
                        if (Event.current.modifiers == EventModifiers.Shift)
                        {
                            DeleteVine((VinePlacer) target, hitInfo.point, 2);
                        }
                        else
                        {
                            CreateVine((VinePlacer) target, hitInfo.point, hitInfo.normal);
                        }
                    }
                }
            }
        }

        private void CreateVine(VinePlacer vinePlacer, Vector3 position, Vector3 normal)
        {
            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject healthPickupPrefab = 
                Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup);
            GameObject instanced = 
                GameObject.Instantiate(healthPickupPrefab, position, normalRotation);
            instanced.transform.parent = ((VinePlacer) target).gameObject.transform;
        }

        private void DeleteVine(VinePlacer vinePlacer, Vector3 position, float radius)
        {
            Collider[] vines =
                Physics.OverlapSphere(position, radius);
            
            for (int i = vines.Length - 1; i >= 0; i--)
            {
                if (vines[i].gameObject.transform.IsChildOf(vinePlacer.transform))
                {
                    GameObject.DestroyImmediate(vines[i].gameObject);
                }
            }
        }
    }
}