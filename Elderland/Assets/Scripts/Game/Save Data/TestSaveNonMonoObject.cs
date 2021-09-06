using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

/*
Testing class used in unit tests for SaveManager functions.
*/
public sealed class TestSaveNonMonoObject : SaveObject
{
    public GameObject GameObject { get { return null; } }

    [HideInInspector]
    [SerializeField]
    private int id;
    public int ID { get { return id; } }

    public TestSaveNonMonoObject(int id)
    {
        this.id = id;
    }

    public string Save(SaveManager saveManager, bool resetSave = false)
    {
        return ID + " " + "{\"test\":" + ID + "}";
    }

    public void Load(string jsonString) {}
    public void CheckID(SaveManager saveManager, bool resetSave) {}
    public void PrintID() {}
}