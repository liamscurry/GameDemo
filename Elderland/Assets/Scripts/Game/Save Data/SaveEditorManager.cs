using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

// Helper class responsible for detecting when scenes are edited during edit mode (not play mode).
// This is needed to reset id's of copied/duplicated objects.
[ExecuteInEditMode]
public class SaveEditorManager : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    private List<(Scene, List<SaveObject>)> lastSceneStates;

    [SerializeField]
    private string lastScene;

    private void Update()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    /*
    Helper method for OnSceneDirty that returns instances of save objects in a scene.
    Tested, verified 9/3/21.

    Inputs:
    Scene : scene : the scene structure to be searched.

    Outputs:
    List<SaveObject> : list of save objects in the scene.
    */
    private List<SaveObject> GetSaveObjects(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        Debug.Log(scene.name + ", root objects: " + rootObjects.Length);
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.transform.GetComponentsInChildren<SaveObject>());
        }
        return saveObjects;
    }

    /*
    Finds all the save objects of all the loaded scenes currently in the hierarchy.
    Tested, verified 9/3/21.

    Inputs:
    None

    Outputs:
    List<(Scene, List<SaveObject>)> : list of scene, save object list pairs.
    */
    private List<(Scene, List<SaveObject>)> GetCurrentHierarchyState()
    {
        List<Scene> openedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isLoaded)
                openedScenes.Add(SceneManager.GetSceneAt(i));
        }

        var newSceneState = new List<(Scene, List<SaveObject>)>();
        foreach (Scene scene in openedScenes)
        {
            var sceneSaveObjects = GetSaveObjects(scene);
            Debug.Log(scene.name + ", save object count:" + sceneSaveObjects.Count);
            foreach (var obj in sceneSaveObjects)
            {
                Debug.Log(obj.GameObject.name);
            }
            newSceneState.Add((scene, sceneSaveObjects));
        }

        return newSceneState;
    }

    private void OnHierarchyChanged()
    {
        GetCurrentHierarchyState();
    }
}