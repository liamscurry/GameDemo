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
    private const int unitializedID = 0;

    [HideInInspector]
    [SerializeField]
    private int uniqueIDCounter; // one of the id is being set to -1 on play, only prefab instance.
    // using prefab method, need to use it differently/another method?

    private List<SaveObject> changedObjects;

    private void Awake()
    {
        changedObjects = new List<SaveObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            GenerateIDs();
            Debug.Log("saved");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Load(SceneManager.GetActiveScene().name + "-Save-01");
            Debug.Log("loaded");
        }
    }

    // Finds all save objects correctly.
    [ContextMenu("GenerateIDs")]
    public void GenerateIDs()
    {
        var saveObjects = GetAllSaveObjects();
        List<string> jsonObjects = new List<string>();
        foreach (var saveObject in saveObjects)
        {
            string jsonObject = saveObject.Save(this);
            jsonObjects.Add(jsonObject);
        }

        WriteToSaveFile(SceneManager.GetActiveScene().name + "-Save-01", jsonObjects);
    }

    public void Load(string saveName)
    {
        List<JsonIDPair> jsonIDPairs =
            ReadFromSaveFile(saveName);

        var saveObjects = GetAllSaveObjects();

        foreach (var pair in jsonIDPairs)
        {
            SaveObject matchingIDObject =
                saveObjects.Find(saveObject => saveObject.ID == pair.id);
            if (matchingIDObject != null)
            {
                matchingIDObject.Load(pair.jsonString);
            }
        }
    }

    private List<SaveObject> GetAllSaveObjects()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.GetComponentsInChildren<SaveObject>());
        }
        return saveObjects;
    }

    public int RequestUniqueID()
    {
        int next = uniqueIDCounter++;
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        return next;
    }

    // Force resets all IDs. Used in development of save files only, not used in application.
    [ContextMenu("RegenerateIDs")]
    public void RegenerateIDs()
    {
        uniqueIDCounter = 1;
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        var saveObjects = new List<SaveObject>();
        foreach (GameObject rootObject in rootObjects)
        {
            saveObjects.AddRange(rootObject.GetComponentsInChildren<SaveObject>());
        }
    
        List<string> jsonObjects = new List<string>();
        foreach (var saveObject in saveObjects)
        {
            string jsonObject = saveObject.Save(this, true);
            jsonObjects.Add(jsonObject);
        }

        WriteToSaveFile(activeScene.name + "-Save-01", jsonObjects);
    }

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
            Debug.Log(saveObject.GameObject.name + ": " + saveObject.ID);
        }
    }

    public void ObjectChanged(SaveObject saveObject)
    {
        changedObjects.Add(saveObject);
    }

    // Saves all changed objects to a save file named saveName.
    private void WriteToSaveFile(string saveName, List<string> jsonObjects)
    {
        string cwd = Directory.GetCurrentDirectory();
        string filePath = cwd + "\\Assets\\Save Files\\" + saveName + ".txt";

        using (StreamWriter streamWriter = File.CreateText(filePath))
        {
            foreach (string s in jsonObjects)
            {
                streamWriter.WriteLine(s);
            }
        }
    }

    private List<JsonIDPair> ReadFromSaveFile(string saveName)
    {
        string cwd = Directory.GetCurrentDirectory();
        string filePath = cwd + "\\Assets\\Save Files\\" + saveName + ".txt";

        if (!File.Exists(filePath))
        {
            throw new System.Exception("Save file does not exist: " + filePath);
        }
        else
        {
            var jsonIDPairs = new List<JsonIDPair>();
            string[] jsonObjects = System.IO.File.ReadAllLines(filePath);
            foreach (string jsonObject in jsonObjects)
            {
                int spaceIndex = jsonObject.IndexOf(' ');
                int id = Int32.Parse(jsonObject.Substring(0, spaceIndex));
                string jsonString = jsonObject.Substring(spaceIndex + 1);
                jsonIDPairs.Add(new JsonIDPair(id, jsonString));
            }

            return jsonIDPairs;
        }
    }
}
