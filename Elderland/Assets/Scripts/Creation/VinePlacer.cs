using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [ExecuteInEditMode]
    public class VinePlacer : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefabObject1;
        [SerializeField]
        private GameObject prefabObject2;
        [SerializeField]
        private GameObject prefabObject3;
        [SerializeField]
        [Range(1, 3)]
        private int selectedPrefab;
        [SerializeField]
        [Range(0, 5)]
        private float scaleMultiplier = 1;
        [SerializeField]
        private float scaleRandom = 0;
        [SerializeField]
        private float normalRotation;
        [SerializeField]
        private float normalRotationRandom;
        [SerializeField]
        [Range(0, 5)]
        private float deletionRadius = 1;
        //[SerializeField]
        //private GameObject viewObject;

        public GameObject SelectedPrefab 
        { 
            get
            { 
                switch (selectedPrefab)
                {
                    case 1:
                        return prefabObject1;
                    case 2:
                        return prefabObject2;
                    case 3:
                        return prefabObject3;
                    default:
                        throw new System.Exception("Not a valid selected prefab index");
                }
            }
        }

        public float ScaleMultiplier { get { return scaleMultiplier; } }
        public float ScaleRandom { get { return scaleRandom; } }
        public float NormalRotation { get { return normalRotation; } }
        public float NormalRotationRandom { get { return normalRotationRandom; } }
 
        public float DeletionRadius { get { return deletionRadius; } }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("view mode");
        }

        /*private void OnGUI()
        {
            Event currentEvent = Event.current;
            GUI.Box(new Rect(0,0, Screen.width, Screen.height), "Whole Screen");
            Debug.Log("wow");
            if (currentEvent.button == 0)
            { 
                Debug.Log("clicked");
            }
        }*/
   
        private void OnDrawGizmosSelected()
        {
            /*
            Ray mousePositionRay = Camera.current.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            //Debug.Log(mousePositionRay.origin);
            if (Physics.Raycast(mousePositionRay, out hitInfo, 100f, LayerConstants.GroundCollision))
            {
                
                Gizmos.color = new Color(0.5f, 0, 0.76f, 0.5f);
                Gizmos.DrawSphere(hitInfo.point, 2);
            }
            */
        }
    }
}