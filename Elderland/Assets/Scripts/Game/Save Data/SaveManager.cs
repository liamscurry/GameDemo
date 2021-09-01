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
    private int uniqueIDCounter;

    public string CurrentSave { get; private set; }
    private string autoSaveName = "AutoSave";
    private List<SaveObject> changedObjects;

    private void Start()
    {
        changedObjects = new List<SaveObject>();

        // temp: current save will be selected in main menu.
        StartCoroutine(TestSaveSelect());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save(SceneManager.GetActiveScene().name + "-Save-01");
            Debug.Log("saved");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Load(SceneManager.GetActiveScene().name + "-Save-01");
            Debug.Log("loaded");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerInfo.Manager.ChangeHealth(-1000);
        }
    }

    /*
    Developement function used to load auto save file after everything else is initialized
    */
    private IEnumerator TestSaveSelect()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        SetCurrentSave(autoSaveName);
        Load(autoSaveName);
    }

    /*
    Helper method needed in main menus to set the current save when loading into a specified save

    Inputs:
    string : saveName : name of save file.

    Outputs:
    None
    */
    public void SetCurrentSave(string saveName)
    {
        CurrentSave = saveName;
    }

    /*
    Saves the game to the auto save slot. In addition, sets current save to the auto save
    so that if the player dies, it will load the auto save.

    Inputs:
    None

    Outputs:
    None
    */
    public void AutoSave()
    {
        Save(autoSaveName);
    }

    /*
    Loads the most recent save file.

    Inputs:
    None

    Outputs:
    None
    */
    public void LoadCurrentSave()
    {
        Load(CurrentSave);
    }

    /*
    Takes a snapshot of all changed objects and adds it to the save file.

    Inputs: 
    string : saveName : name of save file.

    Outputs:
    None
    */
    [ContextMenu("Save")]
    public void Save(string saveName)
    {
        var saveObjects = GetAllSaveObjects();
        List<string> jsonObjects = new List<string>();
        foreach (var saveObject in saveObjects)
        {
            string jsonObject = saveObject.Save(this);
            jsonObjects.Add(jsonObject);
        }

        WriteToSaveFile(saveName, jsonObjects);

        CurrentSave = saveName;
    }

    /*
    Loads a save file, changing objects in the scene.

    Inputs:
    string : saveName : name of save file.

    Outputs:
    None
    */
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

    /*
    Generates ids for prefabs/objects in scene. Needs to be called before each build.

    Inputs:
    None

    Outputs:
    None
    */
    [ContextMenu("GenerateIDs")]
    public void GenerateIDs()
    {
        var saveObjects = GetAllSaveObjects();
        foreach (var saveObject in saveObjects)
        {
            saveObject.CheckID(this, false);
        }
    }

    // Force resets all IDs. Used in development of save files only, not used in application.
    [ContextMenu("RegenerateIDs: Development only, wipes saves")]
    public void RegenerateIDs()
    {
        uniqueIDCounter = 1;
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);

        var saveObjects = GetAllSaveObjects();
        foreach (var saveObject in saveObjects)
        {
            saveObject.CheckID(this, true);
        }
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

    /*
    Helper method for load that parses the save file into json id structures.

    Inputs:
    string : saveName : name of save file.

    Outputs:
    List<JsonIDPair> : list of id json objects pairs found in save file.
    */
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
