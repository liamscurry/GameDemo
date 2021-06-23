using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;

// References Microsoft C# file docs.
// unity blog: https://blog.unity.com/technology/persistent-data-how-to-save-your-game-states-and-settings
// JSON utility. JsonSerializer
// unique names by position in scene tree? will have index id on save object as well. last loading
// step will be to re organize the tree if needed.

// Main manager class for saving/loading data.
public class SaveManager : MonoBehaviour
{
    private const int unitializedID = -1;

    [HideInInspector]
    [SerializeField]
    private int uniqueIDCounter;

    private List<SaveObject> changedObjects;

    private void Awake()
    {
        changedObjects = new List<SaveObject>();
        WriteToSaveFile("WriteTest");
    }

    // Finds all save objects correctly.
    [ContextMenu("GenerateIDs")]
    public void GenerateIDs()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.GetComponentsInChildren<SaveObject>());
        }
        foreach (var saveObject in saveObjects)
        {
            saveObject.Save(this);
        }
    }

    public int RequestUniqueID()
    {
        int next = uniqueIDCounter++;
        EditorUtility.SetDirty(gameObject);
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
        return next;
    }

    // Force resets all IDs. Used in development of save files only, not used in application.
    /*
    [ContextMenu("RegenerateIDs")]
    public void RegenerateIDs()
    {
        uniqueIDCounter = 0;
        EditorUtility.SetDirty(gameObject);
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.GetComponentsInChildren<SaveObject>());
        }
        foreach (var saveObject in saveObjects)
        {
            saveObject.Save(this, true);
        }
    }
    */

    [ContextMenu("PrintIDs")]
    public void PrintIDs()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.GetComponentsInChildren<SaveObject>());
        }
        foreach (var saveObject in saveObjects)
        {
            Debug.Log(saveObject.ID);
        }
    }

    public void ObjectChanged(SaveObject saveObject)
    {
        changedObjects.Add(saveObject);
    }

    // Saves all changed objects to a save file named saveName.
    private void WriteToSaveFile(string saveName)
    {
        string cwd = Directory.GetCurrentDirectory();
        string filePath = cwd + "\\Assets\\Save Files\\" + saveName + ".txt";

        if (!File.Exists(saveName))
        {
            using (StreamWriter streamWriter = File.CreateText(filePath))
            {
                streamWriter.WriteLine("Test file: " + saveName);
            }
        }
    }
}
