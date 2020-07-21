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
 
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        /*
        private void OnGUI()
        {
            Event currentEvent = Event.current;
            GUI.Box(new Rect(0,0, Screen.width, Screen.height), "Whole Screen");
            Debug.Log("wow");
            if (currentEvent.button == 0)
            {
                Debug.Log("clicked");
            }
        }*/
    }
}