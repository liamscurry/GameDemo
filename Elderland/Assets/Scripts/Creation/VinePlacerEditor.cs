using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    #if (UNITY_EDITOR)
    [ExecuteInEditMode]
    [CustomEditor(typeof(VinePlacer))]
    public class VinePlacerEditor : Editor
    {
        void OnSceneGUI()
        {
            // Learned in "Disable mouse selection in editor view" unity answer.
            int eventID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.type == EventType.MouseDown && ((VinePlacer) target).EditorOn)
            {
                if (Event.current.button == 0 && Event.current.isMouse)
                {
                    // Learned in "Disable mouse selection in editor view" unity answer.
                    GUIUtility.hotControl = eventID;
                    Event.current.Use();

                    Ray mousePositionRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    bool hit;
        
                    VinePlacer vinePlacer = (VinePlacer) target;

                    if (Event.current.modifiers == EventModifiers.Shift ||
                        Event.current.modifiers == EventModifiers.Control ||
                        Event.current.modifiers == EventModifiers.Alt)
                    {
                        hit = Physics.Raycast(mousePositionRay, out hitInfo, 100f, LayerConstants.Folliage);
                    }
                    else
                    {
                        hit = Physics.Raycast(mousePositionRay, out hitInfo, 100f, LayerConstants.GroundCollision);
                    }

                    if (Event.current.modifiers == EventModifiers.Shift)
                    {
                        DeleteVine((VinePlacer) target, hitInfo.point, vinePlacer.DeletionRadius);
                    }
                    else if (Event.current.modifiers == EventModifiers.Control)
                    {
                        DeleteSpecificVine(
                            vinePlacer,
                            hitInfo.point,
                            vinePlacer.DeletionRadius,
                            vinePlacer.SelectedPrefab.name);
                    }
                    else if (Event.current.modifiers == EventModifiers.Alt)
                    {
                        DeleteAllVines((VinePlacer) target);
                    }
                    else
                    {
                        CreateVine((VinePlacer) target, hitInfo.point, hitInfo.normal);
                    }
                }
            }
        }

        private void CreateVine(VinePlacer vinePlacer, Vector3 position, Vector3 normal)
        {
            Quaternion normalRotation =
                Quaternion.FromToRotation(Vector3.up, normal) *
                Quaternion.Euler(0, vinePlacer.NormalRotation * (1 + ((Random.value - 0.5f) / 0.5f) * vinePlacer.NormalRotationRandom), 0);

            GameObject instanced = 
                GameObject.Instantiate(vinePlacer.SelectedPrefab, position + normal * vinePlacer.NormalOffset, normalRotation);
            instanced.transform.parent = vinePlacer.gameObject.transform;
            instanced.transform.localScale *=
                vinePlacer.ScaleMultiplier * (1 + ((Random.value - 0.5f) / 0.5f) * vinePlacer.ScaleRandom);

            LODGroup lodGroup = vinePlacer.GetComponent<LODGroup>();
            LOD[] lods = lodGroup.GetLODs();
            List<Renderer> nearRenderers = new List<Renderer>(lods[0].renderers);
            Renderer[] newRenderers = instanced.GetComponents<Renderer>();
            nearRenderers.AddRange(newRenderers);
            newRenderers = instanced.GetComponentsInChildren<Renderer>();
            nearRenderers.AddRange(newRenderers);
            lods[0].renderers = nearRenderers.ToArray();
            
            lodGroup.SetLODs(lods);
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

        private void DeleteSpecificVine(VinePlacer vinePlacer, Vector3 position, float radius, string typeName)
        {
            Collider[] vines =
                Physics.OverlapSphere(position, radius);
            
            for (int i = vines.Length - 1; i >= 0; i--)
            {
                if (vines[i].gameObject.transform.IsChildOf(vinePlacer.transform) &&
                    vines[i].gameObject.name.Contains(typeName))
                {
                    GameObject.DestroyImmediate(vines[i].gameObject);
                }
            }
        }

        private void DeleteAllVines(VinePlacer vinePlacer)
        {
            for (int i = vinePlacer.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(vinePlacer.transform.GetChild(i).gameObject);
            }
        }
    }
    #endif
}