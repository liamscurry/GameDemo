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
    public string Save()
    {
        id = 10; // need to mark scene dirty
        EditorUtility.SetDirty(gameObject);
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
        return "save filler return";
    }

    public void Load(string jsonString)
    {

    }
}