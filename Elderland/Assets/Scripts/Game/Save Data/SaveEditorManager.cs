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

// As of right now, if you accidently remove a save object, unload the scene (or quit unity) and readd it
// the id of the save object is lost. Control z after accidently deleting this object works fine though (TODO).
[ExecuteInEditMode]
public class SaveEditorManager : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    private List<(Scene, string, List<SaveObject>)> lastSceneStates;

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

        //Debug.Log(scene.name + ", root objects: " + rootObjects.Length);

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
    List<(Scene, string, List<SaveObject>)> : list of scene, scene name, save object list pairs.
    */
    private List<(Scene, string, List<SaveObject>)> GetCurrentHierarchyState()
    {
        List<Scene> openedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).isLoaded)
                openedScenes.Add(SceneManager.GetSceneAt(i));
        }

        var newSceneState = new List<(Scene, string,List<SaveObject>)>();
        foreach (Scene scene in openedScenes)
        {
            var sceneSaveObjects = GetSaveObjects(scene);

            /*
            Debug.Log(scene.name + ", save object count:" + sceneSaveObjects.Count);
            foreach (var obj in sceneSaveObjects)
            {
                Debug.Log(obj.GameObject.name);
            }
            */

            newSceneState.Add((scene, scene.name, sceneSaveObjects));
        }

        return newSceneState;
    }

    /*
    Detects when an existing scene gains a new save object object. If so, calls the reset ID function
    of the save object.

    Inputs:
    Scene : scene : the scene to scan for new save objects.
    List<SaveObject> : currentSaveObjects : list of save objects currently in the scene 'scene.'

    Outputs:
    None
    */
    private void DetectNewSaveObjects(Scene scene, List<SaveObject> newSaveObjects)
    {
        Scene tupleFiller;
        string tupleFillerName;
        List<SaveObject> oldSaveObjects;
        (tupleFiller, tupleFillerName, oldSaveObjects) = lastSceneStates.Find((pair) => pair.Item1 == scene);

        foreach (SaveObject newSaveObject in newSaveObjects)
        {
            if (!oldSaveObjects.Exists((saveObject) => newSaveObject == saveObject))
            {
                // New save object detected.
                Debug.Log("new save object detected");
                // objects in scenes: rearragning and renaming don't trigger,
                // and while adding works on its own (which it should trigger), removing and readding
                // with control z triggers this when it shouldnt.
            }
        }
    }

    /*
    Function needed to update to new scene list state and detect which scenes are still there since the
    last hierarchy change. (this way we only consider existing states that change in the hierarchy,
    not the change in loading/unloading scenes). When these persistent scenes are found, then
    the parser goes through and sets new save game objects to have an ID of 0.

    Inputs:
    None

    Outputs:
    None
    */
    private void OnHierarchyChanged() // No need to serialize after assigning lastSceneStates, as it is held in edit memory.
    {
        var newSceneStates = GetCurrentHierarchyState();

        if (lastSceneStates != null)
        {
            foreach (var newPair in newSceneStates)
            {
                if (lastSceneStates.Exists((lastPair) => 
                    lastPair.Item1 == newPair.Item1 && lastPair.Item2 == newPair.Item2))
                {
                    // Existing scene still here, may have hierarchy save object change.
                    DetectNewSaveObjects(newPair.Item1, newPair.Item3); 
                }
            }
        }

        lastSceneStates = newSceneStates;
    }
}