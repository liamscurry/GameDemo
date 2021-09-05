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
    [HideInInspector]
    private string lastScene;

    [SerializeField]
    [HideInInspector]
    private bool initialized;

    [SerializeField]
    [HideInInspector]
    private SaveManager saveManager;

    private void TryInitialize()
    {
        if (!initialized)
        {
            initialized = true;
            saveManager = GetComponent<SaveManager>();
        }
    }

    private void Update()
    {
        TryInitialize();

        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;

        if (lastSceneStates == null) // Needed to keep track of existing state on compilation for swap detection.
        {
            var newSceneStates = GetCurrentHierarchyState();
            lastSceneStates = newSceneStates;
        }
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
                Scene oldScene;
                bool foundOldScene;
                (oldScene, foundOldScene) = DetectNewSaveObjectSceneSwap(newSaveObject);
                if (!foundOldScene)
                {
                    // New save object detected, reset id, no need to edit save files
                    Debug.Log("new save object detected");
                }
                else
                {
                    SwapSaveObjectFile(newSaveObject, scene, oldScene);
                    Debug.Log("nothing happens, save object swapped scenes");
                }
                
                // objects in scenes: rearragning and renaming don't trigger,
                // and while adding works on its own (which it should trigger), removing and readding
                // with control z triggers this when it shouldnt.
            }
        }
    }

    /*
    Checks if 'new' object detected in DetectNewSaveObjects really is a new object or if it
    was an object in another scene that was moved to this scene. This method assumes the object
    can be in only one scene at a time.

    Inputs: 
    SaveObject : newSaveObject : possible new save object to be checked for scene transfer

    Outputs:
    Scene : scene found if the object scene swapped, generic scene if newSaveObject is an actual new object.
    bool : was an old scene found.
    */
    private (Scene, bool) DetectNewSaveObjectSceneSwap(SaveObject newSaveObject) 
    {
        foreach (var oldPair in lastSceneStates)
        {
            if (oldPair.Item3.Contains(newSaveObject))
            {
                return (oldPair.Item1, true);
            }
        }
    
        return (new Scene(), false);
    }

    /*
    Moves save data of a swapped save object from one scene save file to another save scene file.

    Inputs:
    SaveObject : newSaveObject : the save object to be moved in the save scene files.
    Scene : newScene : the new save scene file the object should reside in.

    Outputs:
    None
    */
    private void SwapSaveObjectFile(SaveObject newSaveObject, Scene newScene, Scene oldScene)
    {
        string oldSaveName = saveManager.GetSceneAutoSaveName(oldScene);
        string newSaveName = saveManager.GetSceneAutoSaveName(newScene);
        saveManager.TransferObjectToSaveFile(newSaveObject, newSaveName, oldSaveName);
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