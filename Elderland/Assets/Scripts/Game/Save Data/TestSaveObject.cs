using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class TestSaveObject : MonoBehaviour, SaveObject
{
    [HideInInspector]
    [SerializeField]
    private int id = -1;
    public int ID { get { return id; } set { id = value; } }

    public string Save(SaveManager saveManager, bool resetSave = false)
    {
        if (id == -1 || resetSave)
            id = saveManager.RequestUniqueID();
        EditorUtility.SetDirty(gameObject);
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
        return "save filler return";
    }

    public void Load(string jsonString)
    {

    }
}